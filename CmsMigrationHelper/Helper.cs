using System;
using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Build.Evaluation;
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
    }
}
