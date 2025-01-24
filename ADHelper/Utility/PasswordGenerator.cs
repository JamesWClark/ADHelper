using System;

namespace ADHelper.Utility {
    public static class PasswordGenerator {
        public static string WordListPassword(int n) {
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
    }
}