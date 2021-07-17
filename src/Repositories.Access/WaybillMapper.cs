using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    public static class WaybillMapper
    {
        public static Waybill AsWaybill(IDataRecord me) =>
            new()
            {
                Cargo = me.GetString("CargoName"),
                Class = me.GetString("Class", "-"),
                OperationDaysFlags = me.GetByte("OperationDaysFlag"),
                Epoch = "-",
                OperatorName = me.GetString("Operator", "-"),
                Origin = new CargoCustomer
                {
                    Name = me.GetString("FromCustomerName"),
                    Station = me.GetString("FromStationName"),
                    Instruction = me.GetString("LoadingNote", "-"),
                    Language = me.GetString("OriginLanguageCode"),
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
                    Language = me.GetString("DestinationLanguageCode"),
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
