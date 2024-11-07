using MySql.Data.MySqlClient;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using System;
using System.Threading.Tasks;

namespace PFMS_MI04.Services
{
    public class ManageUserAccountService
    {
        // Connection string to the MySQL database
        private static readonly string connectionString2 = "Server=88.222.244.88;Port=3306;Database=account_schema;User ID=user;Password=mi04Curtin@2024#";

        // Method to retrieve user details from the database asynchronously
        public async Task<ManageUserAccountItemList.ManageUser?> GetUserDetailsAsync(string userId)
        {
            ManageUserAccountItemList.ManageUser? user = null;
            string query = "SELECT user_name, user_id, user_tel, acc_name, acc_role, user_gender, user_race, user_address, user_description FROM accounts WHERE user_id = @userId";

            using (MySqlConnection conn = new MySqlConnection(connectionString2))
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user = new ManageUserAccountItemList.ManageUser
                            {
                                Name = reader["acc_name"].ToString(),
                                UserId = reader["user_id"].ToString(),
                                Contact = reader["user_tel"].ToString(),
                                Role = reader["acc_role"].ToString(),
                                Gender = reader["user_gender"].ToString(),
                                Race = reader["user_race"].ToString(),
                                Address = reader["user_address"].ToString(),
                                Description = reader["user_description"].ToString(),
                                Username = reader["user_name"].ToString()
                            };
                        }
                    }
                }
            }

            return user;
        }

        // Method to update user details in the database asynchronously
        public async Task<bool> UpdateUserInDatabaseAsync(ManageUserAccountItemList.ManageUser user, string? hashedPassword, string? salt)
        {
            string query = "UPDATE accounts SET user_tel = @userTel, acc_role = @role, user_gender = @gender, user_race = @race, user_address = @address, user_description = @description, user_name = @userName";

            if (hashedPassword != null && salt != null)
            {
                query += ", pass_hash = @passHash, pass_salt = @passSalt";
            }

            query += " WHERE user_id = @userId";

            using (MySqlConnection conn = new MySqlConnection(connectionString2))
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userTel", user.Contact);
                    cmd.Parameters.AddWithValue("@role", user.Role);
                    cmd.Parameters.AddWithValue("@gender", user.Gender);
                    cmd.Parameters.AddWithValue("@race", user.Race);
                    cmd.Parameters.AddWithValue("@address", user.Address);
                    cmd.Parameters.AddWithValue("@description", user.Description);
                    cmd.Parameters.AddWithValue("@userName", user.Username);
                    cmd.Parameters.AddWithValue("@userId", user.UserId);

                    if (hashedPassword != null && salt != null)
                    {
                        cmd.Parameters.AddWithValue("@passHash", hashedPassword);
                        cmd.Parameters.AddWithValue("@passSalt", salt);
                    }

                    try
                    {
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0; // Return true if at least one row was updated
                    }
                    catch (Exception ex)
                    {
                        return false; // Return false if there's an error
                    }
                }
            }
        }

        // Method to remove a user from the database asynchronously
        public async Task<bool> RemoveUserInDb(AccountModel request)
        {
            string query = "DELETE FROM accounts WHERE user_id = @userId";

            using (MySqlConnection conn = new MySqlConnection(connectionString2))
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", request.UserID);
                    try
                    {
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0; // Return true if at least one row was deleted
                    }
                    catch (Exception ex)
                    {
                        return false; // Return false if there's an error
                    }
                }
            }
        }
    }
}
