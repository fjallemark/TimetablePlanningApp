using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Server.Services
{
    public class PrintedReportsService
    {
        public PrintedReportsService(IPrintedReportsStore store) => Store = store;

        private readonly IPrintedReportsStore Store;

        public Task<IEnumerable<BlockDestinations>> GetBlockDestinationsAsync(int layoutId) =>
            Store.GetBlockDestinationsAsync(layoutId);

        public async Task<DriverDutyBooklet?> GetDriverDutyBookletAsync(int layoutId)
        {
            var booklet = await Store.GetDriverDutyBookletAsync(layoutId).ConfigureAwait(false);
            if (booklet is null) return null;
            var notes = await Store.GetTrainCallNotesAsync(layoutId).ConfigureAwait(false);
            var duties = await Store.GetDriverDutiesAsync(layoutId).ConfigureAwait(false);
            duties.MergeTrainPartsWithSameTrainNumber();
            duties.AddTrainCallNotes(notes);
            booklet.Duties = duties.OrderBy(d => d.DisplayOrder).ToArray();
            return booklet;
        }

        public Task<IEnumerable<LocoSchedule>> GetLocoSchedulesAsync(int layoutId) =>
            Store.GetLocoSchedulesAsync(layoutId);

        public async Task<StationDutyBooklet?> GetStationDutyBookletAsync(int layoutId) =>
            await GetStationDutyBookletAsync(layoutId, false);

        public async Task<StationDutyBooklet?> GetStationDutyBookletAsync(int layoutId, bool includeAllTrains = false)
        {
            var booklet = await Store.GetStationDutyBookletAsync(layoutId).ConfigureAwait(false);
            if (booklet is null) return null;
            var data = await Store.GetStationDutiesDataAsync(layoutId).ConfigureAwait(false);
            var trains = await Store.GetTrainsAsync(layoutId);
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
                        if (stretch.Stations.Any(s => s.Station.Id == trainSection.FromStationId) && stretch.Stations.Any(s => s.Station.Id == trainSection.ToStationId))
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

        public Task<IEnumerable<TrainDeparture>> GetTrainDeparturesAsync(int layoutId) =>
            Store.GetTrainDeparturesAsync(layoutId);

        public Task<IEnumerable<VehicleSchedule>> GetTrainsetSchedulesAsync(int layoutId) =>
            Store.GetTrainsetSchedulesAsync(layoutId);


        public Task<IEnumerable<Waybill>> GetWaybillsAsync(int layoutId) =>
            Store.GetWaybillsAsync(layoutId);
    }

    internal static class TrainExtensions
    {
        public static IEnumerable<TimetableTrainSection> GetTimetableTrainSections(this Train me)
        {
            var result = new List<TimetableTrainSection>(50);
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
                TrainNumber = me.Number,
                OperationDays = me.OperationDays(),
                Color = me.Color
            }));
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
                    TrainNumber = me.Number,
                    OperationDays = me.OperationDays(),
                    Color = me.Color
                });
            }
            return result;
        }
    }
}
