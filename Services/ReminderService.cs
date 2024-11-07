using MySql.Data.MySqlClient;
using PFMS_MI04.Models;

namespace PFMS_MI04.Services
{
    public class ReminderService
    {
        private static string connectionString = "Server=88.222.244.88;Port=3306;Database=reminder_schema;User ID=user;Password=mi04Curtin@2024#;";

        public static List<ReminderItem> GetAllReminders()
        {
            List<ReminderItem> reminderList = new List<ReminderItem>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM reminders";

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ReminderItem item = new ReminderItem();
                                item.Id = reader["id"].ToString();
                                item.Title = reader["title"].ToString();
                                item.UserName = reader["user_name"].ToString();
                                item.Email = reader["email"].ToString();
                                item.UserType = reader["user_type"].ToString();
                                item.Priority = reader["priority_level"].ToString();

                                DateTime dueDateTime = (DateTime)reader["due_date"];
                                TimeSpan dueTimeSpan = (TimeSpan)reader["due_time"];

                                item.DueDate = DateOnly.FromDateTime(dueDateTime);
                                item.DueTime = TimeOnly.FromTimeSpan(dueTimeSpan);

                                item.Description = reader["reminder_description"].ToString();
                                item.EmailContent = reader["email_content"].ToString();

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

        public static ReminderItem GetReminderById(int reminderId)
        {
            ReminderItem reminderItem = new ReminderItem();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM reminders WHERE id = @Id";
                        command.Parameters.AddWithValue("@Id", reminderId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reminderItem.Id = reader["id"].ToString();
                                reminderItem.Title = reader["title"].ToString();
                                reminderItem.UserName = reader["user_name"].ToString();
                                reminderItem.Email = reader["email"].ToString();
                                reminderItem.UserType = reader["user_type"].ToString();
                                reminderItem.Priority = reader["priority_level"].ToString();

                                DateTime dueDateTime = (DateTime)reader["due_date"];
                                TimeSpan dueTimeSpan = (TimeSpan)reader["due_time"];

                                reminderItem.DueDate = DateOnly.FromDateTime(dueDateTime);
                                reminderItem.DueTime = TimeOnly.FromTimeSpan(dueTimeSpan);

                                reminderItem.Description = reader["reminder_description"].ToString();
                                reminderItem.EmailContent = reader["email_content"].ToString();
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

            return reminderItem;
        }

        public static async Task<bool> createNewReminder(ReminderItem reminder)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO reminders(title, user_name, email, user_type, priority_level, due_date, due_time, reminder_description, email_content)
                            VALUES (@title, @user_name, @email, @user_type, @priority_level, @due_date, @due_time, @reminder_description, @email_content);
                        ";

                        // Add parameters
                        command.Parameters.AddWithValue("@title", reminder.Title);
                        command.Parameters.AddWithValue("@user_name", reminder.UserName);
                        command.Parameters.AddWithValue("@email", reminder.Email);
                        command.Parameters.AddWithValue("@user_type", reminder.UserType);
                        command.Parameters.AddWithValue("@priority_level", reminder.Priority);

                        command.Parameters.AddWithValue("@due_date", reminder.DueDate.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@due_time", reminder.DueTime.ToString("HH:mm:ss"));

                        command.Parameters.AddWithValue("@reminder_description", reminder.Description);
                        command.Parameters.AddWithValue("@email_content", reminder.EmailContent);

                        int rowsInserted = await command.ExecuteNonQueryAsync();

                        return rowsInserted > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return false;
        }

        public static async Task<bool> updateReminder(ReminderItem reminder)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            UPDATE reminders
                            SET title = @title, due_date = @due_date, due_time = @due_time, priority_level = @priority_level, reminder_description = @reminder_description, email_content = @email_content
                            WHERE id = @id
                        ";

                        // Add parameters
                        command.Parameters.AddWithValue("@id", reminder.Id);
                        command.Parameters.AddWithValue("@title", reminder.Title);
                        command.Parameters.AddWithValue("@user_type", reminder.UserType);
                        command.Parameters.AddWithValue("@priority_level", reminder.Priority);

                        command.Parameters.AddWithValue("@due_date", reminder.DueDate.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@due_time", reminder.DueTime.ToString("HH:mm:ss"));

                        command.Parameters.AddWithValue("@reminder_description", reminder.Description);
                        command.Parameters.AddWithValue("@email_content", reminder.EmailContent);

                        int rowsUpdated = await command.ExecuteNonQueryAsync();

                        return rowsUpdated > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return false;
        }


        public static async Task<bool> DeleteReminder(ReminderItem reminder)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        int id = -1;
                        command.CommandText = @"SELECT id FROM reminders WHERE email = @Email AND user_name = @UserName AND user_type = @UserType AND priority_level = @Priority AND 
                                       due_date = @DueDate AND due_time = @DueTime AND reminder_description = @Description";

                        command.Parameters.AddWithValue("@Email", reminder.Email);
                        command.Parameters.AddWithValue("@UserName", reminder.UserName);
                        command.Parameters.AddWithValue("@UserType", reminder.UserType);
                        command.Parameters.AddWithValue("@Priority", reminder.Priority);
                        command.Parameters.AddWithValue("@DueDate", reminder.DueDate.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@DueTime", reminder.DueTime.ToString("HH:mm:ss"));
                        command.Parameters.AddWithValue("@Description", reminder.Description);

                        Int32.TryParse(command.ExecuteScalar().ToString(), out id);

                        command.CommandText = "DELETE FROM reminders WHERE id = @ID";
                        command.Parameters.AddWithValue("@ID", id);

                        int rowsDeleted = await command.ExecuteNonQueryAsync();
                        connection.Close();

                        if (rowsDeleted > 0)
                        {
                            return true;
                        }
                    }
                    connection.Close();
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeleteReminder(ReminderItem reminder, bool emailSent)
        {
            if (!emailSent) return false;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        // Start a transaction
                        using (var transaction = await connection.BeginTransactionAsync())
                        {
                            command.Transaction = transaction;
                            try
                            {
                                command.CommandText = @"DELETE FROM reminders WHERE email = @Email AND user_name = @UserName AND user_type = @UserType 
                                                AND priority_level = @Priority AND due_date = @DueDate AND due_time = @DueTime AND reminder_description = @Description";


                                command.Parameters.AddWithValue("@Email", reminder.Email);
                                command.Parameters.AddWithValue("@UserName", reminder.UserName);
                                command.Parameters.AddWithValue("@UserType", reminder.UserType);
                                command.Parameters.AddWithValue("@Priority", reminder.Priority);
                                command.Parameters.AddWithValue("@DueDate", reminder.DueDate.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd"));
                                command.Parameters.AddWithValue("@DueTime", reminder.DueTime.ToString("HH:mm:ss"));
                                command.Parameters.AddWithValue("@Description", reminder.Description);

                                int rowsDeleted = await command.ExecuteNonQueryAsync();


                                if (rowsDeleted > 0)
                                {
                                    command.Parameters.Clear();
                                    // Insert into the reminder log
                                    DateTime now = DateTime.Now;
                                    string curdate = now.ToString("yyyy-MM-dd");
                                    string curtime = now.ToString("HH:mm:ss");

                                    command.CommandText = @"
                                        INSERT INTO reminder_log (title, user_name, email, user_type, priority, date_added, time_added, reminder_status)
                                        VALUES (@title, @user_name, @email, @user_type, @priority, @sent_date, @sent_time, @reminder_status)";

                                    command.Parameters.AddWithValue("@title", reminder.Title);
                                    command.Parameters.AddWithValue("@user_name", reminder.UserName);
                                    command.Parameters.AddWithValue("@email", reminder.Email);
                                    command.Parameters.AddWithValue("@user_type", reminder.UserType);
                                    command.Parameters.AddWithValue("@priority", reminder.Priority);
                                    command.Parameters.AddWithValue("@sent_date", curdate);
                                    command.Parameters.AddWithValue("@sent_time", curtime);
                                    command.Parameters.AddWithValue("@reminder_status", "Sent");

                                    int rowsInserted = await command.ExecuteNonQueryAsync();

                                    // Commit the transaction if both delete and insert are successful
                                    if (rowsInserted > 0)
                                    {
                                        await transaction.CommitAsync();
                                        return true;
                                    }
                                }

                                await transaction.RollbackAsync();
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                Console.WriteLine("Error: " + ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return false;
        }

    }
}
