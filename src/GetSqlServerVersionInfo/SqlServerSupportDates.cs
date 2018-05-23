using System;
using System.Collections.Generic;

namespace GetSqlServerVersionInfo
{
    public class SqlServerSupportDates
    {
        public SqlServerSupportDates(IReadOnlyList<string> csvData)
        {
            ProductName = csvData[0];
            LifeCycleStartDate = DateTime.TryParse(csvData[1], out var lifeCycleStartDate)
                ? lifeCycleStartDate
                : (DateTime?)null;
            MainstreamSupportEndDate = DateTime.TryParse(csvData[2], out var mainStreamSupportEndDate)
                ? mainStreamSupportEndDate
                : (DateTime?)null;
            ExtendedSupportEndDate = DateTime.TryParse(csvData[3], out var extendedSupportEndDate)
                ? extendedSupportEndDate
                : (DateTime?)null;
            ServicePackSupportEndDate = DateTime.TryParse(csvData[4], out var servicePackSupportEndDate)
                ? servicePackSupportEndDate
                : (DateTime?)null;

            for (var i = 5; i < csvData.Count; i++)
            {
                Notes += csvData[i];
            }
        }

        public string ProductName { get; set; }
        public DateTime? LifeCycleStartDate { get; set; }
        public DateTime? MainstreamSupportEndDate { get; set; }
        public DateTime? ExtendedSupportEndDate { get; set; }
        public DateTime? ServicePackSupportEndDate { get; set; }
        public string Notes { get; set; }
    }
}
