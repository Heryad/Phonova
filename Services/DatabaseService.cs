using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Data.SQLite;

namespace Phonova.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public DatabaseService()
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Phonova");
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
            
            _dbPath = Path.Combine(appData, "phonova.db");
            _connectionString = $"Data Source={_dbPath};Version=3;";
            
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                // processed_devices table
                string createDevicesTable = @"
                    CREATE TABLE IF NOT EXISTS processed_devices (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        device_name TEXT,
                        model TEXT,
                        color TEXT,
                        storage TEXT,
                        serial TEXT,
                        imei TEXT,
                        icloud TEXT,
                        fmip TEXT,
                        sim TEXT,
                        mdm TEXT,
                        battery_health TEXT,
                        battery_cycles TEXT,
                        kernel_tests TEXT,
                        app_tests TEXT,
                        comments TEXT,
                        ios_version TEXT,
                        region TEXT,
                        date_time DATETIME
                    );";

                // comments lookup table
                string createCommentsTable = @"
                    CREATE TABLE IF NOT EXISTS comments (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        comment_title TEXT UNIQUE,
                        date_time DATETIME
                    );";

                // mmr_comments lookup table
                string createMmrCommentsTable = @"
                    CREATE TABLE IF NOT EXISTS mmr_comments (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        comment_title TEXT UNIQUE,
                        date_time DATETIME
                    );";

                // customers lookup table
                string createCustomersTable = @"
                    CREATE TABLE IF NOT EXISTS customers (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT UNIQUE,
                        date_time DATETIME
                    );";

                // testers lookup table
                string createTestersTable = @"
                    CREATE TABLE IF NOT EXISTS testers (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT UNIQUE,
                        date_time DATETIME
                    );";

                using (var command = new SQLiteCommand(createDevicesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createCommentsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createMmrCommentsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createCustomersTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createTestersTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Migration: Check and add battery columns if missing
                AddColumnIfNotExists(connection, "processed_devices", "battery_health", "TEXT");
                AddColumnIfNotExists(connection, "processed_devices", "battery_cycles", "TEXT");
                AddColumnIfNotExists(connection, "processed_devices", "ios_version", "TEXT");
                AddColumnIfNotExists(connection, "processed_devices", "region", "TEXT");
                AddColumnIfNotExists(connection, "processed_devices", "customer", "TEXT");
                AddColumnIfNotExists(connection, "processed_devices", "tester", "TEXT");
            }
        }

        private void AddColumnIfNotExists(SQLiteConnection connection, string tableName, string columnName, string columnType)
        {
            try
            {
                using (var command = new SQLiteCommand($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType};", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (SQLiteException ex)// 1 is usually "duplicate column name" in this context
            {
                // Column likely already exists
            }
        }

        public void SaveProcessedDevice(ProcessedDevice device)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string insertSql = @"
                    INSERT INTO processed_devices (
                        device_name, model, color, storage, serial, imei, 
                        icloud, fmip, sim, mdm, battery_health, battery_cycles, kernel_tests, app_tests, comments, ios_version, region, customer, tester, date_time
                    ) VALUES (
                        @device_name, @model, @color, @storage, @serial, @imei, 
                        @icloud, @fmip, @sim, @mdm, @battery_health, @battery_cycles, @kernel_tests, @app_tests, @comments, @ios_version, @region, @customer, @tester, @date_time
                    );";

                using (var command = new SQLiteCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@device_name", device.DeviceName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@model", device.Model ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@color", device.Color ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@storage", device.Storage ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@serial", device.Serial ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@imei", device.Imei ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@icloud", device.IcloudStatus ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@fmip", device.FmiStatus ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@sim", device.SimStatus ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@mdm", device.MdmStatus ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@battery_health", device.BatteryHealth ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@battery_cycles", device.BatteryCycles ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@kernel_tests", JsonSerializer.Serialize(device.KernelTests));
                    command.Parameters.AddWithValue("@app_tests", JsonSerializer.Serialize(device.AppTests));
                    command.Parameters.AddWithValue("@comments", JsonSerializer.Serialize(device.Comments));
                    command.Parameters.AddWithValue("@ios_version", device.IosVersion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@region", device.Region ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@customer", device.Customer ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@tester", device.Tester ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@date_time", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddCommentToLibrary(string title)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string insertSql = "INSERT OR IGNORE INTO comments (comment_title, date_time) VALUES (@title, @date);";
                using (var command = new SQLiteCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@date", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<ProcessedDevice> GetProcessedDevices(string? searchQuery = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var result = new List<ProcessedDevice>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM processed_devices WHERE 1=1";
                
                if (!string.IsNullOrWhiteSpace(searchQuery))
                    query += " AND (imei LIKE @search OR serial LIKE @search OR device_name LIKE @search)";
                
                if (startDate.HasValue)
                    query += " AND date_time >= @start";
                
                if (endDate.HasValue)
                    query += " AND date_time <= @end";

                query += " ORDER BY date_time DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    if (!string.IsNullOrWhiteSpace(searchQuery))
                        command.Parameters.AddWithValue("@search", $"%{searchQuery}%");
                    
                    if (startDate.HasValue)
                        command.Parameters.AddWithValue("@start", startDate.Value);
                    
                    if (endDate.HasValue)
                        command.Parameters.AddWithValue("@end", endDate.Value);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new ProcessedDevice
                            {
                                DeviceName = reader.IsDBNull(reader.GetOrdinal("device_name")) ? null : reader.GetString(reader.GetOrdinal("device_name")),
                                Model = reader.IsDBNull(reader.GetOrdinal("model")) ? null : reader.GetString(reader.GetOrdinal("model")),
                                Color = reader.IsDBNull(reader.GetOrdinal("color")) ? null : reader.GetString(reader.GetOrdinal("color")),
                                Storage = reader.IsDBNull(reader.GetOrdinal("storage")) ? null : reader.GetString(reader.GetOrdinal("storage")),
                                Serial = reader.IsDBNull(reader.GetOrdinal("serial")) ? null : reader.GetString(reader.GetOrdinal("serial")),
                                Imei = reader.IsDBNull(reader.GetOrdinal("imei")) ? null : reader.GetString(reader.GetOrdinal("imei")),
                                IcloudStatus = reader.IsDBNull(reader.GetOrdinal("icloud")) ? null : reader.GetString(reader.GetOrdinal("icloud")),
                                FmiStatus = reader.IsDBNull(reader.GetOrdinal("fmip")) ? null : reader.GetString(reader.GetOrdinal("fmip")),
                                SimStatus = reader.IsDBNull(reader.GetOrdinal("sim")) ? null : reader.GetString(reader.GetOrdinal("sim")),
                                MdmStatus = reader.IsDBNull(reader.GetOrdinal("mdm")) ? null : reader.GetString(reader.GetOrdinal("mdm")),
                                BatteryHealth = reader.IsDBNull(reader.GetOrdinal("battery_health")) ? null : reader.GetString(reader.GetOrdinal("battery_health")),
                                BatteryCycles = reader.IsDBNull(reader.GetOrdinal("battery_cycles")) ? null : reader.GetString(reader.GetOrdinal("battery_cycles")),
                                KernelTests = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.GetString(reader.GetOrdinal("kernel_tests"))) ?? new(),
                                AppTests = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.GetString(reader.GetOrdinal("app_tests"))) ?? new(),
                                Comments = JsonSerializer.Deserialize<List<string>>(reader.GetString(reader.GetOrdinal("comments"))) ?? new(),
                                IosVersion = reader.IsDBNull(reader.GetOrdinal("ios_version")) ? null : reader.GetString(reader.GetOrdinal("ios_version")),
                                Region = reader.IsDBNull(reader.GetOrdinal("region")) ? null : reader.GetString(reader.GetOrdinal("region")),
                                Customer = reader.IsDBNull(reader.GetOrdinal("customer")) ? null : reader.GetString(reader.GetOrdinal("customer")),
                                Tester = reader.IsDBNull(reader.GetOrdinal("tester")) ? null : reader.GetString(reader.GetOrdinal("tester")),
                                DateTime = reader.GetDateTime(reader.GetOrdinal("date_time"))
                            });
                        }
                    }
                }
            }
            return result;
        }

        public void DeleteCommentFromLibrary(string title)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string deleteSql = "DELETE FROM comments WHERE comment_title = @title;";
                using (var command = new SQLiteCommand(deleteSql, connection))
                {
                    command.Parameters.AddWithValue("@title", title);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetAllComments()
        {
            var result = new List<string>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string selectSql = "SELECT comment_title FROM comments ORDER BY comment_title ASC;";
                using (var command = new SQLiteCommand(selectSql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return result;
        }

        public void AddCustomerToLibrary(string name)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string insertSql = "INSERT OR IGNORE INTO customers (name, date_time) VALUES (@name, @date);";
                using (var command = new SQLiteCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@date", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteCustomerFromLibrary(string name)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string deleteSql = "DELETE FROM customers WHERE name = @name;";
                using (var command = new SQLiteCommand(deleteSql, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateCustomerInLibrary(string oldName, string newName)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string updateSql = "UPDATE customers SET name = @newName WHERE name = @oldName;";
                using (var command = new SQLiteCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@newName", newName);
                    command.Parameters.AddWithValue("@oldName", oldName);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetAllCustomers()
        {
            var result = new List<string>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string selectSql = "SELECT name FROM customers ORDER BY name ASC;";
                using (var command = new SQLiteCommand(selectSql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return result;
        }

        public void AddTesterToLibrary(string name)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string insertSql = "INSERT OR IGNORE INTO testers (name, date_time) VALUES (@name, @date);";
                using (var command = new SQLiteCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@date", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteTesterFromLibrary(string name)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string deleteSql = "DELETE FROM testers WHERE name = @name;";
                using (var command = new SQLiteCommand(deleteSql, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateTesterInLibrary(string oldName, string newName)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string updateSql = "UPDATE testers SET name = @newName WHERE name = @oldName;";
                using (var command = new SQLiteCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@newName", newName);
                    command.Parameters.AddWithValue("@oldName", oldName);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetAllTesters()
        {
            var result = new List<string>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string selectSql = "SELECT name FROM testers ORDER BY name ASC;";
                using (var command = new SQLiteCommand(selectSql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return result;
        }

        public void AddMmrCommentToLibrary(string title)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string insertSql = "INSERT OR IGNORE INTO mmr_comments (comment_title, date_time) VALUES (@title, @date);";
                using (var command = new SQLiteCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@date", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteMmrCommentFromLibrary(string title)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string deleteSql = "DELETE FROM mmr_comments WHERE comment_title = @title;";
                using (var command = new SQLiteCommand(deleteSql, connection))
                {
                    command.Parameters.AddWithValue("@title", title);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateMmrCommentInLibrary(string oldTitle, string newTitle)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string updateSql = "UPDATE mmr_comments SET comment_title = @newTitle WHERE comment_title = @oldTitle;";
                using (var command = new SQLiteCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@newTitle", newTitle);
                    command.Parameters.AddWithValue("@oldTitle", oldTitle);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetAllMmrComments()
        {
            var result = new List<string>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string selectSql = "SELECT comment_title FROM mmr_comments ORDER BY comment_title ASC;";
                using (var command = new SQLiteCommand(selectSql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return result;
        }
    }

    public class ProcessedDevice
    {
        public string? DeviceName { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public string? Storage { get; set; }
        public string? Serial { get; set; }
        public string? Imei { get; set; }
        public string? IcloudStatus { get; set; }
        public string? FmiStatus { get; set; }
        public string? SimStatus { get; set; }
        public string? MdmStatus { get; set; }
        public string? BatteryHealth { get; set; }
        public string? BatteryCycles { get; set; }
        public string? ProductType { get; set; }
        public string? EnclosureCode { get; set; }
        public string? IosVersion { get; set; }
        public string? Region { get; set; }
        public Dictionary<string, string> KernelTests { get; set; } = new();
        public Dictionary<string, string> AppTests { get; set; } = new();
        public List<string> Comments { get; set; } = new();
        public string? Customer { get; set; }
        public string? Tester { get; set; }
        public DateTime DateTime { get; set; }
    }
}

