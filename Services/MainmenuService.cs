using MySql.Data.MySqlClient;
using PFMS_MI04.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace PFMS_MI04.Services
{
    public class MainmenuService
    {
        private static string connectionString = "Server=88.222.244.88;Port=3306;Database=reminder_schema;User ID=user;Password=mi04Curtin@2024#;";

        public static List<MainMenuModel_Reminder> GetSomeReminders()
        {
            List<MainMenuModel_Reminder> reminderList = new List<MainMenuModel_Reminder>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"SELECT user_type, user_name, due_date, reminder_description
                                                FROM reminders
                                                WHERE due_date IS NOT NULL
                                                  AND MONTH(due_date) = MONTH(CURRENT_DATE())
                                                  AND YEAR(due_date) = YEAR(CURRENT_DATE())
                                                ORDER BY 
                                                    ABS(DATEDIFF(due_date, CURRENT_DATE())),
                                                    CASE 
                                                        WHEN priority_level = 'High' THEN 1
                                                        WHEN priority_level = 'Medium' THEN 2
                                                        WHEN priority_level = 'Low' THEN 3
                                                        ELSE 4
                                                    END
                                                LIMIT 4;";
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MainMenuModel_Reminder item = new MainMenuModel_Reminder();
                                item.Name = reader["user_name"].ToString();
                                item.UserType = reader["user_type"].ToString();
                                DateTime dueDateTime = (DateTime)reader["due_date"];
                                item.DueDate = dueDateTime.ToString();
                                item.Description = reader["reminder_description"].ToString();

                                reminderList.Add(item);
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

            return reminderList;
        }

    }
}
