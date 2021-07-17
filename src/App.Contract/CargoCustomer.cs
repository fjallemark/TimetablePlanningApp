namespace Tellurian.Trains.Planning.App.Contracts
{
    public class CargoCustomer
    {
        public string Name { get; set; } = string.Empty;
        public string Station { get; set; } = string.Empty;
        public Region Region { get; set; } = Region.DestinationDefault;
        public string Instruction { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;

        public static CargoCustomer Origin(string name, string station, string instruction = "-") =>
            new CargoCustomer { Name = name, Station = station, Instruction = instruction, Region = Region.OriginDefault };
        public static CargoCustomer Origin(string name, string station, Region region, string instruction = "-") =>
            new CargoCustomer { Name = name, Station = station, Instruction = instruction, Region = region };
        public static CargoCustomer Destination(string name, string station, string instruction = "-") =>
           new CargoCustomer { Name = name, Station = station, Instruction = instruction, Region = Region.DestinationDefault };
        public static CargoCustomer Destination(string name, string station, Region region, string instruction = "-") =>
           new CargoCustomer { Name = name, Station = station, Instruction = instruction, Region = region };
    }
}
