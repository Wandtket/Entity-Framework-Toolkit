using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Converters
{
    public class EnumToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum enumValue)
            {
                var displayAttribute = enumValue.GetType()
                    .GetMember(enumValue.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DisplayAttribute>();

                return displayAttribute?.GetName() ?? $"Unknown value: {value}";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}
