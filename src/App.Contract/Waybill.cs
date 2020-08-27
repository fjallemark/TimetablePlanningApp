using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace Tellurian.Trains.Planning.App.Contract
{
    public class Waybill
    {
        public CargoCustomer? Origin { get; set; }
        public CargoCustomer? Destination { get; set; }
        public string Cargo { get; set; } = "TOM";
        public string OperatorName { get; set; } = string.Empty;
        public string Epoch { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        [Obsolete("Use " + nameof(OperationDays))]
        public string Days { get; set; } = string.Empty;
        public OperationDays OperationDays { get; set; } = new OperationDays();
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
}
