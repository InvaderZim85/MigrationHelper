using Microsoft.SqlServer.Management.SqlParser.Parser;

namespace CmsMigrationHelper.DataObjects
{
    public class ErrorEntry
    {
        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the column where the error exists
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Gets or sets the row where the error exists
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Converts a error entry into the custom error entry
        /// </summary>
        /// <param name="entry">The original entry</param>
        public static explicit operator ErrorEntry(Error entry)
        {
            if (entry == null)
                return new ErrorEntry();

            return new ErrorEntry
            {
                Message = entry.Message,
                Column = entry.Start.ColumnNumber,
                Line = entry.Start.LineNumber
            };
        }
    }
}
