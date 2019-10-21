using System.ComponentModel;

namespace MigrationHelper.DataObjects
{
    public static class CustomEnums
    {
        public enum SubFolderFormat
        {
            /// <summary>
            /// Adds no date to the sub folder name
            /// </summary>
            [Description("Adds nothing to the sub folder name.")]
            None,
            /// <summary>
            /// Adds the year to the sub folder name
            /// </summary>
            [Description("Adds the year to the sub folder name (Format: yyyy)")]
            Year,
            /// <summary>
            /// Adds the year the and month to the sub folder name
            /// </summary>
            [Description("Adds the year and the month to the sub folder name (Format: yyyyMM)")]
            YearAndMonth
        }
    }
}
