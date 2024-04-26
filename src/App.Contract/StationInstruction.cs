namespace Tellurian.Trains.Planning.App.Contracts;

public class StationInstruction
{
    public StationInstruction()
    {
        StationInfo = new StationInfo();
    }
    public StationInfo StationInfo { get; init; }
    public string StationInstructionsMarkdown { get; init; } = string.Empty;
    public string ShuntingInstructionsMarkdown { get; init; } = string.Empty;
}
