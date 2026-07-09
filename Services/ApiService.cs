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
                    // If error like 401, 403, 404
                    var errorData = JsonConvert.DeserializeObject<LoginResponse>(responseStr);
                    return errorData ?? new LoginResponse { Error = "Unknown error occurred during login." };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse { Error = $"Network error: {ex.Message}" };
            }
        }
    }
}
