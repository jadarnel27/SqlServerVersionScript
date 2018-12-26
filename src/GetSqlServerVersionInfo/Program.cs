using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace GetSqlServerVersionInfo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                TryMain(args);
            }
            catch(Exception e)
            {
                Console.WriteLine("An error occurred.  Details to follow:");
                Console.WriteLine();
                Console.WriteLine(e);
            }
            finally
            {
                Console.ReadLine();
            }
        }

        private static void TryMain(string[] args)
        {
            var supportDates = GetSqlServerSupportDatesFromMicrosoftWebsite();
            var builds = GetSqlServerBuildsFromMicrosoftWebsite(supportDates);
            var sqlScript = BuildSqlScript(builds);

            var file = GetDestinationFileFromCommandLineArguments(args);
            File.WriteAllText(file.FullName, sqlScript);

            Console.WriteLine($"SqlServerVersions.sql was saved to {file.FullName}");
        }

        private static FileInfo GetDestinationFileFromCommandLineArguments(string[] args)
        {
            var destinationPathArgument =
                args.Length < 1 || string.IsNullOrWhiteSpace(args[0])
                    ? @".\"
                    : args[0];
            var destinationPath = Path.Combine(destinationPathArgument,
                "SqlServerVersions.sql");

            return new FileInfo(destinationPath);
        }

        private static List<SqlServerSupportDates> GetSqlServerSupportDatesFromMicrosoftWebsite()
        {
            const string lifeCycleCsvDownloadLink =
                "https://support.microsoft.com/api/lifecycle/" +
                "GetProductsLifecycle" +
                "?query=%7B%22names%22:%5B%22SQL%2520Server%22%5D,%22years%22:" +
                "%220%22,%22gdsId%22:0,%22export%22:true%7D";
            var supportDates = new List<SqlServerSupportDates>();

            using (var client = new HttpClient())
            using (var res = client.GetAsync(lifeCycleCsvDownloadLink).GetAwaiter().GetResult())
            using (var content = res.Content)
            {
                var data = content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (data == null) throw new Exception("We did not receive any CSV data from Microsoft.");

                var lines = data.Split(Environment.NewLine,
                    StringSplitOptions.RemoveEmptyEntries);

                // The first line of the CSV is a header row, so it can be skipped
                foreach (var line in lines.Skip(1).ToList())
                {
                    supportDates.Add(new SqlServerSupportDates(line.Split(',')));
                }

                supportDates = supportDates
                    // The order by is important, because later code needs to 
                    // find the first support date that matches the product name 
                    // + the service pack level.  So we want "SQL Server 2016" to 
                    // get the support date for RTM, thus it should be first in 
                    // the list
                    .OrderBy(p => p.LifeCycleStartDate)
                    .ToList();
            }

            return supportDates;
        }

        private static List<SqlServerBuild> GetSqlServerBuildsFromMicrosoftWebsite(
            IReadOnlyCollection<SqlServerSupportDates> supportDates)
        {
            const string buildListWebPageLink =
                "https://support.microsoft.com/en-us/help/321185/" +
                "how-to-determine-the-version-edition-and-update-level-of-sql-server-an";
            var builds = new List<SqlServerBuild>();

            string[] buildNumberPage;
            using (var client = new HttpClient())
            using (var res = client.GetAsync(buildListWebPageLink).GetAwaiter().GetResult())
            using (var content = res.Content)
            {
                var data = content.ReadAsStringAsync().GetAwaiter().GetResult();
                buildNumberPage = data.Split(Environment.NewLine,
                    StringSplitOptions.RemoveEmptyEntries);
            }

            var tablesAndStuff = buildNumberPage.Single(x => x.Contains("Frequently asked questions"));
            // remove the open and close quote on this JSON element.  The result should be HTML
            tablesAndStuff = tablesAndStuff
                .TrimStart()
                .Trim('"');
            var doc = new HtmlDocument();
            doc.LoadHtml(tablesAndStuff);
            var tables = doc.DocumentNode.Descendants("table").ToList();

            // Skip the first table, it is the mapping between version patterns 
            // and product names
            foreach (var table in tables.Skip(1).ToList())
            {
                var rows = table.Descendants("tr").Where(x => x.FirstChild.Name != "th").ToList();
                var majorVersionName = table.ParentNode.PreviousSibling.PreviousSibling.InnerText;

                var unsupportVersions = new[]
                {
                    "SQL Server 2005",
                    "SQL Server 2000",
                    "SQL Server 7.0",
                    "SQL Server 6.5"
                };
                if (unsupportVersions.Contains(majorVersionName)) continue;

                foreach (var row in rows)
                {
                    var cells = row.Descendants("td").ToList();
                    if (cells.Count < 5) continue;
                    if (cells[4].InnerText == "&nbsp;" || cells[4].InnerText == string.Empty) continue;

                    var build = new SqlServerBuild(cells, majorVersionName);

                    var supportSearchString = build.Branch.Contains("SP")
                        ? $"{majorVersionName} {build.ServicePack.Replace("SP", "Service Pack ")}"
                        : majorVersionName;
                    var supportDate = supportDates.FirstOrDefault(p => p.ProductName.Contains(supportSearchString));

                    var mainstreamSupportEndDate = supportDate?.MainstreamSupportEndDate;
                    var extendedSupportEndDate = supportDate?.ExtendedSupportEndDate;
                    var servicePackSupportEndDate = supportDate?.ServicePackSupportEndDate;

                    build.SetSupportDates(mainstreamSupportEndDate, extendedSupportEndDate, 
                        servicePackSupportEndDate);

                    builds.Add(build);
                }
            }

            // For some reason, the RTM builds of SQL Server 2016 and 2017 are 
            // not listed on the support website.  So I am adding them manually 
            // here for now.

            // 2016 is 13.00.1601.5
            var build2016Rtm = new SqlServerBuild("13.00.1601.5", "None", 
                string.Empty, string.Empty, new DateTime(2016, 6, 1),  "SQL Server 2016");
            build2016Rtm.SetSupportDates(null, null, new DateTime(2019, 1, 9));
            builds.Add(build2016Rtm);

            // 2017 is 14.0.1000.169
            var build2017Rtm = new SqlServerBuild("14.0.1000.169", "None",
                string.Empty, string.Empty, new DateTime(2017, 10, 2), "SQL Server 2017");
            build2017Rtm.SetSupportDates(new DateTime(2022, 10, 11), new DateTime(2027, 10, 12), null);
            builds.Add(build2017Rtm);

            return builds.OrderBy(b => b.MajorVersionName)
                .ThenBy(b => b.MinorVersionNumber)
                .Reverse()
                .ToList();
        }

        private static string BuildSqlScript(List<SqlServerBuild> builds)
        {
            var sqlScript = new StringBuilder();
            sqlScript.AppendLine(@"
IF (OBJECT_ID('dbo.SqlServerVersions') IS NULL)
BEGIN

    CREATE TABLE dbo.SqlServerVersions
    (
        MajorVersionNumber tinyint not null,
        MinorVersionNumber smallint not null,
        Branch varchar(34) not null,
        [Url] varchar(99) not null,
        ReleaseDate date not null,
        MainstreamSupportEndDate date not null,
        ExtendedSupportEndDate date not null,
        MajorVersionName varchar(19) not null,
        MinorVersionName varchar(67) not null,

        CONSTRAINT PK_SqlServerVersions PRIMARY KEY CLUSTERED
        (
            MajorVersionNumber ASC,
            MinorVersionNumber ASC,
            ReleaseDate ASC
        )
    );

END;
GO

DELETE dbo.SqlServerVersions;"
            );

            sqlScript.Append(@"
INSERT INTO dbo.SqlServerVersions
    (MajorVersionNumber, MinorVersionNumber, Branch, [Url], ReleaseDate, MainstreamSupportEndDate, ExtendedSupportEndDate, MajorVersionName, MinorVersionName)
VALUES");
            foreach (var build in builds)
            {
                sqlScript.Append($@"
    ({build.MajorVersionNumber}, {build.MinorVersionNumber}, '{build.Branch}', '{build.Url}', {build.ReleaseDateSqlString}, {build.MainstreamSupportEndDateSqlString}, {build.ExtendedSupportEndDateSqlString}, '{build.MajorVersionName}', '{build.MinorVersionName}'),");
            }
            // Remove the trailing comma
            sqlScript.Remove(sqlScript.Length - 1, 1);
            sqlScript.Append(@"
;
GO");

            return sqlScript.ToString();
        }
    }
}
