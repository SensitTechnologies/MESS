namespace MESS.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Data.Models;

public class LineOperatorService
{
    private readonly string _connectionString;

    public LineOperatorService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string not found");
        }
    }

    public async Task<List<LineOperator>> GetAllLineOperators() // spits out a list of all line operators
    {
        var operators = new List<LineOperator>();
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command =
                   new SqlCommand("SELECT Id, FirstName, LastName, IsActive, ProductionLogId FROM LineOperators",
                       connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    operators.Add(new LineOperator
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        IsActive = reader.GetBoolean(3),
                        ProductionLogId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                        
                        CreatedBy = "MESS",
                        CreatedOn = DateTime.Now,
                        LastModifiedBy = "MESS",
                        LastModifiedOn = DateTime.Now,
                    });
                }
            }
        }
        return operators;
    }

    public async Task<LineOperator> AddLineOperator(LineOperator lineOperator)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command =
                   new SqlCommand(
                       "INSERT INTO LineOperators (FirstName, LastName, IsActive, ProductionLogId) OUTPUT INSERTED.Id VALUES (@FirstName, @LastName, @IsActive, @ProductionLogId)",
                       connection))
            {
                command.Parameters.AddWithValue("@FirstName", lineOperator.FirstName);
                command.Parameters.AddWithValue("@LastName", lineOperator.LastName);
                command.Parameters.AddWithValue("@IsActive", lineOperator.IsActive);
                command.Parameters.AddWithValue("@ProductionLogId", (object?)lineOperator.ProductionLogId ?? DBNull.Value);
                
                var result = await command.ExecuteScalarAsync();
                int operatorId = 0;

                if (result != null && result != DBNull.Value)
                {
                    operatorId = Convert.ToInt32(result);
                }
                
                return new LineOperator()
                {
                    Id = operatorId,
                    FirstName = lineOperator.FirstName,
                    LastName = lineOperator.LastName,
                    IsActive = lineOperator.IsActive,
                    ProductionLogId = lineOperator.ProductionLogId,
                    
                    CreatedBy = "MESS",
                    CreatedOn = DateTime.Now,
                    LastModifiedBy = "MESS",
                    LastModifiedOn = DateTime.Now,
                };
            }
        }
    }

    public async Task<LineOperator> UpdateLineOperator(LineOperator lineOperator)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command =
                   new SqlCommand(
                       "UPDATE LineOperators SET Firstname = @FirstName, LastName = @LastName, IsActive = @IsActive, ProductionLogId = @ProductionLogId WHERE Id = @Id",
                       connection))
            {
                command.Parameters.AddWithValue("@Id", lineOperator.Id);
                command.Parameters.AddWithValue("@FirstName", lineOperator.FirstName);
                command.Parameters.AddWithValue("@LastName", lineOperator.LastName);
                command.Parameters.AddWithValue("@IsActive", lineOperator.IsActive);
                command.Parameters.AddWithValue("@ProductionLogId", (object?) lineOperator.ProductionLogId ?? DBNull.Value);
                
                command.Parameters.AddWithValue("@LastModifiedOn", DateTime.Now);
                command.Parameters.AddWithValue("@LastModifiedBy", "MESS");
                await command.ExecuteNonQueryAsync();
            }
        }
        return lineOperator;
    }

    public async Task DeleteLineOperator(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand("DELETE FROM LineOperators WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}