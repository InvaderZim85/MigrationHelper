using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Build.Evaluation;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using MigrationHelper.DataObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
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
        public static IHighlightingDefinition LoadSqlSchema(bool dark)
        {
            var fileName = dark ? "AvalonSqlSchema_Dark.xml" : "AvalonSqlSchema.xml";
            var file = Path.Combine(Global.GetBaseFolder(), fileName);

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

            var scriptDir = GetScriptDirectory(projectDir, true);

            var scriptName = CreateFilename(filename);

            var resourceDir = GetScriptDirectory(projectDir, false);
            var resourcePath = string.IsNullOrEmpty(resourceDir)
                ? scriptName
                : Path.Combine(resourceDir, scriptName);

            var destinationPath = Path.Combine(scriptDir, scriptName);

            // Step 2: "Create" the file with the given content
            File.WriteAllText(destinationPath, content);

            // Add the file to the project when it doesn't exist
            if (existingFile)
                return (true, scriptName);

            LoadProject();

            var itemList = _project.AddItem("EmbeddedResource", resourcePath);
            // Check if the file was created in the project
            if (!itemList.Any(a => a.EvaluatedInclude.EqualsIgnoreCase(resourcePath)))
            {
                // Remove the file from the system because it wasn't created correctly
                File.Delete(destinationPath);
                return (false, "");
            }

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
        /// Adds an existing file to the project
        /// </summary>
        /// <param name="files">The list with the files</param>
        /// <returns>true when successful, otherwise false</returns>
        public static bool IncludeProjectFiles(List<NotIncludedFile> files)
        {
            LoadProject();

            // Remove the project dir path to get only the resource path
            var projectDir = Path.GetDirectoryName(Properties.Settings.Default.ProjectFile);
            if (string.IsNullOrEmpty(projectDir))
                throw new DirectoryNotFoundException("The directory of the project file cannot be found.");

            foreach (var file in files)
            {
                var path = file.FilePath.Replace($"{projectDir}\\", "");

                var itemList = _project.AddItem("EmbeddedResource", path);

                // Check if the file was added
                if (!itemList.Any(a => a.EvaluatedInclude.EqualsIgnoreCase(path)))
                {
                    return false;
                }
            }

            _project.Save();

            return true;
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

            var doc = XDocument.Load(packageFile);

            // Get the values
            var result = (from entry in doc.Descendants("package")
                let id = entry.Attribute("id")?.Value
                let version = entry.Attribute("version")?.Value
                let targetFramework = entry.Attribute("targetFramework")?.Value
                let developmentDependency = entry.Attribute("developmentDependency")?.Value
                select new ReferenceEntry
                {
                    Name = id,
                    Version = version,
                    TargetFramework = targetFramework,
                    IsDevelopmentDependency = developmentDependency.ToBool()
                }).ToList();

            return result;
        }

        /// <summary>
        /// Loads the script files
        /// </summary>
        /// <returns>The list with the files</returns>
        public static (TreeViewNode rootNode, List<FileInfo> notIncludedFiles) LoadScriptFiles()
        {
            var projectFiles = LoadProjectFiles();

            var projectDir = Path.GetDirectoryName(Properties.Settings.Default.ProjectFile);
            if (string.IsNullOrEmpty(projectDir) || !Directory.Exists(projectDir))
                throw new DirectoryNotFoundException("The given directory doesn't exist.");

            var fileDir = string.IsNullOrEmpty(Properties.Settings.Default.ScriptDirectory)
                ? projectDir
                : Path.Combine(projectDir, Properties.Settings.Default.ScriptDirectory);

            return LoadScriptFilesWithSubDir(fileDir, projectFiles);
        }

        /// <summary>
        /// Loads the script files 
        /// </summary>
        /// <param name="root">The path of the script root</param>
        /// <param name="projectFiles">The list with the project files</param>
        /// <returns>The list of the sub directories</returns>
        private static (TreeViewNode rootNode, List<FileInfo> notIncludedFiles) LoadScriptFilesWithSubDir(string root, List<(ProjectItem projectItem, string filename)> projectFiles)
        {
            if (string.IsNullOrEmpty(root))
                throw new ArgumentNullException(nameof(root));

            if (projectFiles == null)
                throw new ArgumentNullException(nameof(projectFiles));

            var dirInfo = new DirectoryInfo(root);
            if (!dirInfo.Exists)
                throw new DirectoryNotFoundException("The given directory doesn't exist.");

            // The root element
            var rootDir = new TreeViewNode(dirInfo) {IsExpanded = true};

            // Get the directories
            var subDirs = dirInfo.GetDirectories();
            var fileList = new List<FileInfo>();
            foreach (var subDirectory in subDirs)
            {
                var subDir = new TreeViewNode(subDirectory);
                rootDir.SubNodes.Add(subDir);
                
                // Add the files
                fileList.AddRange(AddScriptFiles(subDir, projectFiles));
            }

            // Order the entries
            rootDir.SubNodes = rootDir.SubNodes.OrderBy(o => o.IsDirectory).ToList();

            // Add the files
            fileList.AddRange(AddScriptFiles(rootDir, projectFiles));

            return (rootDir, GetNotIncludedFiles(projectFiles, fileList));
        }

        /// <summary>
        /// Gets the files which are not included in the project but exists in the project folder
        /// </summary>
        /// <param name="projectItems">The list with the project items</param>
        /// <param name="fileList">The list with the files in the scripts directory</param>
        /// <returns>The list with the not included files</returns>
        private static List<FileInfo> GetNotIncludedFiles(List<(ProjectItem projectItem, string filename)> projectItems,
            List<FileInfo> fileList)
        {
            return fileList.Where(file => !projectItems.Any(a => a.filename.EqualsIgnoreCase(file.Name))).ToList();
        }

        /// <summary>
        /// Loads the files for the given directory
        /// </summary>
        /// <param name="directory">The directory</param>
        /// <param name="projectFiles">The list with the project files</param>
        private static List<FileInfo> AddScriptFiles(TreeViewNode directory, List<(ProjectItem projectItem, string filename)> projectFiles)
        {
            var dirInfo = new DirectoryInfo(directory.FullPath);
            var files = dirInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                var entry = projectFiles.FirstOrDefault(f => f.filename.EqualsIgnoreCase(file.Name)).projectItem;
                if (entry == null)
                    continue;

                directory.SubNodes.Add(new TreeViewNode(file, entry));
            }

            return files.ToList();
        }

        /// <summary>
        /// Loads the files of the project
        /// </summary>
        /// <returns>Loads the files which are in the project</returns>
        private static List<(ProjectItem projectItem, string filename)> LoadProjectFiles()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.ProjectFile))
                throw new ArgumentException("The path of the project file is missing.");

            var excludeDirs = Properties.Settings.Default.ExcludeDirectories
                .Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries).ToList();

            LoadProject();

            var tmpList = _project.Items.Where(w => w.ItemType.Equals("EmbeddedResource"))
                .Select(s => new {Item = s, FileName = Path.GetFileName(s.EvaluatedInclude), DirName = GetDirName(s.EvaluatedInclude)}).ToList();

            var result = new List<(ProjectItem projectItem, string filename)>();
            foreach (var entry in tmpList)
            {
                if (excludeDirs.Any(a => a.ContainsIgnoreCase(entry.DirName)))
                    continue;

                result.Add((entry.Item, entry.FileName));
            }

            return result;
        }

        /// <summary>
        /// Gets the name of the directory
        /// </summary>
        /// <param name="fullPath">The full path</param>
        /// <returns>The name of the directory</returns>
        private static string GetDirName(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return "";

            if (fullPath.Contains("\\"))
            {
                var index = fullPath.IndexOf("\\", StringComparison.InvariantCultureIgnoreCase);

                return fullPath.Substring(0, index);
            }

            return "";
        }

        /// <summary>
        /// Deletes the given project item
        /// </summary>
        /// <param name="file">The selected file</param>
        public static bool DeleteProjectItem(TreeViewNode file)
        {
            if (file?.ProjectItem == null)
                return false;

            LoadProject();

            // Remove the file from the project
            if (_project.RemoveItem(file.ProjectItem))
            {
                _project.Save();

                // Delete the file on the system
                File.Delete(file.FullPath);

                return true;
            }

            return false;
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

        /// <summary>
        /// Returns the name of the script directory
        /// </summary>
        /// <param name="projectDir">The directory of the project</param>
        /// <param name="complete">true to get the complete path, otherwise false</param>
        /// <returns>The path of the script directory</returns>
        private static string GetScriptDirectory(string projectDir, bool complete)
        {
            string scriptDir;
            if (string.IsNullOrEmpty(Properties.Settings.Default.ScriptDirectory))
            {
                scriptDir = complete ? projectDir : "";
            }
            else
            {
                scriptDir = complete
                    ? Path.Combine(projectDir, Properties.Settings.Default.ScriptDirectory)
                    : Properties.Settings.Default.ScriptDirectory;
            }

            var result = "";
            if (Properties.Settings.Default.UseSubFolder)
            {
                var format = (CustomEnums.SubFolderFormat) Properties.Settings.Default.SubScriptDirectoryFormat;

                var subScriptDir = Properties.Settings.Default.SubScriptDirectory;
                switch (format)
                {
                    case CustomEnums.SubFolderFormat.None:
                        result =
                            $"{scriptDir}\\{subScriptDir}";
                        break;
                    case CustomEnums.SubFolderFormat.Year:
                        result = $"{scriptDir}\\{subScriptDir}{DateTime.Now:yyyy}";
                        break;
                    case CustomEnums.SubFolderFormat.YearAndMonth:
                        result = $"{scriptDir}\\{subScriptDir}{DateTime.Now:yyyyMM}";
                        break;
                }
            }
            else
            {
                result = Path.Combine(projectDir, scriptDir);
            }

            if (complete && !Directory.Exists(result))
                Directory.CreateDirectory(result);

            return result;
        }

        /// <summary>
        /// Deletes the given files
        /// </summary>
        /// <param name="files">The list with the files</param>
        /// <returns>true when everything was successful, otherwise false</returns>
        public static bool DeleteFiles(List<NotIncludedFile> files)
        {
            try
            {
                foreach (var file in files)
                {
                    File.Delete(file.FilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(nameof(DeleteFiles), ex);
                return false;
            }
        }
    }
}