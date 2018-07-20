using System;
using System.Collections.Generic;
using System.Globalization;

namespace GetSqlServerVersionInfo
{
    public class SqlServerSupportDates
    {
        public SqlServerSupportDates(IReadOnlyList<string> csvData)
        {
            ProductName = csvData[0];
            LifeCycleStartDate = DateTime.TryParse(csvData[1], 
                CultureInfo.CurrentCulture, DateTimeStyles.None, out var lifeCycleStartDate)
                ? lifeCycleStartDate
                : (DateTime?)null;
            MainstreamSupportEndDate = DateTime.TryParse(csvData[2], 
                CultureInfo.CurrentCulture, DateTimeStyles.None, out var mainStreamSupportEndDate)
                ? mainStreamSupportEndDate
                : (DateTime?)null;
            ExtendedSupportEndDate = DateTime.TryParse(csvData[3], 
                CultureInfo.CurrentCulture, DateTimeStyles.None, out var extendedSupportEndDate)
                ? extendedSupportEndDate
                : (DateTime?)null;
            ServicePackSupportEndDate = DateTime.TryParse(csvData[4], 
                CultureInfo.CurrentCulture, DateTimeStyles.None, out var servicePackSupportEndDate)
                ? servicePackSupportEndDate
                : (DateTime?)null;

            for (var i = 5; i < csvData.Count; i++)
            {
                Notes += csvData[i];
            }

            if (MainstreamSupportEndDate != null || ServicePackSupportEndDate != null) return;

            if (!ProductName.Contains("2017") && !ProductName.Contains("2016") &&
                !ProductName.Contains("2014") && !ProductName.Contains("2012") &&
                !ProductName.Contains("2008 R2")) return;

            Console.WriteLine($"NULL Dates in input string (culture={CultureInfo.CurrentCulture}): ");
            Console.WriteLine(string.Join(",", csvData));
            throw new ArgumentException("NULL date values detected, exiting.");
        }

        public string ProductName { get; set; }
        public DateTime? LifeCycleStartDate { get; set; }
        public DateTime? MainstreamSupportEndDate { get; set; }
        public DateTime? ExtendedSupportEndDate { get; set; }
        public DateTime? ServicePackSupportEndDate { get; set; }
        public string Notes { get; set; }
    }
}

