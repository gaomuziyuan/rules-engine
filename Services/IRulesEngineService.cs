using RulesEngine.Models;

namespace RulesEngine.Services;

public interface IRulesEngineService
{
    OutputData CalculateSupplement(InputData input);
}