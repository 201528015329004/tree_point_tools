using System.Globalization;
using System.Windows.Controls;

namespace Large_File_Transformations
{
    public class TransformationStringValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string text = value as string;
            return new ValidationResult(TransformationStringParser.Parse(text).IsValid, null);
        }
    }
}