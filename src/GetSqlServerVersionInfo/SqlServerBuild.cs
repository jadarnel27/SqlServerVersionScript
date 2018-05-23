using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GetSqlServerVersionInfo
{
    public class SqlServerBuild
    {
        public SqlServerBuild(List<HtmlNode> cells, string majorVersionName)
        {
            _buildNumber = cells[0].InnerText.Replace("&nbsp;", string.Empty);
            ServicePack = cells[1].InnerText.Replace("&nbsp;", string.Empty);
            _updates = cells[2].InnerText.Replace("&nbsp;", string.Empty);
            _knowledgeBaseArticleId = cells[3].InnerText.Replace("&nbsp;", string.Empty);
            _releaseDate = DateTime.Parse(cells[4].InnerText.Replace("&nbsp;", string.Empty));
            MajorVersionName = majorVersionName;
        }

        [Obsolete("The other constructor should generally be used, this is a " +
                  "workaround for manaully adding builds that aren't present" +
                  "on the support site", false)]
        public SqlServerBuild(string buildNumber, string servicePack, string updates,
            string knowledgeBaseArticleId, DateTime? releaseDate, string majorVersionName)
        {
            _buildNumber = buildNumber;
            ServicePack = servicePack;
            _updates = updates;
            _knowledgeBaseArticleId = knowledgeBaseArticleId;
            _releaseDate = releaseDate;
            MajorVersionName = majorVersionName;
        }

        private readonly string _buildNumber;
        private readonly string _knowledgeBaseArticleId;
        public readonly string ServicePack;
        private readonly string _updates;
        private readonly DateTime? _releaseDate;
        private DateTime? _mainstreamSupportEndDate;
        private DateTime? _extendedSupportEndDate;

        public short MajorVersionNumber => short.Parse(_buildNumber.Split('.')[0]);
        public int MinorVersionNumber => int.Parse(_buildNumber.Split('.')[2]);
        public string Branch => $"{(ServicePack == "None" ? "RTM" : ServicePack)} " +
                                $"{(_updates.Contains("RTW") ? string.Empty : _updates)}";

        public string Url => string.IsNullOrEmpty(_knowledgeBaseArticleId)
            ? string.Empty
            : string.Join(", ", _knowledgeBaseArticleId.Split(", ").Select(s => "https://support.microsoft.com/en-us/help/" +
                                                                                $"{s}").ToList());
        public string ReleaseDateSqlString => $"'{_releaseDate:yyyy-MM-dd}'";
        public string MajorVersionName { get; }
        public string MinorVersionName => Branch.Replace("SP", "Service Pack ")
            .Replace("CU", "Cumulative Update ");

        public string MainstreamSupportEndDateSqlString => _mainstreamSupportEndDate.HasValue 
            ? $"'{_mainstreamSupportEndDate:yyyy-MM-dd}'" 
            : "NULL";

        public string ExtendedSupportEndDateSqlString => _extendedSupportEndDate.HasValue 
            ? $"'{_extendedSupportEndDate:yyyy-MM-dd}'" 
            : "NULL";

        public void SetSupportDates(DateTime? mainstreamSupportEndDate, 
            DateTime? extendedSupportEndDate, DateTime? servicePackSupportEndDate)
        {
            _mainstreamSupportEndDate = mainstreamSupportEndDate ?? servicePackSupportEndDate;
            _extendedSupportEndDate = extendedSupportEndDate ?? servicePackSupportEndDate;
        }
    }
}
