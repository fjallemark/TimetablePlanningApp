using System.Diagnostics;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Server.Extensions;

namespace Tellurian.Trains.Planning.App.Server.Services;

public class PrintedReportsService(IPrintedReportsStore store)
{
    private readonly IPrintedReportsStore Store = store;

    public Task<IEnumerable<BlockDestinations>> GetBlockDestinationsAsync(int layoutId) =>
        Store.GetBlockDestinationsAsync(layoutId);

    public async Task<DriverDutyBooklet?> GetDriverDutyBookletAsync(int layoutId)
    {
        var booklet = await Store.GetDriverDutyBookletAsync(layoutId).ConfigureAwait(false);
        if (booklet is null) return null;
        var notes = await Store.GetTrainCallNotesAsync(layoutId).ConfigureAwait(false);
        var readNotesCount = notes.Count();
        var duties = await Store.GetDriverDutiesAsync(layoutId).ConfigureAwait(false);
        duties.MergeTrainPartsWithSameTrainNumber();
        var addedNotesCount = duties.AddTrainCallNotes(notes);
        booklet.Duties = duties.OrderBy(d => d.DisplayOrder).ToArray();
        return booklet;
    }

    public Task<IEnumerable<StationInstruction>> GetStationInstructionsAsync(int layoutId) =>
        Store.GetStationInstructionsAsync(layoutId);

    public async Task<Layout?> GetLayout(int layoutId) => await Store.GetLayout(layoutId).ConfigureAwait(false);

    public async Task<IEnumerable<LocoSchedule>> GetLocoSchedulesAsync(int layoutId)
    {
        var schedules = await Store.GetLocoSchedulesAsync(layoutId);
        return schedules.MergePartsOfSameTrain();
    }

    public Task<IEnumerable<ShuntingLoco>> GetShuntingLocosAsync(int layoutId) =>
         Store.GetShuntingLocosAsync(layoutId);

    public async Task<StationDutyBooklet?> GetStationDutyBookletAsync(int layoutId) =>
        await GetStationDutyBookletAsync(layoutId, false);

    public async Task<StationDutyBooklet?> GetStationDutyBookletAsync(int layoutId, bool includeAllTrains = false)
    {
        var booklet = await Store.GetStationDutyBookletAsync(layoutId).ConfigureAwait(false);
        if (booklet is null) return null;
        var data = await Store.GetStationDutiesDataAsync(layoutId).ConfigureAwait(false);
        var trains = await Store.GetTrainsAsync(layoutId).ConfigureAwait(false);
        var notes = await Store.GetTrainCallNotesAsync(layoutId).ConfigureAwait(false);
        var duties = data.AsStationDuties(trains, notes, includeAllTrains);
        booklet.Duties = duties;
        return booklet;
    }

    public async Task<IEnumerable<TimetableStretch>> GetTimetableStretchesAsync(int layoutId, string? stretchNumber = null)
    {
        var stretches = await Store.GetTimetableStretchesAsync(layoutId, stretchNumber).ConfigureAwait(false);
        var trains = await Store.GetTrainsAsync(layoutId).ConfigureAwait(false);
        foreach (var train in trains)
        {
            foreach (var trainSection in train.GetTimetableTrainSections())
            {
                foreach (var stretch in stretches)
                {
                    if (stretch.Stations.Any(s => s.Station.Id == trainSection.FromStationId) &&
                        stretch.Stations.Any(s => s.Station.Id == trainSection.ToStationId))
                    {
                        stretch.TrainSections.Add(trainSection);
                    }
                }
            }
        }
        foreach (var stretch in stretches)
        {
            stretch.StartHour = stretches.Min(s => s.FirstHour());
            stretch.EndHour = stretches.Max(s => s.LastHour());
        }
        return stretches;
    }

    public async Task<IEnumerable<StationTrainOrder>> GetStationsTrainOrder(int layoutId)
    {
        var items = await Store.GetStationsTrainOrder(layoutId);
        var notes = await Store.GetTrainCallNotesAsync(layoutId);
        return items.WithNotes(notes.ToArray());
    }

    public Task<IEnumerable<Train>> GetTrainsAsync(int layoutId, string? operatorSignature = null) =>
        Store.GetTrainsAsync(layoutId, operatorSignature);

    public Task<IEnumerable<TrainDeparture>> GetTrainDeparturesAsync(int layoutId) =>
        Store.GetTrainDeparturesAsync(layoutId);

    public async Task<IEnumerable<VehicleSchedule>> GetTrainsetSchedulesAsync(int layoutId)
    {
        var schedules = await Store.GetTrainsetSchedulesAsync(layoutId);
        return schedules.SchedulesToPrint();
    }

    public Task<IEnumerable<VehicleStartInfo>> GetVehicleStartInfoAsync(int layoutId) => Store.GetVehicleStartInfosAsync(layoutId);
}

internal static class StationTrainOrderExtensions
{
    public static IEnumerable<StationTrainOrder> WithNotes(this IEnumerable<StationTrainOrder> stations, IEnumerable<TrainCallNote> notes)
    {
        var notesDictionary = notes.ToDictionary();
        foreach (var station in stations)
        {
            foreach (var train in station.Trains)
            {
                if (!train.IsStop) train.Notes.Add(Contracts.Resources.Notes.NoStop);
                if (notesDictionary.TryGetValue(train.CallId, out List<TrainCallNote>? value))
                {
                    var callNotes = value.Where(n => (n.IsStationNote || n.IsShuntingNote) && (train.IsArrival == n.IsForArrival || train.IsDeparture == n.IsForDeparture)).OrderBy(n => n.DisplayOrder);
                    foreach (var note in callNotes) note.TrainInfo = train.ToTrainInfo();
                    var texts = callNotes.SelectMany(n => n.ToNotes().Select(n => n.Text));
                    train.Notes.AddRange(texts);
                }
            }
        }
        return stations;
    }
}

internal static class TrainExtensions
{
    public static IEnumerable<TimetableTrainSection> GetTimetableTrainSections(this Train me)
    {
        var result = new List<TimetableTrainSection>(50);
        // Add track usage
        result.AddRange(me.Calls.Select(c => new TimetableTrainSection
        {
            FromStationId = c.Station.Id,
            FromTrackId = c.TrackId,
            ToStationId = c.Station.Id,
            ToTrackId = c.TrackId,
            IsCargo = me.IsCargo,
            IsPassenger = me.IsPassenger,
            StartTime = c.Arrival.OffsetMinutes(),
            EndTime = c.Departure.OffsetMinutes(),
            OperatorSignature = me.OperatorName,
            TrainNumber = me.Number,
            OperationDays = me.OperationDays(),
            Color = me.Color
        }));
        // Add train movements
        for (var i = 0; i < me.Calls.Count - 1; i++)
        {
            result.Add(new TimetableTrainSection
            {
                FromStationId = me.Calls[i].Station.Id,
                FromTrackId = me.Calls[i].TrackId,
                ToStationId = me.Calls[i + 1].Station.Id,
                ToTrackId = me.Calls[i + 1].TrackId,
                IsCargo = me.IsCargo,
                IsPassenger = me.IsPassenger,
                StartTime = me.Calls[i].Departure.OffsetMinutes(),
                EndTime = me.Calls[i + 1].Arrival.OffsetMinutes(),
                OperatorSignature = me.OperatorName,
                TrainNumber = me.Number,
                OperationDays = me.OperationDays(),
                Color = me.Color
            });
        }
        return result;
    }
}
