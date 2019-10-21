using System;
using System.IO;

namespace MigrationHelper.DataObjects
{
    public class NotIncludedFile
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the file path of the file
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Converts a <see cref="FileInfo"/> object into a <see cref="NotIncludedFile"/> object
        /// </summary>
        /// <param name="file">The file info object</param>
        public static explicit operator NotIncludedFile(FileInfo file)
        {
            return new NotIncludedFile
            {
                Name = file.Name,
                FilePath = file.FullName,
                CreationDate = file.CreationTime
            };
        }

        /// <summary>
        /// Creates a copy of the given entry
        /// </summary>
        /// <param name="entry">The entry</param>
        /// <returns>The copy of the entry</returns>
        public static NotIncludedFile CopyEntry(NotIncludedFile entry)
        {
            return new NotIncludedFile
            {
                Name = entry.Name,
                FilePath = entry.FilePath,
                CreationDate = entry.CreationDate
            };
        }
    }
}
