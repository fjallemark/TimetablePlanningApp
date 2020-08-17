using System.Data;
using System.Resources;
using Tellurian.Trains.Planning.App.Shared;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    public static class IDataRecordExtensions
    {
        public static string GetString(this IDataRecord me, string columnName, string defaultValue = "")
        {
            var i = me.GetOrdinal(columnName);
            if (me.IsDBNull(i)) return defaultValue;
            var s = me.GetString(me.GetOrdinal(columnName));
            return (string.IsNullOrWhiteSpace(s)) ? defaultValue : s;
        }

        public static string GetStringResource(this IDataRecord me, string columnName, ResourceManager resourceManager, string defaultValue = "")
        {
            var resourceKey = me.GetString(columnName, defaultValue);
            if (resourceKey.HasValue())
            {
                var resourceValue = resourceManager.GetString(resourceKey);
                if (resourceValue.HasValue()) return resourceValue;
                return resourceKey;
            }
            return defaultValue;
        }

        public static byte GetByte(this IDataRecord me, string columnName)
        {
            var i = me.GetOrdinal(columnName);
            if (me.IsDBNull(i)) return 0; ;
            return me.GetByte(i);
        }

        public static int GetInt16(this IDataRecord me, string columnName, short defaultValue = 0)
        {
            var i = me.GetOrdinal(columnName);
            if (me.IsDBNull(i)) return defaultValue;
            return me.GetInt16(i);
        }

        public static int GetInt32(this IDataRecord me, string columnName, int defaultValue = 0)
        {
            var i = me.GetOrdinal(columnName);
            if (me.IsDBNull(i)) return defaultValue;
            return me.GetInt32(i);
        }

        public static string GetTime(this IDataRecord me, string columnName, string defaultValue = "")
        {
            var i = me.GetOrdinal(columnName);
            if (me.IsDBNull(i)) return defaultValue;
            return me.GetDateTime(i).ToString("HH:mm");
        }

        public static bool GetBool(this IDataRecord me, string columnName)
        {
            var i = me.GetOrdinal(columnName);
            if (me.IsDBNull(i)) return false;
            return me.GetBoolean(i);
        }
    }
}
