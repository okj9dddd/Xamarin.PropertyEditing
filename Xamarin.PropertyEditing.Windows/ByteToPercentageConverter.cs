using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (byte), typeof (double))]
	internal class ByteToPercentageConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
			=> !(value is byte byteValue) ? DependencyProperty.UnsetValue
				: byteValue / 2.55d;

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			=> !(value is double doubleValue) ? DependencyProperty.UnsetValue
				: doubleValue < 0 ? 0
				: doubleValue > 100 ? 255
				: System.Convert.ToByte (doubleValue * 2.55d);

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
