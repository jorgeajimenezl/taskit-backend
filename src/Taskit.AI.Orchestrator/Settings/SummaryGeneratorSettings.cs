namespace Taskit.AI.Orchestrator.Settings;

public class SummaryGeneratorSettings
{
    public string Model { get; init; } = "gpt-4.1-nano";
    public int BatchMessageLimit { get; init; } = 10;
    public int BatchTimeLimitSeconds { get; init; } = 5;
}