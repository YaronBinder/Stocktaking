using System;
using System.Globalization;
using System.Windows.Controls;

namespace Stocktaking.Models;

public class DateValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string dateValue)
            return new ValidationResult(true, "שדה ריק");

        if (dateValue.Replace("/", "").Replace("_", "").Length < 8)
        {
            dateValue = dateValue.Replace("/", "").Replace("_", "");
        }

        if (dateValue.Length < 10)
            return new ValidationResult(true, "הכנס/י תאריך מלא");

        DateTime date;
        try
        {
            date = DateTime.Parse(value as string);
        }
        catch
        {
            return new ValidationResult(false, $"תאריך שגוי");
        }

        if (date > DateTime.Now || !DateTime.TryParse(date.ToString(), out _))
        {
            return new ValidationResult(false, "אנא הכנס/י תאריך תקין");
        }
        return ValidationResult.ValidResult;
    }
}
