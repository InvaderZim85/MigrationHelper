using System.IO;
using Microsoft.Build.Evaluation;

namespace MigrationHelper.DataObjects
{
    public class FileItem
    {
        /// <summary>
        /// Gets or sets the file
        /// </summary>
        public FileInfo File { get; set; }

        /// <summary>
        /// Gets the file name
        /// </summary>
        public string Name => File?.Name ?? "";

        /// <summary>
        /// Gets the file size
        /// </summary>
        public string Size => (File?.Length ?? 0).ToFormattedFileSize();

        /// <summary>
        /// Gets or sets the project item
        /// </summary>
        public ProjectItem Item { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="FileItem"/>
        /// </summary>
        /// <param name="file">The file info object</param>
        /// <param name="item">The project item</param>
        public FileItem(FileInfo file, ProjectItem item)
        {
            File = file;
            Item = item;
        }
    }
}
