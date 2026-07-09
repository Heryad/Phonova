using System;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;

namespace Phonova.Services
{
    public class LoginResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("stationName")]
        public string StationName { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public static class ApiService
    {
        private static readonly HttpClient _httpClient;
        public static string CurrentToken { get; private set; }
        public static string Username { get; set; }
        public static ClientConfigModel? CurrentConfig { get; set; }

        static ApiService()
        {
            _httpClient = new HttpClient();
            // Load base address dynamically from global settings
            _httpClient.BaseAddress = new Uri(SettingsManager.Current.ApiBaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static void Logout()
        {
            SetToken(null);
        }

        public static void SetToken(string token)
        {
            CurrentToken = token;
            if (string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public static string GetHardwareId()
        {
            string cpuInfo = string.Empty;
            string moboInfo = string.Empty;

            try
            {
                using (ManagementClass mc = new ManagementClass("win32_processor"))
                {
                    ManagementObjectCollection moc = mc.GetInstances();
                    foreach (ManagementObject mo in moc)
                    {
                        if (cpuInfo == string.Empty)
                        {
                            cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                        }
                    }
                }

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                {
                    ManagementObjectCollection moc = searcher.Get();
                    foreach (ManagementObject mo in moc)
                    {
                        moboInfo = mo["SerialNumber"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting hardware ID: {ex.Message}");
                // Fallback hardware ID if WMI fails
                return Environment.MachineName + "_FallbackID";
            }

            string combined = cpuInfo + moboInfo;
            if (string.IsNullOrEmpty(combined)) return Environment.MachineName + "_FallbackID";

            // Simple hash
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static async Task<LoginResponse> LoginAsync(string companyEmail, string username, string password)
        {
            var payload = new
            {
                companyEmail,
                username,
                password,
                hardwareId = GetHardwareId(),
                stationName = Environment.MachineName
            };

            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/desktop/auth/login", content);
                string responseStr = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var loginData = JsonConvert.DeserializeObject<LoginResponse>(responseStr);
                    if (loginData != null && !string.IsNullOrEmpty(loginData.Token))
                    {
                        SetToken(loginData.Token);
                    }
                    return loginData;
                }
                else
                {
                    var errorData = JsonConvert.DeserializeObject<LoginResponse>(responseStr);
                    return errorData ?? new LoginResponse { Error = "Unknown error occurred during login." };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse { Error = $"Network error: {ex.Message}" };
            }
        }

        // Generic wrapper for API calls to handle tokens and JSON serialization
        private static async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API GET Error on {endpoint}: {ex.Message}");
            }
            return default(T);
        }

        private static async Task<bool> PostAsync(string endpoint, object data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API POST Error on {endpoint}: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> PutAsync(string endpoint, object data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(endpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API PUT Error on {endpoint}: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API DELETE Error on {endpoint}: {ex.Message}");
                return false;
            }
        }

        // --- Comments ---
        public class CommentModel { public string id { get; set; } public string content { get; set; } }
        public class CommentsResponse { public System.Collections.Generic.List<CommentModel> comments { get; set; } }
        
        public static async Task<System.Collections.Generic.List<CommentModel>> GetCommentsAsync()
        {
            var res = await GetAsync<CommentsResponse>("/desktop/comments");
            return res?.comments ?? new System.Collections.Generic.List<CommentModel>();
        }
        public static async Task<bool> AddCommentAsync(string content) => await PostAsync("/desktop/comments", new { content });
        public static async Task<bool> UpdateCommentAsync(string id, string content) => await PutAsync($"/desktop/comments/{id}", new { content });
        public static async Task<bool> DeleteCommentAsync(string id) => await DeleteAsync($"/desktop/comments/{id}");

        // --- MMR Comments ---
        public class MmrCommentsResponse { public System.Collections.Generic.List<CommentModel> mmrComments { get; set; } }
        
        public static async Task<System.Collections.Generic.List<CommentModel>> GetMmrCommentsAsync()
        {
            var res = await GetAsync<MmrCommentsResponse>("/desktop/mmr-comments");
            return res?.mmrComments ?? new System.Collections.Generic.List<CommentModel>();
        }
        public static async Task<bool> AddMmrCommentAsync(string content) => await PostAsync("/desktop/mmr-comments", new { content });
        public static async Task<bool> UpdateMmrCommentAsync(string id, string content) => await PutAsync($"/desktop/mmr-comments/{id}", new { content });
        public static async Task<bool> DeleteMmrCommentAsync(string id) => await DeleteAsync($"/desktop/mmr-comments/{id}");

        // --- Customers ---
        public class CustomerModel { public string id { get; set; } public string name { get; set; } public string phone { get; set; } }
        public class CustomersResponse { public System.Collections.Generic.List<CustomerModel> customers { get; set; } }

        public static async Task<System.Collections.Generic.List<CustomerModel>> GetCustomersAsync()
        {
            var res = await GetAsync<CustomersResponse>("/desktop/customers");
            return res?.customers ?? new System.Collections.Generic.List<CustomerModel>();
        }
        public static async Task<bool> AddCustomerAsync(string name, string phone = "") => await PostAsync("/desktop/customers", new { name, phone });
        public static async Task<bool> UpdateCustomerAsync(string id, string name, string phone = "") => await PutAsync($"/desktop/customers/{id}", new { name, phone });
        public static async Task<bool> DeleteCustomerAsync(string id) => await DeleteAsync($"/desktop/customers/{id}");

        // --- Config / Sync ---
        public class ClientConfigModel
        {
            public string companyName { get; set; } = string.Empty;
            public string? logoUrl { get; set; }
            public int fuel { get; set; }
            public bool isUnlimitedTesting { get; set; }
            public string? unlimitedTestingEndDate { get; set; }
            public int maxConcurrentDevices { get; set; }
            public bool canDoMMR { get; set; }
            public bool canDoRecovery { get; set; }
            public bool canDoBlacklistCheck { get; set; }
            public bool canDoSimLockCheck { get; set; }
            public bool canDoMDMCheck { get; set; }
            public bool canFlashSoftware { get; set; }
        }

        public class ConfigResponse
        {
            [JsonProperty("client")]
            public ClientConfigModel Client { get; set; }
        }

        public static async Task<ConfigResponse?> GetConfigAsync()
        {
            return await GetAsync<ConfigResponse>("/desktop/sync/config");
        }
    }
}
