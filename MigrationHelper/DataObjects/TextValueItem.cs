using System;
using System.ComponentModel;
using ZimLabs.Utility.Extensions;

namespace MigrationHelper.DataObjects
{
    public class TextValueItem
    {
        /// <summary>
        /// Gets or sets the id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the value of the item
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Creates a new empty instance of the <see cref="TextValueItem"/>
        /// </summary>
        public TextValueItem() { }

        /// <summary>
        /// Creates a new instance of the <see cref="TextValueItem"/>
        /// </summary>
        /// <param name="value">The enum</param>
        public TextValueItem(Enum value)
        {
            Id = Convert.ToInt32(value);
            Text = value.ToString();
            
            if (value.GetType() == typeof(CustomEnums.SubFolderFormat))
            {
                var description = value.GetAttribute<DescriptionAttribute>();
                if (!string.IsNullOrEmpty(description?.Description))
                {
                    Text = $"{value.ToString()} - {description.Description}";
                }
            }

            Value = value;
        }

        /// <summary>
        /// Returns the text of the item
        /// </summary>
        /// <returns>The text</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
