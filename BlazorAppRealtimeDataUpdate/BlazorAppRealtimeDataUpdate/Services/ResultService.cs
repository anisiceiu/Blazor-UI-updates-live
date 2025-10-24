namespace BlazorAppRealtimeDataUpdate.Services
{
    using BlazorAppRealtimeDataUpdate.Models;
    using Microsoft.Data.SqlClient;
    // Services/ResultService.cs
    using System.Data;
    using System.Data.SqlClient;

    public class ResultService
    {
        private readonly IConfiguration _config;

        public ResultService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<ResultModel>> GetAllAsync()
        {
            var results = new List<ResultModel>();
            var connectionString = _config.GetConnectionString("DefaultConnection");

            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("SELECT Id, Score,Result FROM Results", conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new ResultModel
                {
                    Id = reader.GetInt32(0),
                    Score = reader.GetInt32(1),
                    Result = reader.GetString(2)
                });
            }

            return results;
        }
    }

}
