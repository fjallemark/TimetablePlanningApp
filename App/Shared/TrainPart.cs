namespace Tellurian.Trains.Planning.App.Shared
{
    public class TrainPart
    {
        public string? TrainNumber { get; set; }
        public CallAction FromDeparture { get; set; } = CallAction.Empty;
        public CallAction ToArrival { get; set; } = CallAction.Empty;
    }
}
