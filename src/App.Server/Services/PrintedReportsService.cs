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

        public async Task<IEnumerable<DriverDuty>> GetDriverDutiesAsync(int layoutId)
        {
            var notes = await Store.GetTrainCallNotesAsync(layoutId).ConfigureAwait(false);
            var duties = await Store.GetDriverDutiesAsync(layoutId).ConfigureAwait(false);
            duties.MergeTrainPartsWithSameTrainNumber();
            duties.AddTrainCallNotes(notes);
            return duties.OrderBy(d => d.Number).AsEnumerable();
        }
    }
}
