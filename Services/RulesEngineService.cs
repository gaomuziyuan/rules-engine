using RulesEngine.Models;

namespace RulesEngine.Services;

public class RulesEngineService : IRulesEngineService
{
    public OutputData CalculateSupplement(InputData input)
    {
        var result = new OutputData
        {
            Id = input.Id,
            IsEligible = input.InPayForDec,
            BaseAmount = 0.0f,
            ChildrenAmount = 0.0f,
            SupplementAmount = 0.0f
        };

        if (!input.InPayForDec)
        {
            return result;
        }

        // Calculate base amount
        result.BaseAmount = input.Composition switch
        {
            "single" => 60.0f,
            "couple" => 120.0f,
            _ => 0.0f
        };

        // Calculate children amount
        result.ChildrenAmount = input.NumOfChildren * 20.0f;

        // Calculate total supplement amount
        result.SupplementAmount = result.BaseAmount + result.ChildrenAmount;
        return result;
    }
}