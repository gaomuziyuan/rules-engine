namespace RulesEngine.Models;

public class OutputData
{
    public string Id { get; set; }
    public bool IsEligible { get; set; }
    public float BaseAmount { get; set; }
    public float ChildrenAmount { get; set; }
    public float SupplementAmount { get; set; }
}