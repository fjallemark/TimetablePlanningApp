namespace Tellurian.Trains.Planning.App.Shared
{
    public class Loco
    {
        public string OperatorName { get; set; } = string.Empty;
        public int Number { get; set; }
        public OperationDays OperatingDays { get; set; } = new OperationDays();
    }
}
