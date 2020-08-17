using System.Linq;

namespace Tellurian.Trains.Planning.App.Shared
{
    public class TrainPart
    {
        public string? TrainNumber { get; set; }
        public string? LocoNumber { get; set; }
        public CallAction? FromDeparture { get; set; }
        public CallAction? ToArrival { get; set; }
        public Train? Train { get; set; }
        public override string ToString() => $"{TrainNumber}";
    }

    public static class TrainPartExtensions
    {
        public static TrainPart AsTrainPart(this Train train, string locoNumber, int fromSequenceNumber = 1, int toSequenceNumber = 0) =>
            new TrainPart
            {
                TrainNumber = $"{train.OperatorName} {train.Number}",
                FromDeparture = train.Calls.Single(c => c.SequenceNumber == fromSequenceNumber).AsDeparture(),
                ToArrival = train.Calls.Single(c => c.SequenceNumber == (toSequenceNumber == 0 ? train.Calls.Count : toSequenceNumber)).AsArrival(),
                LocoNumber = locoNumber,
                Train = train
            };
    }
}
