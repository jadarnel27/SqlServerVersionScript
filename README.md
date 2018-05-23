# SqlServerVersionScript
Creates a T-SQL script to build a table with all supported SQL Server versions and their support dates

# Usage
Download the latest release and run the exe or run `dotnet GetSqlServerVersionInfo.dll` depending on your system (see the Release tab for info).

This is a Windows console app.  To build and run the source code yourself, open the solution in Visual Studio, and press F5 to run the app under the debugger.

A Command Prompt window will be displayed with the T-SQL script.  You can then copy the contents of the script out of the console window and run it or save it.

# Example script
See the \src\SqlServerVersions-2018-05-22.sql in this repository for an example of the output of the application.

# How it works
The applications reads information from Microsoft's support site.

Mainstream and extended support dates come from this API endpoint, which is accessed by clicking the "Export" link on [this page](https://support.microsoft.com/en-us/lifecycle/search?alpha=SQL%20Server)

[Search product lifecycle ("SQL Server")](https://support.microsoft.com/api/lifecycle/GetProductsLifecycle?query=%7B%22names%22:%5B%22SQL%2520Server%22%5D,%22years%22:%220%22,%22gdsId%22:0,%22export%22:true%7D)

All other information (build number, branch, KB article, release date, etc) come from the tables at the bottom of this web page:

[How to determine the version, edition, and update level of SQL Server and its components](https://support.microsoft.com/en-us/help/321185/how-to-determine-the-version-edition-and-update-level-of-sql-server-an)