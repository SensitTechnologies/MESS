using MESS.Data.Models;
using MESS.Services;

namespace MESS.Tests;

public class UnitTest1
{
    [Fact]
    public void ServiceReferenceTest()
    {
        // var service = new Class1();
        
        // Assert.Equal("Hello I am a service", service.Hello());
    }
    
    [Fact]
    public void DataLayerReferenceTest()
    {
        var data = new Data.Class1();
        
        Assert.Equal("Hello, I am the data layer.", data.Hello());
    }
}