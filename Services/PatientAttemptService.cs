using MySql.Data.MySqlClient;
using PFMS_MI04.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace PFMS_MI04.Services
{
    public class PatientAttemptService
    {
        private static string connectionString = "Server=88.222.244.88;Port=3306;Database=reminder_schema;User ID=user;Password=mi04Curtin@2024#;";

        public static List<PatientsAttempsModel> GetAllPatientAttemptList()
        {
            List<PatientsAttempsModel> attempt_patient_List = new List<PatientsAttempsModel>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM reminder_log ORDER BY id DESC;";

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PatientsAttempsModel item = new PatientsAttempsModel();
                                item.ID = reader["user_name"].ToString();
                                item.Name = reader["email"].ToString();
                                item.Status = reader["reminder_status"].ToString();

                                DateTime dueDateTime = (DateTime)reader["date_added"];
                                item.Date = dueDateTime.ToString("MM/dd/yyyy");

                                attempt_patient_List.Add(item);
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occured when trying to fetch from Database: " + ex.Message);
            }

            return attempt_patient_List;
        }

        public static List<PatientsAttempsModel> GetAllPatientAttemptListMain()
        {
            List<PatientsAttempsModel> attempt_patient_List = new List<PatientsAttempsModel>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM reminder_log ORDER BY id DESC LIMIT 8;";

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PatientsAttempsModel item = new PatientsAttempsModel();
                                item.ID = reader["user_name"].ToString();
                                item.Name = reader["email"].ToString();
                                item.Status = reader["reminder_status"].ToString();

                                DateTime dueDateTime = (DateTime)reader["date_added"];
                                item.Date = dueDateTime.ToString("MM/dd/yyyy");

                                attempt_patient_List.Add(item);
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occured when trying to fetch from Database: " + ex.Message);
            }

            return attempt_patient_List;
        }

    }
}
