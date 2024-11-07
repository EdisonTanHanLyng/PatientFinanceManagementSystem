using MySql.Data.MySqlClient;
using PFMS_MI04.Models;
using System.Data;

namespace PFMS_MI04.Services
{
    public class PricingService
    {
        private static string connectionString = "Server=88.222.244.88;Port=3306;Database=pricing_schema;User ID=user;Password=mi04Curtin@2024#;";

        
        public static async Task<bool> updatePrice(PricingModel model)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            UPDATE prices
                            SET dialysis_cost = @dialysis_cost, epo_cost = @epo_cost
                            WHERE sponsor_code = @sponsor_code
                        ";

                        // Add parameters
                        command.Parameters.AddWithValue("@sponsor_code", model.SponsorCode);
                        command.Parameters.AddWithValue("@dialysis_cost", model.DialysisCost);
                        command.Parameters.AddWithValue("@epo_cost", model.EPOCost);

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

        public static async Task<PricingModel> getPrice(string sponsorCode)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT dialysis_cost, epo_cost
                            FROM prices
                            WHERE sponsor_code = @sponsor_code
                        ";

                        // Add parameters
                        command.Parameters.AddWithValue("@sponsor_code", sponsorCode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var dialysisCost = reader.GetDecimal("dialysis_cost");
                                var epoCost = reader.GetDecimal("epo_cost");

                                return new PricingModel
                                {
                                    SponsorCode = sponsorCode,
                                    DialysisCost = (float)dialysisCost,
                                    EPOCost = (float)epoCost
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return null;
        }
    }
}
