﻿using System;
using System.Data;
using System.Globalization;
using System.Resources;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    public static class IDataRecordExtensions
    {
        public static string GetString(this IDataRecord me, string columnName, string defaultValue = "")
        {
            var i = me.GetColumIndex(columnName, false);
            if (i < 0 || me.IsDBNull(i)) return defaultValue;
            var s = me.GetString(me.GetOrdinal(columnName));
            return (string.IsNullOrWhiteSpace(s)) ? defaultValue : s;
        }

        public static string GetStringResource(this IDataRecord me, string columnName, ResourceManager resourceManager, string defaultValue = "")
        {
            var resourceKey = me.GetString(columnName, defaultValue);
            if (resourceKey.HasValue())
            {
                var resourceValue = resourceManager.GetString(resourceKey, CultureInfo.CurrentCulture);
                if (resourceValue.HasValue()) return resourceValue;
                return resourceKey;
            }
            return defaultValue;
        }

        public static byte GetByte(this IDataRecord me, string columnName)
        {
            var i = me.GetColumIndex(columnName);
            if (me.IsDBNull(i)) return 0;
            var value = me.GetValue(i);
            if (value is byte a) return a;
            if (value is double b) return (byte)b;
            throw new InvalidOperationException(columnName);
        }

         public static int GetInt(this IDataRecord me, string columnName, short defaultValue = 0)
        {
            var i = me.GetColumIndex(columnName, false);
            if (i < 0) return defaultValue;
            if (me.IsDBNull(i)) return defaultValue;
            var value = me.GetValue(i);
            if (value is int b) return b;
            if (value is short a) return a;
            throw new InvalidOperationException(columnName);
        }

        public static double GetDouble(this IDataRecord me, string columnName, double defaultValue = 0)
        {
            var i = me.GetColumIndex(columnName);
            if (me.IsDBNull(i)) return defaultValue;
            var value = me.GetValue(i);
            if (value is double b) return b;
            if (value is float a) return a;
            throw new InvalidOperationException(columnName);
        }

        public static string GetTime(this IDataRecord me, string columnName, string defaultValue = "")
        {
            var i = me.GetColumIndex(columnName);
            if (me.IsDBNull(i)) return defaultValue;
            return me.GetDateTime(i).ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        public static TimeSpan GetTimeAsTimespan(this IDataRecord me, string columnName)
        {
            var i = me.GetColumIndex(columnName);
            if (me.IsDBNull(i)) return TimeSpan.MinValue;
            var value = me.GetValue(i);
            if (value is DateTime d) return new TimeSpan(d.Hour, d.Minute, 0);
            throw new InvalidOperationException(columnName);
        }

        public static double GetTimeAsDouble(this IDataRecord me, string columnName)
        {
            var t = me.GetTimeAsTimespan(columnName);
            return t.TotalMinutes;
        }

        public static bool GetBool(this IDataRecord me, string columnName)
        {
            var i = me.GetColumIndex(columnName);
            if (me.IsDBNull(i)) return false;
            var value = me.GetValue(i);
            if (value is bool a) return a;
            if (value is Int16 b) return b != 0;
            if (value is double c) return c != 0;
            throw new InvalidOperationException(columnName);
        }

        public static bool IsDBNull(this IDataRecord me, string columnName)
        {
            var i = me.GetOrdinal(columnName);
            return me.IsDBNull(i);
        }

        private static int GetColumIndex(this IDataRecord me, string columnName, bool throwOnNotFound = true)
        {
            var i = -1;
            try { i = me.GetOrdinal(columnName); }
            catch (IndexOutOfRangeException) {
                if (throwOnNotFound) throw new InvalidOperationException(columnName);
            }
            return i;
        }
    }
}
