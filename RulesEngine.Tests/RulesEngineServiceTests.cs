using RulesEngine.Models;
using RulesEngine.Services;

namespace RulesEngine.Tests;

public class RulesEngineServiceTests
{
    private readonly RulesEngineService _rulesEngineService;

    public RulesEngineServiceTests()
    {
        _rulesEngineService = new RulesEngineService();
    }

    [Fact]
    public void CalculateSupplement_NotEligible_ReturnsZeroAmounts()
    {
        // Arrange
        var inputData = new InputData
        {
            Id = "abc",
            InPayForDec = false,
            Composition = "single",
            NumOfChildren = 2
        };

        // Act
        var result = _rulesEngineService.CalculateSupplement(inputData);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsEligible, "Result should indicate ineligibility");
        Assert.Equal(0.0f, result.BaseAmount);
        Assert.Equal(0.0f, result.ChildrenAmount);
        Assert.Equal(0.0f, result.SupplementAmount);
    }

    [Fact]
    public void CalculateSupplement_EligibleSingleComposition_ReturnsCorrectAmounts()
    {
        // Arrange
        var inputData = new InputData
        {
            Id = "abc",
            InPayForDec = true,
            Composition = "single",
            NumOfChildren = 2
        };

        // Act
        var result = _rulesEngineService.CalculateSupplement(inputData);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEligible, "Result should indicate eligibility");
        Assert.Equal(60.0f, result.BaseAmount);
        Assert.Equal(40.0f, result.ChildrenAmount);
        Assert.Equal(100.0f, result.SupplementAmount);
    }

    [Fact]
    public void CalculateSupplement_EligibleCoupleComposition_ReturnsCorrectAmounts()
    {
        // Arrange
        var inputData = new InputData
        {
            Id = "abc",
            InPayForDec = true,
            Composition = "couple",
            NumOfChildren = 3
        };

        // Act
        var result = _rulesEngineService.CalculateSupplement(inputData);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEligible, "Result should indicate eligibility");
        Assert.Equal(120.0f, result.BaseAmount);
        Assert.Equal(60.0f, result.ChildrenAmount);
        Assert.Equal(180.0f, result.SupplementAmount);
    }

    [Fact]
    public void CalculateSupplement_InvalidComposition_ReturnsZeroBaseAmount()
    {
        // Arrange
        var inputData = new InputData
        {
            Id = "abc",
            InPayForDec = true,
            Composition = "invalid",
            NumOfChildren = 1
        };

        // Act
        var result = _rulesEngineService.CalculateSupplement(inputData);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEligible, "Result should indicate eligibility");
        Assert.Equal(0.0f, result.BaseAmount);
        Assert.Equal(20.0f, result.ChildrenAmount);
        Assert.Equal(20.0f, result.SupplementAmount);
    }

    [Fact]
    public void CalculateSupplement_EdgeCases_HandledGracefully()
    {
        // Arrange
        var edgeCases = new List<InputData>
        {
            new InputData { Id = "126", InPayForDec = true, Composition = "single", NumOfChildren = 0 },
            new InputData { Id = "127", InPayForDec = true, Composition = "couple", NumOfChildren = 10 },
            new InputData { Id = "128", InPayForDec = false, Composition = "single", NumOfChildren = 5 }
        };

        // Act & Assert
        foreach (var input in edgeCases)
        {
            var result = _rulesEngineService.CalculateSupplement(input);
            Assert.NotNull(result);
            Assert.Equal(input.InPayForDec, result.IsEligible);

            if (input.InPayForDec)
            {
                var expectedBaseAmount = input.Composition switch
                {
                    "single" => 60.0f,
                    "couple" => 120.0f,
                    _ => 0.0f
                };

                Assert.Equal(expectedBaseAmount, result.BaseAmount);
                Assert.Equal(input.NumOfChildren * 20.0f, result.ChildrenAmount);
                Assert.Equal(result.BaseAmount + result.ChildrenAmount, result.SupplementAmount);
            }
            else
            {
                Assert.Equal(0.0f, result.BaseAmount);
                Assert.Equal(0.0f, result.ChildrenAmount);
                Assert.Equal(0.0f, result.SupplementAmount);
            }
        }
    }
}