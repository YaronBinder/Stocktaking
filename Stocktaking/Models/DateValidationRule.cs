using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Stocktaking.Models
{
    public class DateValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is null) return new ValidationResult(false, "שדה ריק");
            DateTime date = DateTime.Now;
            try
            {
                if((value as string).Length > 0)
                {
                    date = DateTime.Parse(value as string);
                }
            }
            catch
            {
                return new ValidationResult(false, $"תאריך שגוי");
            }

            if(date > DateTime.Now || !DateTime.TryParse(date.ToString(), out _))
            {
                return new ValidationResult(false, "אנא הכנס תאריך תקין");
            }
            return ValidationResult.ValidResult;
        }
    }
}
