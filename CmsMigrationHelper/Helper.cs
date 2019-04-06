using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
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
    }
}
