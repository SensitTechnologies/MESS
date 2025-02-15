namespace MESS.Services.ProductionLog;
using Data.Models;

public class ProductionLogService : IProductionLogService
{
    public IEnumerable<Data.Models.ProductionLog> GetAll()
    {
        return new List<Data.Models.ProductionLog>
        {
            new ProductionLog
            {
                Id = 1,
                CreatedBy = "",
                CreatedOn = default,
                LastModifiedBy = "",
                LastModifiedOn = default,
                Product = new Product
                {
                    Name = "Test Product 1",
                    CreatedBy = "TestUser1",
                    CreatedOn = default,
                    LastModifiedBy = "TestUser1",
                    LastModifiedOn = default
                }
            }
        };
    }

    public Data.Models.ProductionLog Get(string id)
    {
        throw new NotImplementedException();
    }

    public bool Create(Data.Models.ProductionLog productionLog)
    {
        throw new NotImplementedException();
    }

    public bool Delete(string id)
    {
        throw new NotImplementedException();
    }

    public Data.Models.ProductionLog Edit(Data.Models.ProductionLog existing, Data.Models.ProductionLog updated)
    {
        throw new NotImplementedException();
    }
}