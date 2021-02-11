using System;
using System.ComponentModel;
using System.Globalization;

namespace LibEveryFileExplorer.ComponentModel
{
    public class PrettyArrayConverter : ArrayConverter
	{
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
		{
			return "";
		}
	}
}
