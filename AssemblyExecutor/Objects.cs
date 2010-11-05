using System;

namespace SSISExecuteAssemblyTask100.SSIS
{
    public class ComboBoxObjectComboItem
    {
        public object ValueMemeber { get; private set; }

        public object DisplayMember { get; private set; }

        public ComboBoxObjectComboItem(object aBindingValue, object aDisplayValue)
        {
            ValueMemeber = aBindingValue;
            DisplayMember = aDisplayValue;
        }

        public override String ToString()
        {
            return Convert.ToString(DisplayMember);
        }
    }
}
