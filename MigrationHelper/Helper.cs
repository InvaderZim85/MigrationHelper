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
        public static (bool successful, string filename) SaveMigrationFile(string filename, string content, bool existingFile)
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
            if (existingFile)
                return (true, scriptName);

            LoadProject();
            _project.AddItem("EmbeddedResource", resourcePath);
            _project.Save();

            return (true, scriptName);
        }

        /// <summary>
        /// Creates a new file name with the date / time pattern
        /// </summary>
        /// <param name="filename">The desired file name</param>
        /// <returns>The created file name</returns>
        private static string CreateFilename(string filename)
        {
            var checkPattern = new Regex(@"^\d{12,}");

            if (filename.ContainsIgnoreCase(".sql"))
            {
                filename = Regex.Replace(filename, ".sql", "", RegexOptions.IgnoreCase);
            }

            return checkPattern.IsMatch(filename) ? $"{filename}.sql" : $"{DateTime.Now:yyMMddHHmmss}_{filename}.sql";
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
        /// <returns>The list with the files</returns>
        public static List<FileItem> LoadScriptFiles()
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

            var resultList = new List<FileItem>();

            foreach (var file in files)
            {
                var entry = projectFiles.FirstOrDefault(f => f.filename.EqualsIgnoreCase(file.Name)).projectItem;

                if (entry == null)
                    continue;

                resultList.Add(new FileItem(file, entry));
            }

            return resultList;
        }

        /// <summary>
        /// Loads the files of the project
        /// </summary>
        /// <returns>Loads the files which are in the project</returns>
        private static List<(ProjectItem projectItem, string filename)> LoadProjectFiles()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.ProjectFile))
                throw new ArgumentException("The path of the project file is missing.");

            LoadProject();

            return _project.Items.Where(w => w.ItemType.Equals("EmbeddedResource"))
                .Select(s => (s, Path.GetFileName(s.EvaluatedInclude))).ToList();
        }

        /// <summary>
        /// Deletes the given project item
        /// </summary>
        /// <param name="file">The selected file</param>
        public static void DeleteProjectItem(FileItem file)
        {
            if (file?.Item == null)
                return;

            LoadProject();

            // Remove the file from the project
            _project.RemoveItem(file.Item);
            _project.Save();

            // Delete the file on the system
            File.Delete(file.File.FullName);
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
        /// <param name="forceLoad">true to force the creation of a new instance</param>
        private static void LoadProject(bool forceLoad = false)
        {
            if (_project == null || forceLoad)
                _project = new Project(Properties.Settings.Default.ProjectFile);
        }

        /// <summary>
        /// Unloads the project
        /// </summary>
        public static void UnloadProject()
        {
            if (_project != null)
                ProjectCollection.GlobalProjectCollection.UnloadProject(_project);
        }

        /// <summary>
        /// Updates the project
        /// </summary>
        public static void UpdateProject()
        {
            UnloadProject();
            LoadProject(true);
        }

        /// <summary>
        /// Converts the file length into a readable size
        /// </summary>
        /// <param name="length">The file length</param>
        /// <returns>The readable size</returns>
        public static string ToFormattedFileSize(this long length)
        {
            switch (length)
            {
                case var _ when length < 1024:
                    return "1 KB";
                case var _ when length >= 1024 && length < Math.Pow(1024, 2):
                    return $"{length / 1024d:N2} KB";
                case var _ when length >= Math.Pow(1024, 2) && length < Math.Pow(1024, 3):
                    return $"{length / Math.Pow(1024, 2):N2} MB";
                case var _ when length >= Math.Pow(1024, 3):
                    return $"{length / Math.Pow(1024, 2):N2} GB";
            }

            return "";
        }

        /// <summary>
        /// Returns the name of the script directory if its exists
        /// </summary>
        /// <param name="projectFile">The path of the project file</param>
        /// <returns>The name of the scrip directory</returns>
        public static string GetScriptFolder(string projectFile)
        {
            if (string.IsNullOrEmpty(projectFile))
                return "";

            if (!File.Exists(projectFile))
                return "";

            var dir = Path.GetDirectoryName(projectFile);

            if (string.IsNullOrEmpty(dir))
                return "";

            var subDirs = Directory.GetDirectories(dir);

            var scriptDir = subDirs.FirstOrDefault(f => f.ContainsIgnoreCase("scripts"));

            return string.IsNullOrEmpty(scriptDir) ? "" : scriptDir.Split(new[] {"\\"}, StringSplitOptions.None).Last();
        }
    }
}
