using System;
using System.Globalization;

namespace ClashWrapper
{
    public static class Utilities
    {
        internal static DateTimeOffset FromClashTime(string time)
        {
            return DateTimeOffset.ParseExact(time, "yyyyMMdd'T'HHmmss.fff'Z'", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal |
                DateTimeStyles.AdjustToUniversal);
        }
    }
}
