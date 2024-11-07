using MySql.Data.MySqlClient;
using PFMS_MI04.Models;

namespace PFMS_MI04.Services
{
    public class SponsorDocService
    {
        private static readonly string connString = "Server=88.222.244.88;Port=3306;Database=documents_schema;User ID=user;Password=mi04Curtin@2024#;";

        // ----- Wrapper Methods ----- //

        public async Task<bool> uploadDoc(SponsorDocument doc)
        {
            return await uploadDocumentRecord(doc);
        }

        public async Task<List<SponsorDocument>> getDocs(string sponID)
        {
            return await getDocumentsBySponsor(sponID);
        }

        public async Task<bool> deleteDoc(int docID)
        {
            return await deleteDocument(docID);
        }

        public async Task<bool> updateDoc(SponsorDoc doc)
        {
            return await updateDocument(doc);
        }

        // ----- Internal Functions ----- //

        // Executes an SQL query to get all document entries according to a given sponsor ID
        private async Task<List<SponsorDocument>> getDocumentsBySponsor(string sponID)
        {
            List<SponsorDocument> docList = new List<SponsorDocument>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    await conn.OpenAsync();
                    using (MySqlCommand comm = conn.CreateCommand())
                    {
                        comm.CommandText = @"SELECT * FROM sponsor_documents WHERE sponID = @SponsorID";
                        comm.Parameters.AddWithValue("@SponsorID", sponID);

                        using (MySqlDataReader reader = comm.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SponsorDocument doc = new SponsorDocument();
                                doc.docID = Int32.Parse(reader["docID"].ToString());
                                doc.date = reader["docDate"].ToString();
                                doc.docName = reader["docName"].ToString();
                                doc.docRef = reader["docRef"].ToString();
                                doc.isPreviewable = Int32.Parse(reader["isPreviewable"].ToString());
                                doc.remarks = reader["remarks"].ToString();
                                doc.documentByteCode = (byte[])reader["docBytes"];
                                doc.sponsorId = reader["sponID"].ToString();
                                docList.Add(doc);
                                StrComparator.println("Get: " + doc.docName);
                            }
                        }
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
            return docList;
        }

        // Executes SQL Query for a new Document entry
        private async Task<bool> uploadDocumentRecord(SponsorDocument doc)
        {
            bool inserted = false;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    await conn.OpenAsync();
                    MySqlCommand command = conn.CreateCommand();
                    command = docCommandBuilder(command, doc);
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

        // Delete Document from DB according to docID
        private async Task<bool> deleteDocument(int docID)
        {
            bool deleted = false;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    await conn.OpenAsync();
                    MySqlCommand command = conn.CreateCommand();

                    command.CommandText = @"DELETE FROM sponsor_documents WHERE docID = @documentID";
                    command.Parameters.AddWithValue("@documentID", docID);

                    using (command)
                    {
                        int rows = await command.ExecuteNonQueryAsync();
                        if (rows > 0) { deleted = true; }
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
            return deleted;
        }

        // Executes the SQL Query for updating sql entry
        public async Task<bool> updateDocument(SponsorDoc doc)
        {
            bool updated = false;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    await conn.OpenAsync();
                    MySqlCommand command = conn.CreateCommand();
                    command = docUpdateCommandBuilder(command, doc);

                    using (command)
                    {
                        int rows = await command.ExecuteNonQueryAsync();
                        if (rows > 0) { updated = true; }
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
            return updated;
        }

        // ----- Command Builder Functions ----- //

        // Builds the command for insertion of new values
        private MySqlCommand docCommandBuilder(MySqlCommand command, SponsorDocument doc)
        {
            try
            {
                command.CommandText = @"
                    INSERT INTO sponsor_documents(sponID, docName, docBytes, docDate, docRef, isPreviewable, remarks)
                    VALUES (@sponID, @docName, @docBytes, @docDate, @docRef, @isPreviewable, @remarks);
                ";

                command.Parameters.AddWithValue("@sponID", doc.sponsorId);
                command.Parameters.AddWithValue("@docName", doc.docName);
                command.Parameters.AddWithValue("@docBytes", doc.documentByteCode);
                command.Parameters.AddWithValue("@docDate", doc.date);
                command.Parameters.AddWithValue("@docRef", doc.docRef);
                command.Parameters.AddWithValue("@isPreviewable", doc.isPreviewable);
                command.Parameters.AddWithValue("@remarks", doc.remarks);

                StrComparator.println("Inserting INSERT Command");
            }
            catch (MySqlException mqEx)
            {
                StrComparator.println(mqEx.Message);
            }
            return command;
        }

        // Builds the command for updating of new data
        private MySqlCommand docUpdateCommandBuilder(MySqlCommand command, SponsorDoc doc)
        {
            try
            {
                command.CommandText = @"
                    UPDATE sponsor_documents 
                    SET docName = @docN, docDate = @docD, remarks = @rem 
                    WHERE docID = @docid;
                ";

                command.Parameters.AddWithValue("@docN", doc.docName);
                command.Parameters.AddWithValue("@docD", doc.date);
                command.Parameters.AddWithValue("@rem", doc.remarks);
                command.Parameters.AddWithValue("@docid", doc.docID);

                StrComparator.println("Inserting UPDATE Command");
            }
            catch (MySqlException mqEx)
            {
                StrComparator.println(mqEx.Message);
            }
            return command;
        }
    }
}
