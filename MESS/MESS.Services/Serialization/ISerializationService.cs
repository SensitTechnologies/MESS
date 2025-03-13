using MESS.Data.Models;

namespace MESS.Services.Serialization;

public interface ISerializationService
{
    public Task<List<SerialNumberLog>?> GetAllAsync();
    public Task<bool> CreateAsync(SerialNumberLog serialNumberLog);
    public Task<bool> UpdateAsync(SerialNumberLog serialNumberLog);
    public Task<bool> DeleteAsync(int serialNumberLogId);
}