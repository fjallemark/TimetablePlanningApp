namespace Tellurian.Trains.Planning.App.Contracts;

public class Waybill
{
    public CargoCustomer? Origin { get; set; }
    public CargoCustomer? Destination { get; set; }
    public string Cargo { get; set; } = "TOM";
    public string OperatorName { get; set; } = string.Empty;
    public string Epoch { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public byte OperationDaysFlags { get; set; } 
}

public static class WaybillExtensions
{ 
    public static IEnumerable<string> LabelResourceKeys => new[]
    {
        "Destination",
        "Origin",
        "Consignee",
        "Shipper",
        "Carrier",
        "Cargo",
        "Class",
        "Epoch",
        "Instructions"
    };
}
