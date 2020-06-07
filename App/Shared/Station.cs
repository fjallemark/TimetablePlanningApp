namespace Tellurian.Trains.Planning.App.Shared
{
    public class Station
    {
        public string Name { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    public class StationCall
    {
        public Station Station { get; set; } = new Station();
        public string Track { get; set; } = string.Empty;
        public CallAction? Arrival { get; set; }
        public CallAction? Departure { get; set; }
        public bool IsStop { get; set; }
        public int SequenceNumber { get; set; }
    }
}
