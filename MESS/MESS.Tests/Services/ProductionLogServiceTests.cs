using MESS.Data.Context;
using MESS.Services.ProductionLog;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MESS.Tests.Services.ProductionLog;
using Data.Models;

public class ProductionLogServiceTests
{
    private readonly ProductionLogService _productionLogService;
    private readonly Mock<IDbContextFactory<ApplicationContext>> _mockDbContextFactory;

    public ProductionLogServiceTests()
    {
        _mockDbContextFactory = new Mock<IDbContextFactory<ApplicationContext>>();
        _productionLogService = new ProductionLogService(_mockDbContextFactory.Object);
    }
}