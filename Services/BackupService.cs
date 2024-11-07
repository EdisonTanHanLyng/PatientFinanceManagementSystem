using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using PFMS_MI04.Hubs;
using PFMS_MI04.Models;
using System.Collections.Generic;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PFMS_MI04.Services
{
    public class BackupService : BackgroundService
    {
        private readonly ILogger<BackupService> _logger;

        private static string today = DateTime.Now.ToString("yyyy-MM-dd_hh-mm_tt_");

        private static string[] constring = {
            "Server=88.222.244.88;Port=3306;User ID=user;Password=mi04Curtin@2024#;",
            "Server=88.222.244.88;Port=3306;Database=documents_schema;User ID=user;Password=mi04Curtin@2024#;",
            "Server=88.222.244.88;Port=3306;Database=account_schema;User ID=user;Password=mi04Curtin@2024#;",
            "Server=88.222.244.88;Port=3306;Database=reminder_schema;User ID=user;Password=mi04Curtin@2024#;",
            "Server=88.222.244.88;Port=3306;Database=sec_logging;User ID=user;Password=mi04Curtin@2024#;",
            "Server=88.222.244.88;Port=3306;Database=backup_schema;User ID=user;Password=mi04Curtin@2024#;",
            "Server=88.222.244.88;Port=3306;Database=pricing_schema;User ID=user;Password=mi04Curtin@2024#;",
        };

        private static string path = "/var/mi04_2024_backups/";

        //private static string path = "\\var\\mi04_2024_backups";
        private static string[] file = {
            path + today + "full/",
            path + today + "document/document_schema.sql",
            path + today + "account/account_schema.sql",
            path + today + "reminder/reminder_schema.sql",
            path + today + "sec_logging/sec_log.sql",
            path + today + "backup/backup_schema.sql",
            path + today + "pricing/pricing_schema.sql",
        };

        public BackupService(ILogger<BackupService> logger)
        {
            _logger = logger;

            if (!Directory.Exists(path))
            {
                // Create the directory if it doesn't exist
                Directory.CreateDirectory(path);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            DateTime lastCheckedDate = await getLastTimeStamp(); // Example date

            while (!stoppingToken.IsCancellationRequested)
            {
                // Your logic to check if it's been a week
                if (lastCheckedDate <= DateTime.Now.AddDays(-7))
                {
                    BackupDatabase_Full("This is an automated weekly full backup.");
                    updateNewTimeStamp();
                }

                // Wait for 1 hour before checking again
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task<DateTime> getLastTimeStamp()
        {
            DateTime lastLoggedDate = DateTime.MinValue;

            using (MySqlConnection conn = new MySqlConnection(constring[5]))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    await conn.OpenAsync();
                    string query = @"SELECT * FROM weekly_log ORDER BY created_at DESC LIMIT 1";


                    using (var command = new MySqlCommand(query, conn))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                lastLoggedDate = reader.GetDateTime("created_at");
                            }
                        }
                    }

                }
            }

            if (lastLoggedDate == DateTime.MinValue)
            {
                lastLoggedDate = DateTime.Now; 
                updateNewTimeStamp();
            }

            return lastLoggedDate;
        }

        private async void updateNewTimeStamp()
        {
            using (MySqlConnection conn = new MySqlConnection(constring[5]))
            {
                conn.Open();
                string query = @"INSERT INTO weekly_log (log_date) VALUES (@date)";
                using (var command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@date", DateTime.Now);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        //Backup operations
        internal void BackupDatabase_Full(string remarks)
        {
            using (MySqlConnection conn = new MySqlConnection(constring[0]))
            {
                conn.Open();

                // Fetch the list of databases
                using (MySqlCommand cmd = new MySqlCommand("SHOW DATABASES;", conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string databaseName = reader.GetString(0);

                            // Skip system databases like 'mysql', 'performance_schema', etc.
                            if (databaseName == "information_schema" || databaseName == "mysql" ||
                                databaseName == "performance_schema" || databaseName == "sys")
                            {
                                continue;
                            }

                            //Console.WriteLine($"Backing up database: {databaseName}");

                            // Backup the current database
                            BackupDatabase(databaseName, constring[0], file[0]);
                        }
                    }
                }

                conn.Close();
            }

            logBackupAsync(file[0], "Full", remarks, path);
        }

        private void BackupDatabase(string databaseName, string connectionString, string backupDirectory)
        {
            string backupFilePath = Path.Combine(backupDirectory, $"{databaseName}_backup.sql");

            // Create a new connection for each database backup
            string dbConnectionString = $"{connectionString}Database={databaseName};";
            using (MySqlConnection conn = new MySqlConnection(dbConnectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        mb.ExportToFile(backupFilePath);
                    }
                }

                conn.Close();
            }
        }

        internal void BackupDatabase_Document(string remarks)
        {
            using (MySqlConnection conn = new MySqlConnection(constring[1]))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(file[1]);
                        conn.Close();
                    }
                }
            }

            logBackupAsync(file[1], "Documents", remarks, path);
        }

        internal void BackupDatabase_Account(string remarks)
        {
            using (MySqlConnection conn = new MySqlConnection(constring[2]))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(file[2]);
                        conn.Close();
                    }
                }
            }

            logBackupAsync(file[2], "Account", remarks, path);
        }

        internal void BackupDatabase_Reminder(string remarks)
        {
            using (MySqlConnection conn = new MySqlConnection(constring[3]))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(file[3]);
                        conn.Close();
                    }
                }
            }

            logBackupAsync(file[3], "Reminders", remarks, path);
        }

        internal void BackupDatabase_Sec_Logging(string remarks)
        {
            using (MySqlConnection conn = new MySqlConnection(constring[4]))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(file[4]);
                        conn.Close();
                    }
                }
            }

            logBackupAsync(file[4], "Security Logging", remarks, path);
        }

        internal void BackupDatabase_Backups(string remarks)
        {
            using (MySqlConnection conn = new MySqlConnection(constring[5]))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(file[5]);
                        conn.Close();
                    }
                }
            }

            logBackupAsync(file[5], "Backups", remarks, path);
        }

        internal void BackupDatabase_Pricings(string remarks)
        {
            using (MySqlConnection conn = new MySqlConnection(constring[6]))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(file[6]);
                        conn.Close();
                    }
                }
            }
            Console.WriteLine(file[6]);
            logBackupAsync(file[5], "Pricings", remarks, path);
        }

        //insert backup logs
        internal async void logBackupAsync(string fileName, string type, string remarks, string filePath)
        {
            using (MySqlConnection conn = new MySqlConnection(constring[5]))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    string query = @"INSERT INTO backups (backup_date, backup_type, backup_name, path, remarks) 
                                        VALUES (@date, @type, @name, @filePath, @remarks)";

                    using (var command = new MySqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@date", DateTime.Now);
                        command.Parameters.AddWithValue("@type", type);
                        command.Parameters.AddWithValue("@name", fileName);
                        command.Parameters.AddWithValue("@filePath", filePath);
                        command.Parameters.AddWithValue("@remarks", remarks);

                        conn.Open();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        //retrive backup logs
        internal async Task<List<BackupDetails>> getBackupLogs()
        {
            List<BackupDetails> logs = new List<BackupDetails>();

            using (MySqlConnection conn = new MySqlConnection(constring[5]))
            {
                await conn.OpenAsync();

                string query = @"SELECT backup_date, backup_type, remarks FROM backups WHERE backup_date >= NOW() - INTERVAL 6 MONTH ORDER BY backup_date DESC";

                using (var command = new MySqlCommand(query, conn))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            DateTime date = reader.GetDateTime(0);
                            string type = reader.GetString(1);
                            string remarks = reader.IsDBNull(reader.GetOrdinal("remarks")) ? "" : reader.GetString("remarks");

                            logs.Add(new BackupDetails(date, type, remarks));
                        }
                    }
                }
            }

            return logs;
        }

        internal async Task<List<BackupDetails>> getNextBackupLogs(DateTime lastFetchedDate)
        {
            List<BackupDetails> logs = new List<BackupDetails>();

            using (MySqlConnection conn = new MySqlConnection(constring[5]))
            {
                await conn.OpenAsync();

                string query = @"SELECT backup_date, backup_type, remarks FROM backups 
                         WHERE backup_date < @lastFetchedDate AND backup_date >= @lastFetchedDate - INTERVAL 6 MONTH 
                         ORDER BY backup_date DESC";

                using (var command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@lastFetchedDate", lastFetchedDate);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            // No more data to fetch, return an empty list
                            return logs;
                        }

                        while (await reader.ReadAsync())
                        {
                            DateTime date = reader.GetDateTime(0);
                            string type = reader.GetString(1);
                            string remarks = reader.IsDBNull(reader.GetOrdinal("remarks")) ? "" : reader.GetString("remarks");

                            logs.Add(new BackupDetails(date, type, remarks));
                        }
                    }
                }
            }

            return logs;
        }


        //restore function for specific database/schema
        //public static void RestoreDatabase_Reminder()
        //{
        //    string connstr = "Server=localhost;user=root;Password=KuroTSUki48&;Database=reminder_schema;";
        //    string path = "D:\\Assesment\\CCP2\\code\\testing\\backup_files\\2024-09-16_09-26_PM_reminder\\reminder_schema_backup.sql";

        //    using (MySqlConnection conn = new MySqlConnection(connstr))
        //    {
        //        using (MySqlCommand cmd = new MySqlCommand())
        //        {
        //            using (MySqlBackup mb = new MySqlBackup(cmd))
        //            {
        //                cmd.Connection = conn;
        //                conn.Open();
        //                using (MySqlCommand command = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS reminder_schema;", conn))
        //                {
        //                    mb.ImportFromFile(path);
        //                }
        //                conn.Close();
        //            }
        //        }
        //    }

        //    Console.WriteLine("Reminder Backup completed!");
        //}

        //restore function type 2
        //private static string connstr = "Server=localhost;user=root;Password=KuroTSUki48&;";
        //private static string dir = "D:\\Assesment\\CCP2\\code\\testing\\backup_files\\2024-09-19_04-46_PM_full\\";

        //public static void RestoreDatabases_ChooseFile()
        //{
        //    string[] backupFiles = Directory.GetFiles(dir, "*.sql");

        //    foreach (string backupFilePath in backupFiles)
        //    {
        //        string fileName = Path.GetFileNameWithoutExtension(backupFilePath);
        //        string databaseName = fileName.Replace("_backup", "");

        //        Console.WriteLine($"Restoring database: {databaseName} from {backupFilePath}");

        //        // Create the database if it doesn't exist
        //        CreateDatabaseIfNotExists(databaseName);

        //        // Restore the database from the backup file
        //        RestoreDatabase(databaseName, backupFilePath);
        //    }

        //    Console.WriteLine("Database restore completed!");
        //}

        //private static void CreateDatabaseIfNotExists(string databaseName)
        //{
        //    using (MySqlConnection conn = new MySqlConnection(connstr))
        //    {
        //        conn.Open();
        //        using (MySqlCommand cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{databaseName}`;", conn))
        //        {
        //            cmd.ExecuteNonQuery();
        //            Console.WriteLine($"Database '{databaseName}' created or already exists.");
        //        }
        //        conn.Close();
        //    }
        //}

        //private static void RestoreDatabase(string databaseName, string backupFilePath)
        //{
        //    string dbConnectionString = $"{connstr}Database={databaseName};";

        //    using (MySqlConnection conn = new MySqlConnection(dbConnectionString))
        //    {
        //        conn.Open();

        //        using (MySqlCommand cmd = new MySqlCommand())
        //        {
        //            cmd.Connection = conn;

        //            using (MySqlBackup mb = new MySqlBackup(cmd))
        //            {
        //                mb.ImportFromFile(backupFilePath);
        //                Console.WriteLine($"Restore of {databaseName} completed from: {backupFilePath}");
        //            }
        //        }

        //        conn.Close();
        //    }
        //}
    }
}
