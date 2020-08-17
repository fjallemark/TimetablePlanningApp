using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            result.AddTrainCallNotes(GetTrainCallNotes(scheduleName));
            return result;
        }

        private IEnumerable<TrainCallNote> GetTrainCallNotes(string scheduleName)
        {
            var result = new List<TrainCallNote>(1000);
            result.AddRange(Repository.GetManualTrainStationCallNotes(scheduleName));
            result.AddRange(Repository.GetDepartureTrainsetsCallNotes(scheduleName));
            result.AddRange(Repository.GetArrivalTrainsetsCallNotes(scheduleName));
            result.AddRange(Repository.GetTrainContinuationNumberCallNotes(scheduleName));
            return result;
        }

    }
}
