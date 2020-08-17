using System;
using System.Collections.Generic;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Shared
{
    public class DriverDutyBooklet
    {
        public string ScheduleName { get; set; } = string.Empty;

        public ICollection<DriverDuty> Duties { get; set; } = Array.Empty<DriverDuty>();

        public static DriverDutyBooklet Example => new DriverDutyBooklet
        {
            ScheduleName = "Demo",
            Duties = new[]
            {
                new DriverDuty
                {
                    Operator = "Green Cargo",
                         OperationDays = ((byte)31).OperationDays(),
                         Difficulty = 2,
                         Name = "Chemicals transport",
                         Number=22,
                         StartTime = "11:40",
                         EndTime = "15:38",
                         Parts = new [] {
                             new DutyPart(DriverDutyBookletExtensions.Train51, new Loco {  OperatorName="GC", Number=52}, 22, 27)
                         }
                }
            }
        };
    }

    public static class DriverDutyBookletExtensions
    {
        public static void AddTrainCallNotes(this DriverDutyBooklet me, IEnumerable<TrainCallNote> trainCallNotes)
        {
            var notes = trainCallNotes.ToDictionary();
            foreach (var duty in me.Duties)
            {
                foreach (var part in duty.Parts)
                {
                    foreach (var call in part.Train.Calls)
                    {
                        call.AddAutomaticNotes(part);
                        call.AddGeneratedNotes(part.Train, notes.Item(call.Id));
                    }
                }
            }
        }

        private static IDictionary<int, IList<TrainCallNote>> ToDictionary(this IEnumerable<TrainCallNote> me)
        {
            var result = new Dictionary<int, IList<TrainCallNote>>(1000);
            foreach (var n in me)
            {
                var key = n.CallId;
                if (!result.ContainsKey(key)) result.Add(key, new List<TrainCallNote>());
                result[key].Add(n);
            }
            return result;
        }

        private static IList<TrainCallNote> Item(this IDictionary<int, IList<TrainCallNote>> me, int callId) =>
            me.ContainsKey(callId) ? me[callId] : Array.Empty<TrainCallNote>();

        public static Train Train51 => new Train
        {
            OperatorName = "GC",
            Number = "51",
            Calls = new[]
            {
                new StationCall {
                    Id = 21,
                    SequenceNumber = 1,
                    Station = new Station { Name="Göteborg Kville", Signature="Gkv"},
                    Track = "4",
                    Departure = CallTime.Create("12:10", "Medtar vagnsgrupp 653 tankvagnar."),
                },
                new StationCall {
                    Id=22,
                    SequenceNumber = 2,
                    Station = new Station { Name="Göteborg Sävenäs", Signature="Gsv"},
                    Track = "4",
                    Arrival = CallTime.Create("12:19", "Gör rundgång"),
                    Departure = new CallTime{ Time="12:49" },
                },
                new StationCall {
                    Id=23,
                    SequenceNumber = 3,
                    IsStop = false,
                    Station = new Station { Name="Tingstad", Signature="Tsd"},
                    Track = "4",
                    Arrival = new CallTime { Time="12:54", IsHidden=true},
                    Departure = new CallTime{ Time="12:55" },
                },
                new StationCall {
                    Id=24,
                    SequenceNumber = 4,
                    IsStop = true,
                    Station = new Station { Name="Säve", Signature="Sve"},
                    Track = "4",
                    Arrival = CallTime.Create ("13:05", "Möter persontåg 3766."),
                    Departure = new CallTime{ Time="13:08" },
                },
                new StationCall {
                    Id=25,
                    SequenceNumber = 5,
                    IsStop = false,
                    Station = new Station { Name="Ytterby", Signature="Yb"},
                    Track = "4",
                    Arrival = new CallTime { Time="13:13", IsHidden=true},
                    Departure = new CallTime{ Time="13:14" },
                },
                new StationCall {
                    Id = 26,
                    SequenceNumber = 6,
                    IsStop = false,
                    Station = new Station { Name="Kode", Signature="Kde"},
                    Track = "4",
                    Arrival = new CallTime { Time="13:20", IsHidden=true},
                    Departure = new CallTime{ Time="13:21" },
                },
                new StationCall {
                    Id = 27,
                    SequenceNumber = 7,
                    IsStop = false,
                    Station = new Station { Name="Stora Höga", Signature="Sth"},
                    Track = "4",
                    Arrival = new CallTime { Time="13:29", IsHidden=true},
                    Departure = new CallTime{ Time="13:30" },
                },
                new StationCall {
                    Id = 28,
                    SequenceNumber = 8,
                    IsStop = true,
                    Station = new Station { Name="Stenungsund", Signature="Snu"},
                    Track = "4",
                    Arrival = CallTime.Create( "13:38", "Vagnarna växlas in till respektive godskund enligt fraktsedel."),
                }
            }
        };
    }
}
