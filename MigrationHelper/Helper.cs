using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Build.Evaluation;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using MigrationHelper.DataObjects;
using NuGet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using ZimLabs.Utility;
using ZimLabs.Utility.Extensions;

namespace MigrationHelper
{
    public static class Helper
    {
        /// <summary>
        /// Contains the project
        /// </summary>
        private static Project _project;

        /// <summary>
        /// Loads the highlight definition for the avalon editor
        /// </summary>
        /// <returns>The definition</returns>
        public static IHighlightingDefinition LoadSqlSchema()
        {
            var file = Path.Combine(Global.GetBaseFolder(), "AvalonSqlSchema.xml");

            using (var reader = File.Open(file, FileMode.Open))
            {
                using (var xmlReader = new XmlTextReader(reader))
                {
                    return HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
                }
            }
        }

        /// <summary>
        /// Creates a new migration file
        /// </summary>
        /// <param name="filename">The name of the desired file</param>
        /// <param name="content">The content of the file</param>
        /// <param name="existingFile">true when the user has open an existing file</param>
        /// <returns>A bool which indicates if the action was successful and the created file name</returns>
        /// <exception cref="ArgumentNullException">Will be thrown if the filename is null or empty</exception>
        /// <exception cref="ArgumentException">Will be thrown if the project file wasn't set</exception>
        /// <exception cref="FileNotFoundException">Will be thrown if the project file doesn't exit</exception>
        /// <exception cref="DirectoryNotFoundException">Will be thrown when the directory of the project file cannot be determined</exception>
        public static (bool successful, FileInfo file) CreateMigrationFile(string filename, string content, bool existingFile)
        {
            // Step 0: Check the given parameters and the project file
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException(nameof(filename));

            if (string.IsNullOrEmpty(Properties.Settings.Default.ProjectFile))
                throw new ArgumentException("The path of the project file is missing.");

            if (!File.Exists(Properties.Settings.Default.ProjectFile))
                throw new FileNotFoundException("The given project file doesn't exist.",
                    Properties.Settings.Default.ProjectFile);

            // Step 1: Prepare the variables
            var projectDir = Path.GetDirectoryName(Properties.Settings.Default.ProjectFile);
            if (string.IsNullOrEmpty(projectDir))
                throw new DirectoryNotFoundException("The directory of the project file cannot be found.");

            var scriptDir = string.IsNullOrEmpty(Properties.Settings.Default.ScriptDirectory)
                ? projectDir
                : Path.Combine(projectDir, Properties.Settings.Default.ScriptDirectory);

            var scriptName = CreateFilename(filename);
            var resourcePath = string.IsNullOrEmpty(Properties.Settings.Default.ScriptDirectory)
                ? scriptName
                : Path.Combine(Properties.Settings.Default.ScriptDirectory, scriptName);

            var destinationPath = Path.Combine(scriptDir, scriptName);

            // Step 2: "Create" the file with the given content
            File.WriteAllText(destinationPath, content);

            // Add the file to the project when it doesn't exist
            if (!existingFile)
            {
                LoadProject();
                _project.AddItem("EmbeddedResource", resourcePath);
                _project.Save();
            }

            return (true, new FileInfo(destinationPath));
        }

        /// <summary>
        /// Creates a new file name with the date / time pattern
        /// </summary>
        /// <param name="filename">The desired file name</param>
        /// <returns>The created file name</returns>
        private static string CreateFilename(string filename)
        {
            var checkPattern = new Regex(@"^\d{12,}");
            return checkPattern.IsMatch(filename) ? filename : $"{DateTime.Now:yyMMddHHmmss}_{filename}.sql";
        }

        /// <summary>
        /// Gets the package information of the project
        /// </summary>
        /// <returns>The list with the reference data</returns>
        public static List<ReferenceEntry> GetPackageInformation()
        {
            var packageFile = Path.Combine(Global.GetBaseFolder(), "packages.config");
            if (!File.Exists(packageFile))
                return new List<ReferenceEntry>();

            var file = new PackageReferenceFile(packageFile);
            var references = file.GetPackageReferences();

            var result = new List<ReferenceEntry>();
            foreach (var package in references)
            {
                result.Add(new ReferenceEntry
                {
                    Name = package.Id,
                    IsDevelopmentDependency = package.IsDevelopmentDependency,
                    TargetFramework = package.TargetFramework.Version.ToString(),
                    Version = package.Version.Version.ToString()
                });
            }

            return result;
        }

        /// <summary>
        /// Loads the script files
        /// </summary>
        /// <returns></returns>
        public static List<FileInfo> LoadScriptFiles()
        {
            var projectFiles = LoadProjectFiles();

            var projectDir = Path.GetDirectoryName(Properties.Settings.Default.ProjectFile);
            if (string.IsNullOrEmpty(projectDir) || !Directory.Exists(projectDir))
                throw new DirectoryNotFoundException("The given directory doesn't exist.");

            var fileDir = string.IsNullOrEmpty(Properties.Settings.Default.ScriptDirectory)
                ? projectDir
                : Path.Combine(projectDir, Properties.Settings.Default.ScriptDirectory);

            var dirInfo = new DirectoryInfo(fileDir);
            var files = dirInfo.GetFiles("*.sql", SearchOption.AllDirectories);

            return files.Where(w => projectFiles.Any(a => a.EqualsIgnoreCase(w.Name))).ToList();
        }

        /// <summary>
        /// Loads the files of the project
        /// </summary>
        /// <returns></returns>
        private static List<string> LoadProjectFiles()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.ProjectFile))
                throw new ArgumentException("The path of the project file is missing.");

            LoadProject();

            var items = _project.Items.Where(w => w.ItemType.Equals("EmbeddedResource"))
                .Select(s => s.EvaluatedInclude).ToList();

            return items.Select(Path.GetFileName).ToList();
        }

        /// <summary>
        /// Gets the version infos of the program
        /// </summary>
        /// <returns>The version infos</returns>
        public static (string internalName, string originalName, string fileVersion, string productVersion) GetVersion()
        {
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            return (version.InternalName, version.OriginalFilename, version.FileVersion, version.ProductVersion);
        }

        /// <summary>
        /// Checks if the given sql is valid
        /// </summary>
        /// <param name="content">The sql script</param>
        /// <returns>The result</returns>
        public static async Task<(bool valid, List<ErrorEntry> errors)> IsSqlValid(string content)
        {
            if (string.IsNullOrEmpty(content))
                return (false, null);

            var result = await Task.Run(() => Parser.Parse(content));

            var errorList = result.Errors.ToList().Select(s => (ErrorEntry)s).ToList();
            return (errorList.Count == 0, errorList);
        }

        /// <summary>
        /// Loads the project if necessary
        /// </summary>
        private static void LoadProject()
        {
            if (_project == null)
                _project = new Project(Properties.Settings.Default.ProjectFile);
        }
    }
}
