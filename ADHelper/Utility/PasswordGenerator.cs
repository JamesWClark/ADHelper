using System;
using System.Text;

namespace ADHelper.Utility {
    public static class PasswordGenerator {
        public static string WordList5Password(int n) {
            string pw = "";
            Random rand = new Random();
            for (int i = 0; i < n; i++) {
                pw += Config.WordList5.Words[rand.Next(0, Config.WordList5.Words.Length)];
            }
            String end = "69"; // because high school students
            while (end == "69") {
                end = "" + rand.Next(0, 10) + rand.Next(0, 10);
            }
            return pw + end;
        }

        public static string RandomPassword(int length) {
            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ012345678901234567890123456789";
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            builder.Append(chars[random.Next(chars.IndexOf('A'), chars.IndexOf('Z'))]); // Uppercase letter
            builder.Append(chars[random.Next(chars.IndexOf('a'), chars.IndexOf('z'))]); // Lowercase letter
            builder.Append(chars[random.Next(chars.IndexOf('Z') + 1, chars.Length - 1)]); // Digit
            for (int i = 3; i < length; i++) {
                builder.Append(chars[random.Next(0, chars.Length)]); // Random character
            }
            return builder.ToString();
        }
    }
}