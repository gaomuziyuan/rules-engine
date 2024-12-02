using System.ComponentModel.DataAnnotations;

namespace RulesEngine.Models;

public class InputData
{
    [Required] public string Id { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int NumOfChildren { get; set; }
    [Required] public string Composition { get; set; } = string.Empty;

    public bool InPayForDec { get; set; }
}