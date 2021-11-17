using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Stocktaking.Models;

public class TimeValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string timeValue)
            return new ValidationResult(true, "שדה ריק");

        if (timeValue.Replace(":", "").Replace("_", "").Length < 6)
        {
            timeValue = timeValue.Replace(":", "").Replace("_", "");
        }

        if (timeValue.Length < 8)
            return new ValidationResult(true, "הנכס/י זמן תקין ומלא");

        TimeSpan time;
        try
        {
            time = TimeSpan.Parse(value as string);
        }
        catch
        {
            return new ValidationResult(false, $"זמן שגוי");
        }

        if (!TimePatternValidation(time.ToString()) || !TimeSpan.TryParse(time.ToString(), out _))
        {
            return new ValidationResult(false, "אנא הכנס/י זמן תקין");
        }
        return ValidationResult.ValidResult;
    }

    private bool TimePatternValidation(string time)
        => new Regex(@"^(([0-1][0-9])|([2][0-3]))(:([0-5][0-9])){1,2}$").IsMatch(time);
}
