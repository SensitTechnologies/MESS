﻿using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using MESS.Blazor.Components.Pages.ProductionLog;
using MESS.Data.DTO;
using MESS.Services.ApplicationUser;
using MESS.Services.BrowserCacheManager;
using MESS.Services.Product;
using MESS.Services.ProductionLog;
using MESS.Services.Serialization;
using MESS.Services.SessionManager;
using MESS.Services.WorkInstruction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;

namespace MESS.Tests.UI_Testing.ProductionLog;
using Data.Models;

public class ProductionLogCreationTests : TestContext
{
    private readonly Mock<IProductionLogService> _productionLogServiceMock;
    private readonly Mock<IWorkInstructionService> _workInstructionServiceMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IApplicationUserService> _userServiceMock;
    
    private readonly Mock<ILocalCacheManager> _localCacheManagerMock;
    private readonly Mock<ISerializationService> _serializationServiceMock;
    private readonly Mock<IProductionLogEventService> _productionLogEventServiceMock;
    private readonly Mock<AuthenticationStateProvider> _authProviderMock;
    private readonly Mock<ISessionManager> _sessionManagerMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;
    private readonly Mock<IJSObjectReference> _jsModuleMock;
    private readonly Mock<IToastService> _toastServiceMock;

    public ProductionLogCreationTests()
    {
        _productionLogServiceMock = new Mock<IProductionLogService>();
        _workInstructionServiceMock = new Mock<IWorkInstructionService>();
        _productServiceMock = new Mock<IProductService>();
        _userServiceMock = new Mock<IApplicationUserService>();
        _localCacheManagerMock = new Mock<ILocalCacheManager>();
        _serializationServiceMock = new Mock<ISerializationService>();
        _productionLogEventServiceMock = new Mock<IProductionLogEventService>();
        _authProviderMock = new Mock<AuthenticationStateProvider>();
        _sessionManagerMock = new Mock<ISessionManager>();
        _jsRuntimeMock = new Mock<IJSRuntime>();
        _jsModuleMock = new Mock<IJSObjectReference>();
        _toastServiceMock = new Mock<IToastService>();
            
        Services.AddSingleton(_productionLogServiceMock.Object);
        Services.AddSingleton(_workInstructionServiceMock.Object);
        Services.AddSingleton(_productServiceMock.Object);
        Services.AddSingleton(_userServiceMock.Object);
        Services.AddSingleton(_localCacheManagerMock.Object);
        Services.AddSingleton(_serializationServiceMock.Object);
        Services.AddSingleton(_productionLogEventServiceMock.Object);
        Services.AddSingleton<AuthenticationStateProvider>(_authProviderMock.Object);
        Services.AddSingleton(_sessionManagerMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);
        Services.AddSingleton(_toastServiceMock.Object);
            
        _jsRuntimeMock.Setup(js => js.InvokeAsync<IJSObjectReference>(
            "import", It.IsAny<object[]>())).ReturnsAsync(_jsModuleMock.Object);
            
        SetupAuthenticationState();
        SetupCacheDefaults();
        SetupAuthorizationServices();
    }
    
    private void SetupAuthenticationState()
    {
        var authState = new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "user123"),
            }, "testauth")));
            
        _authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);
    }
    
    private void SetupAuthorizationServices()
    {
        // Add required authorization services
        Services.AddAuthorization();
        Services.AddSingleton<IAuthorizationPolicyProvider>(new DefaultAuthorizationPolicyProvider(
            Options.Create(new AuthorizationOptions())));
        Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();
    }
        
    private void SetupCacheDefaults()
    {
        _localCacheManagerMock.Setup(m => m.GetProductionLogFormAsync())
            .ReturnsAsync(new ProductionLogFormDTO());
                
        _localCacheManagerMock.Setup(m => m.GetWorkflowActiveStatusAsync())
            .ReturnsAsync(false);
    }
    
    [Fact]
    public void Create_OnlyWorkInstructionSelected_DoesNotCreateLog()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("TechnicianUser");
        authContext.SetRoles("Technician");

        var cut = RenderComponent<Create>();
        
        // No need to access the protected HandleSubmit directly
        // Instead, find and click the submit button
        var submitButton = cut.Find("button[type='submit']");
    
        // Act
        submitButton.Click();
    
        // Assert
        _productionLogServiceMock.Verify(
            service => service.CreateAsync(It.IsAny<ProductionLog>()),
            Times.Never);
    }
}