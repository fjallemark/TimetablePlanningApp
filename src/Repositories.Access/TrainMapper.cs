using System.Data;
using System.Resources;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;

internal static class TrainMapper
{
    private static ResourceManager Notes => App.Contracts.Resources.Notes.ResourceManager;

    public static Train ToTrain(this IDataRecord me) =>
         new()
         {
             Id = me.GetInt("TrainId"),
             OperatorName = me.GetString("TrainOperator"),
             Number = me.GetInt("TrainNumber"),
             OperationDaysFlags = me.GetByte("TrainDays").And(me.GetByte("DutyDays")),
             CategoryId = me.GetInt("CategoryId"),
             Instruction = me.GetString("TrainInstruction"),
             MaxNumberOfWaggons = me.GetInt("TrainMaxNumberOfWaggons", 0),
             MaxSpeed = me.GetInt("TrainMaxSpeed"),
             IsCargo = me.GetBool("IsCargo"),
             IsPassenger = me.GetBool("IsPassenger"),
             Color = me.GetString("Color"),
             Calls = new List<StationCall>()
         };

    public static StationCall ToStationCall(this IDataRecord me, int sequenceNumber)
    {
        var callId = me.GetInt("CallId");
        var hasDepartureTime = TimeSpan.TryParse(me.GetTime("DepartureTime", ""), out var departureTime);
        if (!hasDepartureTime)
        {
            throw new ApplicationException($"Call Id {callId} has no departure time");
        }
        var defaultArrivalTime = (departureTime - TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
        var call = new StationCall()
        {
            Id = me.GetInt("CallId"),
            TrackId = me.GetInt("TrackId", -1),
            IsStop = me.GetBool("IsStop"),
            TrackNumber = me.GetString("TrackNumber"),
            SequenceNumber = sequenceNumber,
            Station = new StationInfo
            {
                Id = me.GetInt("StationId"),
                Name = me.GetString("StationName"),
                Signature = me.GetString("StationSignature"),
                HasCombinedInstructions = me.GetBool("HasCombinedInstructions")
            },
            Arrival = new CallTime
            {
                IsHidden = me.GetBool("HideArrival"),
                IsStop = me.GetBool("IsStop"),
                Time = me.GetTime("ArrivalTime", defaultArrivalTime),
            },
            Departure = new CallTime
            {
                IsHidden = me.GetBool("HideDeparture"),
                IsStop = me.GetBool("IsStop"),
                Time = me.GetTime("DepartureTime", "")
            }
        };
        return call;
    }

    public static TrainCategory ToTrainCategory(this IDataRecord me) =>
        new()
        {
            Id = me.GetInt("Id"),
            Type = me.GetString("Type"),
            IsCargo = me.GetBool("IsCargo"),
            IsPassenger = me.GetBool("IsPassenger"),
            Prefix = me.GetString("Prefix"),
        };

}
