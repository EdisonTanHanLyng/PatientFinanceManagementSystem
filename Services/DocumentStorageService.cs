
using MySql.Data.MySqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PFMS_MI04.Services
{
    public class DocumentStorageService
    {
        private static string connectionString = "Server=88.222.244.88;Port=3306;Database=documents_schema;User ID=user;Password=mi04Curtin@2024#;";


        internal static void getSponsorDocuments(ref byte[]? fileBytes, ref string? fileName, ref string? fileType, int id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT file_name, file_type, file_data FROM sponsor_documents WHERE docID = @id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fileName = reader.GetString("file_name");
                            fileType = reader.GetString("file_type");
                            fileBytes = (byte[])reader["file_data"];
                        }
                    }
                }
            }
        }

        internal static void getSponsorDocumentRemarks(ref string remarks, int id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT remarks FROM sponsor_documents WHERE docID = @id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            remarks = reader.GetString("remarks");
                        }
                    }
                }
            }
        }

        internal static void getSponsorDocumentUploadDate(ref DateTime date, ref string fileName, int id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT backup_date, file_name FROM sponsor_documents WHERE docID = @id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            date = reader.GetDateTime("backup_date");
                            fileName = reader.GetString("file_name");
                        }
                    }
                }
            }
        }

        internal static async void uploadFileToDatabase(IFormFile file, string data, byte[] fileBytes, int id)
        {
            DateTime now = DateTime.Now;

            using (var connection = new MySqlConnection(connectionString))
            {
                string query = @"INSERT INTO sponsor_documents (sponID, backup_date, remarks, file_name, file_type, file_size, file_data)
                             VALUES (@SponsorID, @Date, @Remarks, @FileName, @FileType, @FileSize, @FileData)";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SponsorID", id);
                    command.Parameters.AddWithValue("@Date", now);
                    command.Parameters.AddWithValue("@Remarks", data);
                    command.Parameters.AddWithValue("@FileName", file.FileName);
                    command.Parameters.AddWithValue("@FileType", file.ContentType);
                    command.Parameters.AddWithValue("@FileSize", file.Length);
                    command.Parameters.AddWithValue("@FileData", fileBytes);

                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        internal static void getAllRelatedDocumentsID(ref List<int> docIDs, int spoID)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT docID FROM sponsor_documents WHERE sponID = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", spoID); 

                    using (var reader = command.ExecuteReader())
                    {
                        while(reader.Read()) // Loop through all the rows
                        {
                            docIDs.Add(reader.GetInt32(0)); // Get the value from the first column (docID)
                        }
                    }
                }
            }
        }
    }
}
