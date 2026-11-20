using MESS.Services.DTOs.Rework;

namespace MESS.Tests.Validators;

public class ReworkUpdateRequestValidatorTests
{
    private readonly ReworkUpdateRequestValidator _validator = new();

    private static ReworkUpdateRequest ValidRequest() => new()
    {
        AttemptId = 1,
        Original = new ReworkEditValues { ProductId = 1, WorkInstructionId = 1, WorkInstructionStepId = 1, OperatorId = "op" },
        Updated = new ReworkEditValues
        {
            ProductId = 1, WorkInstructionId = 1, WorkInstructionStepId = 1, OperatorId = "op",
            Success = true, FailureNounId = 2, FailureAdjectiveId = 3
        }
    };

    [Fact]
    public void ValidRequest_Passes()
    {
        Assert.True(_validator.Validate(ValidRequest()).IsValid);
    }

    [Fact]
    public void NullSuccess_IsAllowed()
    {
        var request = ValidRequest();
        request.Updated.Success = null;

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void AdjectiveWithoutNoun_Fails()
    {
        var request = ValidRequest();
        request.Updated.FailureNounId = null;

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Theory]
    [InlineData("product")]
    [InlineData("instruction")]
    [InlineData("step")]
    [InlineData("operator")]
    public void MissingRequiredField_Fails(string field)
    {
        var request = ValidRequest();
        switch (field)
        {
            case "product": request.Updated.ProductId = 0; break;
            case "instruction": request.Updated.WorkInstructionId = 0; break;
            case "step": request.Updated.WorkInstructionStepId = 0; break;
            case "operator": request.Updated.OperatorId = ""; break;
        }

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void NonPositiveAttemptId_Fails()
    {
        var request = ValidRequest();
        request.AttemptId = 0;

        Assert.False(_validator.Validate(request).IsValid);
    }
}
