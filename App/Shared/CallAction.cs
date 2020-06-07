namespace Tellurian.Trains.Planning.App.Shared
{
    public class CallAction
    {
        public Station Station { get; set; } = new Station();
        public string? Time { get; set; }
        public bool IsHidden { get; set; }
        public bool IsStop { get; set; }
        public static CallAction Empty => new CallAction();
    }
}
