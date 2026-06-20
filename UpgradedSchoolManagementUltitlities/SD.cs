using System.Text.RegularExpressions;

namespace UpgradedSchoolManagementUltitlities
{
    public static class SD
    {
        /// <summary>
        /// Normalizes a phone number by removing non-digit characters.
        /// Useful for comparing and storing phone numbers consistently.
        /// </summary>
        /// <param name="phone">The phone number to normalize</param>
        /// <returns>Normalized phone number (digits only)</returns>
        public static string NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            // Remove all non-digit characters
            return Regex.Replace(phone.Trim(), @"\D", "");
        }

        /// <summary>
        /// Validates if a phone number has a minimum length after normalization.
        /// </summary>
        /// <param name="phone">The phone number to validate</param>
        /// <param name="minLength">Minimum length required (default: 10)</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidPhone(string? phone, int minLength = 10)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            string normalized = NormalizePhone(phone);
            return normalized.Length >= minLength;
        }

        /// <summary>
        /// Formats a normalized phone number for display (e.g., +234-XXX-XXX-XXXX).
        /// Assumes Nigerian phone format but can be adapted.
        /// </summary>
        /// <param name="normalizedPhone">The normalized phone number (digits only)</param>
        /// <returns>Formatted phone number</returns>
        public static string FormatPhoneForDisplay(string normalizedPhone)
        {
            if (string.IsNullOrWhiteSpace(normalizedPhone) || normalizedPhone.Length < 10)
                return normalizedPhone;

            // For Nigerian format: +234-XXX-XXX-XXXX
            if (normalizedPhone.StartsWith("234"))
                return $"+{normalizedPhone.Substring(0, 3)}-{normalizedPhone.Substring(3, 3)}-{normalizedPhone.Substring(6, 3)}-{normalizedPhone.Substring(9)}";

            // Fallback: XXX-XXX-XXXX for last 10 digits
            int startIdx = normalizedPhone.Length > 10 ? normalizedPhone.Length - 10 : 0;
            string last10 = normalizedPhone.Substring(startIdx);
            return $"{last10.Substring(0, 3)}-{last10.Substring(3, 3)}-{last10.Substring(6)}";
        }

        public static string ToNaira(decimal money)
        {
            char naira = (char)8358;
            string Money;
            Money = money.ToString("c");
            return Money.Replace('$', naira);
        }

        /// <summary>
        /// Generates a unique payment reference number.
        /// Format: yyyyMMddHHmm + 3 random uppercase letters.
        /// </summary>
        public static string GenerateUniqueNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
            var random = new Random();
            var letters = new char[3];
            for (int i = 0; i < 3; i++)
                letters[i] = (char)('A' + random.Next(26));
            return timestamp + new string(letters);
        }

        public static (string Grade, string Remark) GetGradeAndRemark(decimal totalScore)
        {
            if (totalScore >= 75) return ("A", "Distinction");
            if (totalScore >= 65) return ("B", "Very Good");
            if (totalScore >= 55) return ("C", "Good");
            if (totalScore >= 40) return ("D", "Fair");
            return ("E", "Poor");
        }

        public static string GetPrincipalRemark(double averagescore)
        {
            if (averagescore >= 75)
                return "This student has demonstrated an exceptional level of academic excellence " +
                       "this term. The results reflect outstanding dedication, intellectual depth, " +
                       "and consistent effort. Keep up this commendable performance.";

            if (averagescore >= 65)
                return "This student has performed very well this term, showing strong academic " +
                       "ability and commendable effort. With continued focus and dedication, " +
                       "distinction-level performance is well within reach.";

            if (averagescore >= 55)
                return "The student has achieved a satisfactory result this term. While the " +
                       "performance is good, there is clear potential for improvement with more " +
                       "consistent study habits and greater engagement in class.";

            if (averagescore >= 40)
                return "The student's performance this term is below the expected standard. " +
                       "This result is a strong indication that more serious attention must be " +
                       "given to studies. Parents are kindly advised to provide additional " +
                       "academic support at home.";

            return "This result is deeply concerning. The student's academic performance has " +
                   "fallen far short of the required standard. An urgent meeting between " +
                   "parents, the student, and the class teacher is strongly recommended " +
                   "to chart a way forward.";
        }

        public static string GetClassTeacherRemark(decimal averageScore)
        {
            return averageScore switch
            {
                >= 75 => "An exceptional academic performance. You have demonstrated outstanding commitment to your studies. Maintain this excellent standard and continue striving for greater achievements.",

                >= 65 => "A very commendable performance. Your hard work and dedication are evident in your results. Remain focused and aim for even higher academic excellence.",

                >= 55 => "A good performance overall. There is noticeable effort in your work, but greater consistency and determination will help you achieve better results.",

                >= 40 => "A fair performance. While some progress has been made, more commitment and serious attention to your studies are required for improvement.",

                _ => "A poor performance this term. Greater dedication, regular study habits, and active participation in learning activities are necessary for significant improvement."
            };
        }
    }
}
