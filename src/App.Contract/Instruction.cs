namespace Tellurian.Trains.Planning.App.Contracts;

public class Instruction
{
    public string? Markdown { get; init; }
    public string? Language { get; init; }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Markdown);
}
