# MigrationHelper

**Content**
<!-- TOC -->

- [MigrationHelper](#migrationhelper)
    - [General](#general)
    - [Usage](#usage)
        - [Preparations](#preparations)
        - [New migration script](#new-migration-script)
        - [Error analysis](#error-analysis)
    - [Packages](#packages)
        - [NuGet-Packages](#nuget-packages)
        - [SQL Parser](#sql-parser)

<!-- /TOC -->

## General
When you use [dbup](https://dbup.github.io) you can use this small tool to add a new migration script to you migration project without opening visual studio.

## Usage
### Preparations
Open the tool and add the path of your migration project and the name of the script directory (if exists, otherwise leave it empty).

![001](Images/001.png)

- Project file: The path of your migration project
- Script dir: The name of your script direcotry (leave it blank if you don't have a special folder for the scripts)

### New migration script
To create a new migration script, add the name of the script (*Filename:*) and the content of the script (you can open an existing file via *Open existing file*).

![002](Images/002.png)

When you've entered a file name and the sql query you've the following options:
- Check SQL: Checks the script for errors
- Create without check: Creates a new migration script without checking the sql query
- Create with check: Creates a new migration script with a check. If the check fails the process will be stopped.

### Error analysis
If you have performed the check and have and error you will get further information (under the editor) about the error:

![003](Images/003.png)

## Packages
The following packages were used for this application:

### NuGet-Packages
| Package | Version | Target Framework |
|---|---|---|
| AvalonEdit | 5.0.4 | .NET 4.7.2 |
| ControlzEx | 3.0.2.4 | .NET 4.7.2 |
| MahApps.Metro | 1.6.5 | .NET 4.7.2 |
| MahApps.Metro.IconPacks | 2.3.0 | .NET 4.7.2 |
| Microsoft.Build | 16.0.461 | .NET 4.7.2 |
| Microsoft.Build.Framework | 16.0.461 | .NET 4.7.2 |
| Microsoft.Build.Utilities.Core | 16.0.461 | .NET 4.7.2 |
| Microsoft.VisualStudio.Setup.Configuration.Interop | 1.16.30 | .NET 4.7.2 |
| Microsoft.Web.Xdt | 3.0.0 | .NET 4.7.2 |
| Microsoft.WindowsAPICodePack-Core | 1.1.0.2 | .NET 4.7.2 |
| Microsoft.WindowsAPICodePack-Shell | 1.1.0.0 | .NET 4.7.2 |
| NuGet.Core | 2.14.0 | .NET 4.7.2 |
| System.Collections.Immutable | 1.5.0 | .NET 4.7.2 |
| System.Threading.Tasks.Dataflow | 4.9.0 | .NET 4.7.2 |
| ZimLabs.Utility | 0.0.4 | .NET 4.7.2 |
| ZimLabs.WpfBase | 0.0.4 | .NET 4.7.2 |

### SQL Parser
The SQL Parsers uses the `Microsoft.SqlServer.Management.SqlParser.dll` which is a part of the MS SQL Server.