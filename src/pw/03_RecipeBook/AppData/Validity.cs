using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace RecipeBook.AppData
{
    internal class Validity
    {
        /// <summary>
        /// Official W3C pattern taken from https://www.w3.org/TR/2012/WD-html-markup-20120329/input.email.html
        /// </summary>
        /// <example>
        /// Matches foo-bar.baz@example.com
        /// </example>
        private static string emailPattern = "^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$";
        /// <summary>
        /// Pattern used by browsers, taken from https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/email#basic_validation
        /// </summary>
        /// <example>
        /// Match: foo@mañana.com
        /// </example>
        private static string emailInternationalPattern = "^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
        /// <summary>
        /// Meets secure medium/strong password specification:
        /// must be at least 10 characters long containing mixed-case letters, numbers and special symbols
        /// </summary>
        /// <example>
        /// Matches m#P_5*2#s@a^p$V%
        /// </example>
        private static string passPattern = "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\\$%\\^&\\*]).{10,}$";/// <summary>
        /// Russian mobile phone number without country region digit or leading plus (exactly 10 digits)
        /// </summary>
        /// <example>
        /// Matches 9012345678
        /// </example>
        private static string phonePattern = "^[0-9]{10}$";

        public static bool checkEmail(string val) => Regex.IsMatch(val, emailPattern, RegexOptions.IgnoreCase);
        public static bool checkFilled(string val) => !(String.IsNullOrEmpty(val) || String.IsNullOrWhiteSpace(val));
        public static bool checkWhole(string val) => Regex.IsMatch(val, "^[0-9]+$");
        public static bool checkPass(string val) => Regex.IsMatch(val, passPattern);
        public static bool checkPhone(string val) => Regex.IsMatch(val, phonePattern);
        public static void setValidityState(Control ctl, bool? isValid = null)
        {
            if (isValid == true)
            {
                ctl.Background = new SolidColorBrush(Color.FromArgb(255, 210, 249, 210));
                ctl.BorderBrush = Brushes.DarkGray;
            }
            else if (isValid == false)
            {
                ctl.Background = new SolidColorBrush(Color.FromArgb(255, 248, 211, 211));
                ctl.BorderBrush = Brushes.Red;
            }
            else
            {
                ctl.Background = Brushes.White;
            }

        }

    }
}
