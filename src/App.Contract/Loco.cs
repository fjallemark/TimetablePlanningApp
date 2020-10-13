namespace Tellurian.Trains.Planning.App.Contract
{
    public class Loco
    {
        public string OperatorName { get; set; } = string.Empty;
        public int Number { get; set; }
        public OperationDays OperationDays { get; set; } = new OperationDays();
        public string Class { get; set; } = string.Empty;
        public bool IsRailcar { get; set; }
        public override string ToString() => $"{OperationDays} {OperatorName} {Number}".TrimStart();
        public static Loco Empty { get; } = new Loco();
    }
}
