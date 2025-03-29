using System.Diagnostics;

namespace Tellurian.Trains.Planning.App.Contracts;
public class StationDutyData
{
    public int StationId { get; set; }
    public required string StationName { get; set; }    
    public int DisplayOrder { get; set; }
    public string LayoutName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public DateTime ValidFromDate { get; set; }
    public DateTime ValidToDate { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public bool HasCombinedInstructions { get; set; }
    public int Difficulty { get; set; }

    public ICollection<Instruction> StationInstructions { get; set; } = new List<Instruction>();
    public ICollection<Instruction> ShuntingInstructions { get; set; } = new List<Instruction>();
}

public static class StationDutyDataExtensions
{
    public static ICollection<StationDuty> AsStationDuties(this IEnumerable<StationDutyData> items, IEnumerable<Train> trains, IEnumerable<TrainCallNote> notes, bool includeAllTrains = false)
    {
        var duties = new List<StationDuty>(50);
        foreach (var item in items.OrderBy(i => i.DisplayOrder))
        {
            if (item.HasCombinedInstructions)           
            {
                var duty = item.AsStationDuty(
                    StationDutyType.Single,
                    $"{Resources.Notes.TrainClearance} {Resources.Notes.And.ToLowerInvariant()} {Resources.Notes.Shunting}",
                    item.StationInstructions.Concat(item.ShuntingInstructions).MergeInstructions(),
                    null
                );
                duty.Calls = duty.AsStationCallsWithAction(trains, notes, includeAllTrains);
                duties.Add(duty);
            }
            else
            {
                var stationDuty = item.AsStationDuty(StationDutyType.TrainClearance, Resources.Notes.TrainClearance, item.StationInstructions, null);
                stationDuty.Calls = stationDuty.AsStationCallsWithAction(trains, notes, includeAllTrains);
                duties.Add(stationDuty);

                var shuntingDuty = item.AsStationDuty(StationDutyType.Shunting, Resources.Notes.Shunting, null, item.ShuntingInstructions);
                shuntingDuty.Calls = shuntingDuty.AsStationCallsWithAction(trains, notes);
                duties.Add(shuntingDuty);
            }
        }
        return duties;
    }

    private static ICollection<Instruction>? MergeInstructions(this IEnumerable<Instruction> instructions)
    {
        var result = new List<Instruction>();
        var perLanguage = instructions.GroupBy(x => x.Language).ToList();
        foreach(var i in perLanguage)
        {
            var language = i.Key;
            var instruction = new Instruction() { Language = language, Markdown = string.Join("\r\n\n", i.Select(i => i.Markdown)) };
            result.Add(instruction);
        }
        return result;
    }

    private static ICollection<StationCallWithAction> AsStationCallsWithAction(this StationDuty me, IEnumerable<Train> trains, IEnumerable<TrainCallNote> notes, bool includeAllTrains = false)
    {
        var result = new List<StationCallWithAction>();
        var trainsAtStation = trains.Where(t => t.Calls.Any(c => c.Station.Id == me.StationId)).ToList();
        foreach (var train in trainsAtStation)
        {
            var calls = train.Calls.Where(c => c.Station.Id == me.StationId);
            var callNotes = notes
                .Where(n => (n.IsStationNote || n.IsShuntingNote) && n is not TrainMeetCallNote && calls.Any(c => c.Id == n.CallId));


            foreach (var note in callNotes) note.TrainInfo = train;
            foreach (var call in calls)
            {

                if (call.IsStop)
                {
                    if (call.HasArrivalTime() )
                    {
                        var item = new StationCallWithAction(train, call, true, me.StationDutyType == StationDutyType.Shunting);
                        item.Train.OverriddenOrigin = train.Origin;
                        item.AddNotes(callNotes.OrderBy(n => n.DisplayOrder));
                        result.Add(item);
                        ;
                    }

                    if (call.HasDepartureTime() )
                    {
                        var item = new StationCallWithAction(train, call, false, me.StationDutyType == StationDutyType.Shunting);
                        item.AddNotes(callNotes.OrderBy(n => n.DisplayOrder));
                        item.Train.OverriddenDestination = train.Destination;
                        result.Add(item);
                    }
                }
                else if (includeAllTrains)
                {
                    if (call.Arrival is not null)
                    {
                        var item = new StationCallWithAction(train, call, true, me.StationDutyType == StationDutyType.Shunting);
                        item.Train.OverriddenOrigin = train.Origin;
                    }
                    if (call.Departure is not null)
                    {
                        var item =new StationCallWithAction(train, call, false, me.StationDutyType == StationDutyType.Shunting);
                        item.Train.OverriddenDestination = train.Destination;
                    }
                }
            }
        }
        return result.Where(c => includeAllTrains || c.Notes.Any()).OrderBy(c => c.SortTime).ToArray();
    }

     private static StationDuty AsStationDuty(this StationDutyData data, StationDutyType dutyType, string description, ICollection<Instruction>? stationInstructions, ICollection<Instruction>? shuntingInstructions) =>
        new()
        {
            LayoutName = data.LayoutName,
            StationId = data.StationId,
            StationName = data.StationName,
            Number = data.Signature,
            ValidFromDate = data.ValidFromDate,
            ValidToDate = data.ValidToDate,
            Difficulty = data.Difficulty,
            StartTime = $"{data.StartHour:00}:00",
            EndTime = $"{data.EndHour:00}:00",
            Description = description,
            StationInstructions = stationInstructions,
            ShuntingInstructions = shuntingInstructions,
            StationDutyType = dutyType,
        };
}
