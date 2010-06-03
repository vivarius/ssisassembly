using System;

namespace SSISAssemblyExecutor
{
    public class ComboItem
    {
        public object BindingValue { get; private set; }

        public object DisplayValue { get; private set; }

        public ComboItem(object aBindingValue, object aDisplayValue)
        {
            BindingValue = aBindingValue;
            DisplayValue = aDisplayValue;
        }

        public override String ToString()
        {
            return Convert.ToString(DisplayValue);
        }
    }
}
