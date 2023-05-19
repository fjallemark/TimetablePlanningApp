using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.App.Contracts;

public class Trainset
{
    public string Operator { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Class { get; set; } = string.Empty;
    public string WagonTypes { get; set; } = string.Empty;
    public bool IsCargo { get; set; }
    public int PositionInTrain { get; set; }
    public int MaxNumberOfWaggons { get; set; }
    public byte OperationDaysFlag { get; set; }
    public bool HasCoupleNote { get; set; }
    public bool HasUncoupleNote { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string FinalDestination { get; set; } = string.Empty;
    public bool HasFinalDestination => FinalDestination.HasValue() && !FinalDestination.Equals(Destination, System.StringComparison.OrdinalIgnoreCase);
    public override string ToString() => $"{Operator} {Number} {WagonTypes}".TrimEnd();
}
