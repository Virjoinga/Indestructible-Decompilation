using System;
using System.Globalization;
using UnityEngine;

public static class NumberFormat
{
	private static CultureInfo _culture = CultureInfo.CreateSpecificCulture("en-US");

	public static float RoundTo(float a, int precision)
	{
		float num = Mathf.Pow(10f, precision);
		return Mathf.Round(a * num) / num;
	}

	public static string Get(IFormattable number)
	{
		return Get(number, 0);
	}

	public static string Get(IFormattable number, int precision)
	{
		return number.ToString("N" + precision, _culture);
	}

	public static string TryRound(IFormattable number)
	{
		string text = number.ToString("N1", _culture);
		return text.Replace(".0", string.Empty);
	}
}
