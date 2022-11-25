using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuctionProject.Repository
{
    public static class TimeZonesManagement
    {
        public static TimeZoneInfo GetTimeZoneInfo()
        {
           TimeZoneInfo tzi =  TimeZoneInfo.FindSystemTimeZoneById("Syria Standard Time");
           return tzi;
        }
    }
}