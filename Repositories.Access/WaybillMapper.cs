using System.Data;
using Tellurian.Trains.Planning.App.Shared;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    public static class WaybillMapper
    {
        public static Waybill AsWaybill(IDataRecord me) =>
            new Waybill
            {
                Cargo = me.GetString("CargoName"),
                Class = me.GetString("Class", "-"),
                Days = me.GetString("DaysShort"),
                Epoch = "-",
                Operator = me.GetString("Operator", "-"),
                Origin = new CargoCustomer
                {
                    Name = me.GetString("FromCustomerName"),
                    Station = me.GetString("FromStationName"),
                    Instruction = me.GetString("LoadingNote", "-"),
                    Region = new Region
                    {
                        Name = me.GetString("FromRegionName"),
                        BackColor = me.GetString("OriginBackColor"),
                        TextColor = me.GetString("OriginForeColor")
                    }
                },
                Destination = new CargoCustomer
                {
                    Name = me.GetString("ToCustomerName"),
                    Station = me.GetString("ToStationName"),
                    Instruction = me.GetString("UnloadingNote", "-"),
                    Region = new Region
                    {
                        Name = me.GetString("ToRegionName"),
                        BackColor = me.GetString("DestinationBackColor"),
                        TextColor = me.GetString("DestinationForeColor")
                    }
                }
            };
    }
}
