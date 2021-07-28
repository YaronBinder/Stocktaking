using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Stocktaking.Models
{
    public class TimeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is null) return new ValidationResult(false, "שדה ריק");
            TimeSpan time = TimeSpan.Zero;
            try
            {
                if((value as string).Length > 0)
                {
                    time = TimeSpan.Parse(value as string);
                }
            }
            catch
            {
                return new ValidationResult(false, $"זמן שגוי");
            }

            if (!TimePatternValidation(time) || !TimeSpan.TryParse(time.ToString(), out _))
            {
                return new ValidationResult(false, "אנא הכנס זמן תקין");
            }
            return ValidationResult.ValidResult;
        }

        private bool TimePatternValidation(TimeSpan time)
            => new Regex(@"^(([0-1][0-9])|([2][0-3]))(:([0-5][0-9])){0,2}$").IsMatch(time.ToString());
    }
}
