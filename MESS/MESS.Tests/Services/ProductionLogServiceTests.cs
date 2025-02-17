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

    [Fact]
    public void GetTotalTime_ValidLog_ReturnsAccurateTimeSpanDuration()
    {
        var validProductionLog = new ProductionLog
        {
            LogSteps =
            [
                new ProductionLogStep
                {
                    Success = true,
                    StartTime = new DateTimeOffset(2025, 01, 01, 00, 00, 00, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2025, 01, 01, 00, 01, 39, TimeSpan.Zero)
                }
            ]
        };

        var expectedTimeSpan = new TimeSpan(0, 1, 39);
        
        var response = _productionLogService.GetTotalTime(validProductionLog);

        Assert.NotNull(response);
        
        Assert.Equal(expectedTimeSpan, response);
    }
}