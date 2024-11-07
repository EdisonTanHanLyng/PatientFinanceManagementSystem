using MySql.Data.MySqlClient;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Security;
using System.Runtime.InteropServices.Marshalling;

namespace PFMS_MI04.Services
{
    public class SecLoggingService
    {
        private static readonly string connString = "Server=88.222.244.88;Port=3306;Database=sec_logging;User ID=user;Password=mi04Curtin@2024#;";

        // ----- Wrapper Functions ----- //
        public List<SecLog> getSecLogs()
        {
            return getSecurityLogs();
        }

        public bool uploadSecLog(SecLog log)
        {
            bool res = false;

            if (log.isReady())
            {
                uploadSecurityLog(log);
                res = true;
            }
            return res;
        }

        public bool uploadSecLog(List<SecLog> batch)
        {
            List<SecLog> logs = new List<SecLog>(batch);
            StrComparator.println("Batch Data Upload: " + logs.Count);
            uploadSecurityLog(logs);
            return true;
        }

        // ----- SQL Connection functions ----- //

        // Referenced from: ReminderService (Edison)
        // Get All security logs (Not going to be used)
        private List<SecLog> getSecurityLogs()
        {
            List<SecLog> secLogs = new List<SecLog>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    using (MySqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM sec_log";
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Dont know if reading is necessary for Security logs as of yet
                            }
                        }
                    }
                }
            }
            catch (MySqlException mqEx)
            {
                StrComparator.println(mqEx.Message);
            }
            return secLogs;
        }

        //Referenced from: ReminderService (Edison)
        // Inserts a single SecLog entry into the database
        private async Task<bool> uploadSecurityLog(SecLog log)
        {
            bool inserted = false;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    await conn.OpenAsync();
                    MySqlCommand command = conn.CreateCommand();
                    command = secCommandBuilder(command, log);
                    using (command)
                    {
                        int rows = await command.ExecuteNonQueryAsync();
                        if (rows > 0) { inserted = true; }
                    }
                    await conn.CloseAsync();
                }
            }
            catch (MySqlException mqEx)
            {
                StrComparator.println(mqEx.Message);
            }
            catch (Exception eX)
            {
                StrComparator.println(eX.Message);
            }
            return inserted;
        }

        // Inserts a List of SecLogs into the database in bulk
        private async Task<bool> uploadSecurityLog(List<SecLog> batch)
        {
            bool inserted = false;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    await conn.OpenAsync();
                    foreach (SecLog log in batch)
                    {
                        if (log.isReady())
                        {
                            MySqlCommand command = conn.CreateCommand();
                            command = secCommandBuilder(command, log);
                            using (command)
                            {
                                int rows = await command.ExecuteNonQueryAsync();
                                if (rows > 0) { inserted = true; }
                            }
                        }
                    }
                    if (inserted)
                    {
                        Console.WriteLine("Security: Uploaded " + batch.Count + " Logs");
                    }
                }
            }
            catch (MySqlException mqEx)
            {
                StrComparator.println(mqEx.Message);
            }
            catch(Exception eX)
            {
                StrComparator.println(eX.Message);
            }
            batch.Clear();
            return inserted;
        }

        // ----- Internal Data Processing ----- //

        // Builds the query string for a single SecLog entry
        private MySqlCommand secCommandBuilder(MySqlCommand command, SecLog log)
        {
            try
            {
                command.CommandText = @"
                    INSERT INTO sec_log(userID, atPage, atFunc, atFile, atTime, atDate, logType, activityMsg, blocker)
                    VALUES (@userID, @atPage, @atFunc , @atFile, @atTime, @atDate, @logType, @activityMsg, @blocker);
                ";

                command.Parameters.AddWithValue("@userID", log.userID);
                command.Parameters.AddWithValue("@atPage", log.atPage);
                command.Parameters.AddWithValue("@atFunc", log.atFunc);
                command.Parameters.AddWithValue("@atFile", log.atFile);
                command.Parameters.AddWithValue("@atTime", log.atTime);
                command.Parameters.AddWithValue("@atDate", log.atDate);
                command.Parameters.AddWithValue("@logType", log.logType);
                command.Parameters.AddWithValue("@activityMsg", log.activityMsg);
                command.Parameters.AddWithValue("@blocker", log.blocker);

            }
            catch (MySqlException mqEx)
            {
                StrComparator.println(mqEx.Message);
            }
            return command;
        }
    }
}
