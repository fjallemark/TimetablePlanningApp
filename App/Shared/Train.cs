using System;
using System.Collections.Generic;

#pragma warning disable CA2227 // Collection properties should be read only

namespace Tellurian.Trains.Planning.App.Contract
{
    public class TrainInfo
    {
        public string Number { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public OperationDays OperationDays { get; set; } = new OperationDays();
        public override string ToString() => $"{OperatorName} {Number} {OperationDays.ShortName}";
    }

    public class Train : TrainInfo
    {
        public int MaxSpeed { get; set; }
        public int MaxNumberOfWaggons { get; set; }
        public string Instruction { get; set; } = string.Empty;
        public IList<StationCall> Calls { get; set; } = Array.Empty<StationCall>();
        public override string ToString() => $"{base.ToString()} {Calls.Count} calls";
        public static Train Example => new Train
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
