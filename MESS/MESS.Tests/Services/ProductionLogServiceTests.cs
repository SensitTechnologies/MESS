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
    public void GetTotalTime_SingleValidLogMinuteSecond_ReturnsAccurateTimeSpanDuration()
    {
        var validProductionLog = new ProductionLog
        {
            LogSteps =
            [
                new ProductionLogStep
                {
                    Success = true,
                    SubmitTime = new DateTimeOffset(2025, 01, 01, 00, 00, 00, TimeSpan.Zero),
                }
            ],
            SubmitTime = new DateTimeOffset(2025, 01, 01, 00, 01, 39, TimeSpan.Zero),
        };

        var expectedTimeSpan = new TimeSpan(0, 1, 39);
        
        var response = _productionLogService.GetTotalTime(validProductionLog);

        Assert.NotNull(response);
        
        Assert.Equal(expectedTimeSpan, response);
    }

    [Fact]
    public void GetTotalTime_SingleValidLogHourMinuteSecond_ReturnsAccurateTimeSpanDuration()
    {
        var validProductionLog = new ProductionLog
        {
            LogSteps =
            [
                new ProductionLogStep
                {
                    Success = true,
                    SubmitTime = new DateTimeOffset(2025, 01, 01, 0, 00, 00, TimeSpan.Zero)
                }
            ],
            SubmitTime = new DateTimeOffset(2025, 01, 01, 3, 01, 39, TimeSpan.Zero)
        };

        var expectedTimeSpan = new TimeSpan(3, 1, 39);
        
        var response = _productionLogService.GetTotalTime(validProductionLog);

        Assert.NotNull(response);
        
        Assert.Equal(expectedTimeSpan, response);
    }

    [Fact]
    public void GetTotalTime_MultipleValidLogs_ReturnsAccurateTimeSpanDuration()
    {
        var validProductionLog = new ProductionLog
        {
            LogSteps =
            [
                new ProductionLogStep
                {
                    Success = true,
                    SubmitTime = new DateTimeOffset(2025, 01, 01, 00, 00, 00, TimeSpan.Zero),
                },
                new ProductionLogStep
                {
                    Success = true,
                    SubmitTime = new DateTimeOffset(2025, 01, 01, 00, 00, 30, TimeSpan.Zero),
                },
                new ProductionLogStep
                {
                    Success = true,
                    SubmitTime = new DateTimeOffset(2025, 01, 01, 00, 02, 22, TimeSpan.Zero),
                },
                new ProductionLogStep
                {
                    Success = true,
                    SubmitTime = new DateTimeOffset(2025, 01, 01, 00, 03, 10, TimeSpan.Zero),
                }
            ],
            SubmitTime = new DateTimeOffset(2025, 01, 01, 00, 03, 59, TimeSpan.Zero),
        };
        
        var expectedTimeSpan = new TimeSpan(0, 3, 59);

        var response = _productionLogService.GetTotalTime(validProductionLog);

        Assert.NotNull(response);
        
        Assert.Equal(expectedTimeSpan, response);
    }

    [Fact]
    public void GetTotalTime_NullLogSteps_ReturnsTimeSpanWithZeroValue()
    {
        var invalidProductionLog = new ProductionLog();
        var expectedTimeSpan = new TimeSpan(0);
        
        var response = _productionLogService.GetTotalTime(invalidProductionLog);
        
        Assert.Equal(expectedTimeSpan, response);
    }
    
    [Fact]
    public void GetTotalTime_EmptyLogSteps_ReturnsTimeSpanWithZeroValue()
    {
        var invalidProductionLog = new ProductionLog
        {
            LogSteps = []
        };
        
        var expectedTimeSpan = new TimeSpan(0);
        
        var response = _productionLogService.GetTotalTime(invalidProductionLog);
        
        Assert.Equal(expectedTimeSpan, response);
    }

    [Fact]
    public void GetTotalTime_NegativeTimeValue_ReturnsTimeSpanWithZeroValue()
    {
        var invalidProductionLog = new ProductionLog
        {
            LogSteps =
            [
                new ProductionLogStep
                {
                    Success = true, 
                    SubmitTime = DateTimeOffset.UtcNow
                },
            ],
            SubmitTime = new DateTimeOffset(2025, 01, 01, 00, 00, 30, TimeSpan.Zero),
        };

        var expectedTimeSpan = new TimeSpan(0);

        var response = _productionLogService.GetTotalTime(invalidProductionLog);

        Assert.Equal(expectedTimeSpan, response);
    }
}