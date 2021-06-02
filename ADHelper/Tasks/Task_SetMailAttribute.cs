/*
using System;
using System.DirectoryServices;

namespace ExportAD
{
    class Program
    {
        static void Main(string[] args)
        {
            string ou2013 = "OU=2013,OU=Highly Managed,OU=Users,OU=Student.Greenlease,DC=student,DC=rockhurst,DC=int";
            string ou2014 = "OU=2014,OU=Highly Managed,OU=Users,OU=Student.Greenlease,DC=student,DC=rockhurst,DC=int";
            string ou2015 = "OU=2015,OU=Highly Managed,OU=Users,OU=Student.Greenlease,DC=student,DC=rockhurst,DC=int";
            string ou2016 = "OU=2016,OU=Highly Managed,OU=Users,OU=Student.Greenlease,DC=student,DC=rockhurst,DC=int";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\temp\ad_students.csv", true))
            {
                popFile(file, ou2013, "2013");
                popFile(file, ou2014, "2014");
                popFile(file, ou2015, "2015");
                popFile(file, ou2016, "2016");
            }  

        }
        // iterate users in an OU, write their mail attribute such as:
        //       sAMAccountName + "@amdg.rockhursths.edu"
        public static void popFile(System.IO.StreamWriter file, string dn, string year)
        {
            //establish the OU
            DirectoryEntry ou = new DirectoryEntry("LDAP://" + dn);

            //establish the search object for above OU
            DirectorySearcher searcher = new DirectorySearcher(ou);

            //filter the search
            searcher.Filter = "(ObjectCategory=user)";


                //go through every single search result and update its mail attribute
                foreach (SearchResult result in searcher.FindAll())
                {
                    //entry for every user
                    DirectoryEntry user = null;
                    user = result.GetDirectoryEntry();

                    string givenName = user.Properties["givenName"].Value.ToString();
                    string sn = user.Properties["sn"].Value.ToString();
                    string sAMAccountName = user.Properties["sAMAccountName"].Value.ToString().ToLower();
                    string mail = user.Properties["mail"].Value.ToString().ToLower();


                    file.WriteLine(year + "," + givenName + "," + sn + "," + sAMAccountName + "," + mail);
                }

        }
    }
}
*/