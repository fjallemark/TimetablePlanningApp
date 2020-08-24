namespace Tellurian.Trains.Planning.App.Contract
{
    public class CallAction
    {
        public Station? Station { get; set; }
        public string Track { get; set; } = string.Empty;
        public CallTime? Time { get; set; }
        public override string ToString() => $"{Station} {Track} {Time}";
    }
}
