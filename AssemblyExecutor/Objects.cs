using System;

namespace SSISExecuteAssemblyTask100
{
    public class ComboBoxObjectComboItem
    {
        /// <summary>
        /// Gets or sets the value memeber.
        /// </summary>
        /// <value>The value memeber.</value>
        public object ValueMemeber { get; private set; }

        /// <summary>
        /// Gets or sets the display member.
        /// </summary>
        /// <value>The display member.</value>
        public object DisplayMember { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComboBoxObjectComboItem"/> class.
        /// </summary>
        /// <param name="aBindingValue">A binding value.</param>
        /// <param name="aDisplayValue">A display value.</param>
        public ComboBoxObjectComboItem(object aBindingValue, object aDisplayValue)
        {
            ValueMemeber = aBindingValue;
            DisplayMember = aDisplayValue;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return Convert.ToString(DisplayMember);
        }
    }
}
