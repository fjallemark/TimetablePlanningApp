using System.Collections.Generic;
using Tellurian.Trains.Planning.App.Shared;
using Tellurian.Trains.Planning.Repositories;

namespace Tellurian.Trains.Planning.App.Server.Services
{
    public class DriverDutiesService
    {
        public DriverDutiesService(IRepository repository)
        {
            Repository = repository;
        }
        private readonly IRepository Repository;

        public DriverDutyBooklet? GetDriverDutyBooklet(string scheduleName)
        {
            if (scheduleName == "example") return DriverDutyBooklet.Example;
            var result = Repository.GetDriverDutyBooklet(scheduleName);
            if (result is null) return null;
            result.MergeTrainPartsWithSameTrainNumber();
            result.AddTrainCallNotes(Repository.GetTrainCallNotes(scheduleName));
            return result;
        }
    }
}
