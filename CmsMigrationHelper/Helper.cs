using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using CmsMigrationHelper.DataObjects;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Build.Evaluation;
using NuGet;
using ZimLabs.Utility;

namespace CmsMigrationHelper
{
    public static class Helper
    {
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
        /// <returns>A bool which indicates if the action was successful and the created file name</returns>
        /// <exception cref="ArgumentNullException">Will be thrown if the filename is null or empty</exception>
        /// <exception cref="ArgumentException">Will be thrown if the project file wasn't set</exception>
        /// <exception cref="FileNotFoundException">Will be thrown if the project file doesn't exit</exception>
        /// <exception cref="DirectoryNotFoundException">Will be thrown when the directory of the project file cannot be determined</exception>
        public static (bool successful, string filename) CreateMigrationFile(string filename, string content)
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

            var scriptName = $"{DateTime.Now:yyMMddHHmmss}_{filename}.sql";
            var resourcePath = string.IsNullOrEmpty(Properties.Settings.Default.ScriptDirectory)
                ? scriptName
                : Path.Combine(Properties.Settings.Default.ScriptDirectory, scriptName);

            var destinationPath = Path.Combine(scriptDir, scriptName);

            // Step 2: "Create" the file with the given content
            File.WriteAllText(destinationPath, content);

            var project = new Project(Properties.Settings.Default.ProjectFile);
            project.AddItem("EmbeddedResource", resourcePath);
            project.Save();

            return (true, scriptName);
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
        /// Gets the version infos of the program
        /// </summary>
        /// <returns>The version infos</returns>
        public static (string internalName, string originalName, string fileVersion, string productVersion) GetVersion()
        {
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            return (version.InternalName, version.OriginalFilename, version.FileVersion, version.ProductVersion);
        }
    }
}
