using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using MySql.Data.MySqlClient;
using PFMS_MI04.Models;
using Konscious.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;

namespace PFMS_MI04.Services
{
    public class AccountService
    {
        private static string connectionString = "Server=88.222.244.88;Port=3306;Database=account_schema;User ID=user;Password=mi04Curtin@2024#;";

        public static void storeAccount(AccountModel account)
        {
            byte[] salt = generateSalt();
            byte[] hash = hashPassword(account.Password, salt);

            string hashBase64 = Convert.ToBase64String(hash);
            string saltBase64 = Convert.ToBase64String(salt);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO accounts (acc_name, pass_hash, pass_salt, acc_role, user_name, user_gender, user_id, user_race, user_tel, user_address, user_description) VALUES (@AccName, @PassHash, @PassSalt, @AccRole, @UserName, @UserGender, @UserID, @UserRace, @UserTel, @UserAddress, @UserDesc)";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@AccName", account.AccName);
                    cmd.Parameters.AddWithValue("@PassHash", hashBase64);
                    cmd.Parameters.AddWithValue("@PassSalt", saltBase64);
                    cmd.Parameters.AddWithValue("@AccRole", account.Role);
                    cmd.Parameters.AddWithValue("@UserName", account.Username);
                    cmd.Parameters.AddWithValue("@UserGender", account.Gender);
                    cmd.Parameters.AddWithValue("@UserID", account.UserID);
                    cmd.Parameters.AddWithValue("@UserRace", account.Race);
                    cmd.Parameters.AddWithValue("@UserTel", account.Tel);
                    cmd.Parameters.AddWithValue("@UserAddress", account.Address);
                    cmd.Parameters.AddWithValue("@UserDesc", account.Description);

                    cmd.ExecuteNonQuery();
                }

                connection.Close();
            }
            
        }

        public static (byte[] storedHash, byte[] storedSalt, string storedUserID, string storedAccRole) getStoredCredentials(string accName)
        {
            byte[] storedHash = null;
            byte[] storedSalt = null;
            string storedUserID = null;
            string storedAccRole = null;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT pass_hash, pass_salt, user_id, acc_role FROM accounts WHERE acc_name = @AccName;";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@AccName", accName);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHashBase64 = reader["pass_hash"].ToString();
                            string storedSaltBase64 = reader["pass_salt"].ToString();
                            string storedDbUserId = reader["user_id"].ToString();
                            string storedDbAccRole = reader["acc_role"].ToString(); 


                            // Check if account exists
                            if (storedHashBase64 == null || storedSaltBase64 == null || storedDbUserId == null)
                            {
                                throw new ArgumentException("Account name does not exist.");
                                connection.Close();
                            }

                            storedHash = Convert.FromBase64String(storedHashBase64);
                            storedSalt = Convert.FromBase64String(storedSaltBase64);
                            storedUserID = storedDbUserId;
                            storedAccRole = storedDbAccRole;
                        }
                    }

                }

                connection.Close();
            }



            return (storedHash, storedSalt, storedUserID, storedAccRole);
        }

        public async static Task<string> checkStoredId(string userId)
        {
            string existingUserid = "1";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT EXISTS ( SELECT 1 FROM accounts WHERE user_id = '@UserID' ) AS user_exists;";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            existingUserid = reader["user_exists"].ToString();
                        }
                    }

                }

                connection.Close();
            }

            return existingUserid;
        }

        // Generate random salt
        public static byte[] generateSalt(int length = 16)
        {
            byte[] salt = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }

        public static byte[] hashPassword(string password, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Create an instance of Argon2id
            using (var argon2 = new Argon2id(passwordBytes))
            {
                argon2.Salt = salt;
                argon2.MemorySize = 65536; // 64 MB
                argon2.Iterations = 4;
                argon2.DegreeOfParallelism = 2; // Number of CPU cores to use

                return argon2.GetBytes(32);
            }
        }

        public static (bool isValid, string userID, string accRole) verifyPassword(string enteredPassword, byte[] storedHash, byte[] storedSalt, string storedUserID, string storedAccRole)
        {
            // Hash the entered password using the stored salt
            byte[] enteredHash = hashPassword(enteredPassword, storedSalt);

            // Compare the entered hash with the stored hash
            bool isPasswordValid = enteredHash.SequenceEqual(storedHash);

            return (isPasswordValid, storedUserID, storedAccRole);
        }

        public static (bool isValid, string userID, string accRole) verifyAccount(string accName, string accPass)
        {
            (byte[] storedHash, byte[] storedSalt, string storedUserID, string storedAccRole) = getStoredCredentials(accName);

            return verifyPassword(accPass, storedHash, storedSalt, storedUserID, storedAccRole);
        }
    }
}
