# SqlServerVersionScript
Creates a T-SQL script to build a table with all supported SQL Server versions and their support dates

# Usage
Download the [latest release](https://github.com/jadarnel27/SqlServerVersionScript/releases).  There are two versions of the application:

 - **SqlServerVersionScript-NetCoreSC-win64.zip**
     - a self-contained .NET Core app (.exe) that runs on 64-bit Windows
     - run this by extracting the zip and running GetSqlServerVersionInfo.exe
 - **SqlServerVersionScript-NetCoreFDD-win64.zip**
     - a (much smaller) Framework-dependent deployment of the .NET Core app (.dll) that run on 64-bit Windows
     - requires .NET Core to be present on the machine
     - this can be run from the command line by extracting the zip and running `dotnet GetSqlServerVersionInfo.dll`
	 
By default, a file named SqlServerVersions.sql will be created in the same directory where you ran the application.  To specify a destination folder, pass the path as a command line argument:

    GetSqlServerVersionInfo.exe "C:\Temp"
	// or
	dotnet GetSqlServerVersionInfo.dll "C:\Temp"

# Example script
See the \src\SqlServerVersions-2018-05-22.sql in this repository for an example of the output of the application.

# How it works
The applications reads information from Microsoft's support site.

Mainstream and extended support dates come from this API endpoint, which is accessed by clicking the "Export" link on [this page](https://support.microsoft.com/en-us/lifecycle/search?alpha=SQL%20Server)

[Search product lifecycle ("SQL Server")](https://support.microsoft.com/api/lifecycle/GetProductsLifecycle?query=%7B%22names%22:%5B%22SQL%2520Server%22%5D,%22years%22:%220%22,%22gdsId%22:0,%22export%22:true%7D)

All other information (build number, branch, KB article, release date, etc) come from the tables at the bottom of this web page:

[How to determine the version, edition, and update level of SQL Server and its components](https://support.microsoft.com/en-us/help/321185/how-to-determine-the-version-edition-and-update-level-of-sql-server-an)