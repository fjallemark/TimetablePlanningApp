using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.App.Server.Services
{
    public class PrintedReportsService
    {
        public PrintedReportsService(IPrintedReportsStore store) => Store = store;

        private readonly IPrintedReportsStore Store;

        public Task<IEnumerable<Waybill>> GetWaybillsAsync(int layoutId) =>
            Store.GetWaybillsAsync(layoutId);

        public Task<IEnumerable<LocoSchedule>> GetLocoSchedulesAsync(int layoutId) =>
            Store.GetLocoSchedulesAsync(layoutId);

        public Task<IEnumerable<TrainsetSchedule>> GetTrainsetSchedulesAsync(int layoutId) =>
            Store.GetTrainsetSchedulesAsync(layoutId);

        public async Task<DriverDutyBooklet?> GetDriverDutyBookletAsync(int layoutId)
        {
            var booklet = await Store.GetDriverDutyBookletAsync(layoutId).ConfigureAwait(false);
            if (booklet is null) return null;
            var notes = await Store.GetTrainCallNotesAsync(layoutId).ConfigureAwait(false);
            var duties = await Store.GetDriverDutiesAsync(layoutId).ConfigureAwait(false);
            duties.MergeTrainPartsWithSameTrainNumber();
            duties.AddTrainCallNotes(notes);
            booklet.Duties = duties.OrderBy(d => d.Number).ToArray();
            return booklet;
        }

        public async Task<IEnumerable<BlockDestinations>> GetBlockDestinationsAsync(int layoutId) =>
            await Store.GetBlockDestinations(layoutId).ConfigureAwait(false);
    }
}
