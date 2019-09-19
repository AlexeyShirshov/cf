
using System;
using System.Collections.Generic;

namespace CoreFramework
{
	public static partial class ConvertExtensions
    {
		public static bool TryConvertToInt(this object value, out int result, int defaultValue = default(int), System.Globalization.CultureInfo cultureInfo = null)
        {
            if (value == null)
			{
				result = defaultValue;
				return false;
			}
			else if (value is int) 
			{
				result = (int)value;
				return true;
			}

			if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			return int.TryParse(value.ToString(), System.Globalization.NumberStyles.Any, cultureInfo, out result);
        }
		public static bool TryConvertToBool(this object value, out bool result, bool defaultValue = default(bool))
        {
            if (value == null)
			{
				result = defaultValue;
				return false;
			}
			else if (value is bool) 
			{
				result = (bool)value;
				return true;
			}

			return bool.TryParse(value.ToString(), out result);
        }
		public static bool TryConvertToDouble(this object value, out double result, double defaultValue = default(double), System.Globalization.CultureInfo cultureInfo = null)
        {
            if (value == null)
			{
				result = defaultValue;
				return false;
			}
			else if (value is double) 
			{
				result = (double)value;
				return true;
			}

			if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			return double.TryParse(value.ToString(), System.Globalization.NumberStyles.Any, cultureInfo, out result);
        }
		public static bool TryConvertToFloat(this object value, out float result, float defaultValue = default(float), System.Globalization.CultureInfo cultureInfo = null)
        {
            if (value == null)
			{
				result = defaultValue;
				return false;
			}
			else if (value is float) 
			{
				result = (float)value;
				return true;
			}

			if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			return float.TryParse(value.ToString(), System.Globalization.NumberStyles.Any, cultureInfo, out result);
        }
		public static bool TryConvertToShort(this object value, out short result, short defaultValue = default(short), System.Globalization.CultureInfo cultureInfo = null)
        {
            if (value == null)
			{
				result = defaultValue;
				return false;
			}
			else if (value is short) 
			{
				result = (short)value;
				return true;
			}

			if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			return short.TryParse(value.ToString(), System.Globalization.NumberStyles.Any, cultureInfo, out result);
        }
		public static bool TryConvertToDecimal(this object value, out decimal result, decimal defaultValue = default(decimal), System.Globalization.CultureInfo cultureInfo = null)
        {
            if (value == null)
			{
				result = defaultValue;
				return false;
			}
			else if (value is decimal) 
			{
				result = (decimal)value;
				return true;
			}

			if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			return decimal.TryParse(value.ToString(), System.Globalization.NumberStyles.Any, cultureInfo, out result);
        }
	}
}
