using System;
using System.Text.RegularExpressions;

namespace Large_File_Transformations
{
    public class TransformationStringParser
    {
        public double Tx { get; set; }
        public double Ty { get; set; }
        public double Tz { get; set; }
        public double Rz { get; set; }

        public bool IsValid { get; set; }

        private TransformationStringParser(string parseText)
        {
            this.IsValid = this.ParseFromText(parseText);
        }

        private bool ParseFromText(string parseText)
        {
            var matcher = new Regex(@"[-+]?[0-9]*\.?[0-9]+");
            var matches = matcher.Matches(parseText);
            if (matches.Count != 4)
                return false;

            try
            {
                Tx = double.Parse(matches[0].Value);
                Ty = double.Parse(matches[1].Value);
                Tz = double.Parse(matches[2].Value);
                Rz = double.Parse(matches[3].Value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static TransformationStringParser Parse(string parseText)
        {
            return new TransformationStringParser(parseText);
        }
    }
}