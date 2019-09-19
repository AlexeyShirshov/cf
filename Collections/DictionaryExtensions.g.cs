
using System;
using System.Collections.Generic;

namespace CoreFramework.CFCollections
{
	public static partial class DictionaryExtensions
    {
            
		public static int GetInt(this IDictionary<String, String> dic, string name, int def, bool throwOnParseError = false, System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            int i;
            if (dic.TryGetValue(name, out s))
			{
				if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
				if (throwOnParseError)
					return int.Parse(s);
				else if(int.TryParse(s, System.Globalization.NumberStyles.Any, cultureInfo, out i))
					return i;
			}

            return def;
        }

		public static int? GetInt(this IDictionary<String, String> dic, string name, System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            int i;
			if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            if (dic.TryGetValue(name, out s) && int.TryParse(s, System.Globalization.NumberStyles.Any, cultureInfo, out i))
                return i;

            return null;
        }
		
		public static int GetInt(this IDictionary<String, object> dic, string name, int def = default(int), System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			object s;
            if (dic.TryGetValue(name, out s))
			{
                return (int)Convert.ChangeType(s, typeof(int), cultureInfo);
			}

            return def;
        }
            
		public static bool GetBool(this IDictionary<String, String> dic, string name, bool def, bool throwOnParseError = false)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            bool i;
            if (dic.TryGetValue(name, out s))
			{
				if (throwOnParseError)
					return bool.Parse(s);
				else if(bool.TryParse(s, out i))
					return i;
			}

            return def;
        }

		public static bool? GetBool(this IDictionary<String, String> dic, string name)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            bool i;
            if (dic.TryGetValue(name, out s) && bool.TryParse(s, out i))
                return i;

            return null;
        }
		
		public static bool GetBool(this IDictionary<String, object> dic, string name, bool def = default(bool), System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			object s;
            if (dic.TryGetValue(name, out s))
			{
                return (bool)Convert.ChangeType(s, typeof(bool), cultureInfo);
			}

            return def;
        }
            
		public static double GetDouble(this IDictionary<String, String> dic, string name, double def, bool throwOnParseError = false, System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            double i;
            if (dic.TryGetValue(name, out s))
			{
				if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
				if (throwOnParseError)
					return double.Parse(s);
				else if(double.TryParse(s, System.Globalization.NumberStyles.Any, cultureInfo, out i))
					return i;
			}

            return def;
        }

		public static double? GetDouble(this IDictionary<String, String> dic, string name, System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            double i;
			if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            if (dic.TryGetValue(name, out s) && double.TryParse(s, System.Globalization.NumberStyles.Any, cultureInfo, out i))
                return i;

            return null;
        }
		
		public static double GetDouble(this IDictionary<String, object> dic, string name, double def = default(double), System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			object s;
            if (dic.TryGetValue(name, out s))
			{
                return (double)Convert.ChangeType(s, typeof(double), cultureInfo);
			}

            return def;
        }
            
		public static float GetFloat(this IDictionary<String, String> dic, string name, float def, bool throwOnParseError = false, System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            float i;
            if (dic.TryGetValue(name, out s))
			{
				if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
				if (throwOnParseError)
					return float.Parse(s);
				else if(float.TryParse(s, System.Globalization.NumberStyles.Any, cultureInfo, out i))
					return i;
			}

            return def;
        }

		public static float? GetFloat(this IDictionary<String, String> dic, string name, System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            float i;
			if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            if (dic.TryGetValue(name, out s) && float.TryParse(s, System.Globalization.NumberStyles.Any, cultureInfo, out i))
                return i;

            return null;
        }
		
		public static float GetFloat(this IDictionary<String, object> dic, string name, float def = default(float), System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			object s;
            if (dic.TryGetValue(name, out s))
			{
                return (float)Convert.ChangeType(s, typeof(float), cultureInfo);
			}

            return def;
        }
            
		public static short GetShort(this IDictionary<String, String> dic, string name, short def, bool throwOnParseError = false, System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            short i;
            if (dic.TryGetValue(name, out s))
			{
				if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
				if (throwOnParseError)
					return short.Parse(s);
				else if(short.TryParse(s, System.Globalization.NumberStyles.Any, cultureInfo, out i))
					return i;
			}

            return def;
        }

		public static short? GetShort(this IDictionary<String, String> dic, string name, System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            short i;
			if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            if (dic.TryGetValue(name, out s) && short.TryParse(s, System.Globalization.NumberStyles.Any, cultureInfo, out i))
                return i;

            return null;
        }
		
		public static short GetShort(this IDictionary<String, object> dic, string name, short def = default(short), System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			object s;
            if (dic.TryGetValue(name, out s))
			{
                return (short)Convert.ChangeType(s, typeof(short), cultureInfo);
			}

            return def;
        }
            
		public static decimal GetDecimal(this IDictionary<String, String> dic, string name, decimal def, bool throwOnParseError = false, System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            decimal i;
            if (dic.TryGetValue(name, out s))
			{
				if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
				if (throwOnParseError)
					return decimal.Parse(s);
				else if(decimal.TryParse(s, System.Globalization.NumberStyles.Any, cultureInfo, out i))
					return i;
			}

            return def;
        }

		public static decimal? GetDecimal(this IDictionary<String, String> dic, string name, System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            string s;
            decimal i;
			if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            if (dic.TryGetValue(name, out s) && decimal.TryParse(s, System.Globalization.NumberStyles.Any, cultureInfo, out i))
                return i;

            return null;
        }
		
		public static decimal GetDecimal(this IDictionary<String, object> dic, string name, decimal def = default(decimal), System.Globalization.CultureInfo cultureInfo = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));

            if (cultureInfo == null) cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			object s;
            if (dic.TryGetValue(name, out s))
			{
                return (decimal)Convert.ChangeType(s, typeof(decimal), cultureInfo);
			}

            return def;
        }
	}
}
