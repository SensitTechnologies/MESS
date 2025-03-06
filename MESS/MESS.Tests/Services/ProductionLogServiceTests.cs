using MESS.Data.Context;
using MESS.Services.ProductionLog;
using Moq;

namespace MESS.Tests.Services.ProductionLog;
using Data.Models;

public class ProductionLogServiceTests
{
    private readonly ProductionLogService _productionLogService;
    private readonly Mock<ApplicationContext> _mockContext;

    public ProductionLogServiceTests()
    {
        _mockContext = new Mock<ApplicationContext>();
        _productionLogService = new ProductionLogService(_mockContext.Object);
    }
}