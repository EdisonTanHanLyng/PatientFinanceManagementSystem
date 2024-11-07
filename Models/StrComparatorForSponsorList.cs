
namespace PFMS_MI04.Models
{
    public class StrComparatorForSponsorList
    {
        // Println
        public static void println(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        // Print a Dictionary object of type <string, object>
        public static void printDictionary(Dictionary<string, object> dict)
        {
            foreach (KeyValuePair<string, object> kvp in dict)
            {
                System.Diagnostics.Debug.WriteLine("Key: " + kvp.Key + " Value: " + kvp.Value.ToString());
            }
        }

        // Print a List of objects
        public static void printList(List<object> list)
        {
            foreach (object obj in list)
            {
                System.Diagnostics.Debug.WriteLine(obj.ToString());
            }
        }

    

        // Wrapper method for lcs
        public static int calcLCS(string str1, string str2, bool caseSens)
        {
            int str1Len = str1.Length;
            int str2Len = str2.Length;
            int output = 0;

            output = lcs(str1, str2, str1Len, str2Len, caseSens);
            return output;
        }

        // Wrapper method for sponsors item sorting 
        public static List<SponsorListItem> sortSponsor(List<SponsorListItem> sponsors, string keyword)
        {
            printList(sponsors);
            // Used to detect if the search string is a number OR string (ID or Name)
            try
            {
                Double.Parse(keyword);
                //Search by ID
                sponsors = sortByBestMatch(sponsors, keyword, "id");
            }
            catch (FormatException ex)
            {
                //Filter by Name
                sponsors = sortByBestMatch(sponsors, keyword, "name");
            }
            printList(sponsors);
            return sponsors;
        }

     

        //Formatting long numbers by putting "-" in between.
        public static string dashSplitter(string inputStr, string format)
        {
            string[] numstr = format.Split('-');
            int splitAt = 0;
            for (int i = 0; i < numstr.Length - 1; i++)
            {
                try
                {
                    splitAt += int.Parse(numstr[i]);
                    inputStr = inputStr.Insert(splitAt, "-");
                    splitAt++;
                }
                catch (FormatException ex)
                {
                    System.Diagnostics.Debug.WriteLine("FormatException: " + ex.Message);
                }
            }
            return inputStr;
        }

        // Customized Insertion Sort that rejects items that lower than a calculated threshold
        // Sorting Sponsor Items
        private static List<SponsorListItem> sortByBestMatch(List<SponsorListItem> sponsors, string keyword, string sortBy)
        {
            int threshold = 0;
            if (keyword.Length < 2)
            {
                threshold = 1;
            }
            else
            {
                threshold = keyword.Length - 1;
            }

            for (int i = 0; i < sponsors.Count; i++)
            {
                if (calcLCS(keyword, chosenField(sponsors[i], sortBy), false) < threshold)
                {
                    System.Diagnostics.Debug.WriteLine("Removed: " + sponsors[i].SponsorName);
                    sponsors.RemoveAt(i);
                    if (i == 0)
                    {
                        i = -1;
                    }
                    else if (i > 0)
                    {
                        i--;
                    }
                }
            }

            if (sponsors.Count > 0)
            {
                SponsorListItem[] sponsorsArr = sponsors.ToArray();
                System.Diagnostics.Debug.WriteLine("Sorting");
                for (int i = 1; i < sponsorsArr.Length; ++i)
                {
                    int key = calcLCS(keyword, chosenField(sponsorsArr[i], sortBy), false);
                    SponsorListItem keyItem = sponsorsArr[i];
                    int j = i - 1;

                    while (j >= 0 && calcLCS(keyword, chosenField(sponsorsArr[j], sortBy), false) < key)
                    {
                        sponsorsArr[j + 1] = sponsorsArr[j];
                        j = j - 1;
                    }
                    sponsorsArr[j + 1] = keyItem;
                }
                sponsors = sponsorsArr.ToList();
            }
            return sponsors;
        }

       

        // Iterative matrix approach to longest common subsequence(LCS) 
        // Higher the return value, the better the match
        private static int lcs(string str1, string str2, int str1Len, int str2Len, bool caseSens)
        {
            int[,] lcsMatrix = new int[str1Len + 1, str2Len + 1];
            char[] charArr1;
            char[] charArr2;

            if (caseSens)
            {
                charArr1 = str1.ToCharArray();
                charArr2 = str2.ToCharArray();
            }
            else
            {
                charArr1 = str1.ToUpper().ToCharArray();
                charArr2 = str2.ToUpper().ToCharArray();
            }

            for (int i = 0; i <= str1Len; i++)
            {
                for (int j = 0; j <= str2Len; j++)
                {
                    if (i == 0 || j == 0) //forming the '0' boundary at index 0 for both strings
                    {
                        lcsMatrix[i, j] = 0;
                    }
                    else if (charArr1[i - 1] == charArr2[j - 1]) // if the characters are the same
                    {
                        lcsMatrix[i, j] = lcsMatrix[i - 1, j - 1] + 1;
                    }
                    else // if the characters are different
                    {
                        lcsMatrix[i, j] = max(lcsMatrix[i - 1, j], lcsMatrix[i, j - 1]);
                    }
                }
            }
            //System.Diagnostics.Debug.WriteLine(str1 + " " + str2 + " " + lcsMatrix[str1Len,str2Len]);
            return lcsMatrix[str1Len, str2Len];
        }

        private static string chosenField(SponsorListItem sponsor, string field)
        {
            string ret = "";
            switch (field)
            {
                case "id":
                    ret = sponsor.SponsorId.ToString();
                    break;
                case "name":
                    ret = sponsor.SponsorName;
                    break;
                case "phone":
                    ret = sponsor.SponsorPhone;
                    break;
                default:
                    ret = "null";
                    break;
            }
            return ret;
        }

        private static string parseDateToString(string dateStr)
        {
            string parsedDate = "";
            try
            {
                parsedDate = DateOnly.Parse(dateStr).ToString();
            }
            catch (FormatException fEx)
            {
                println("FormatException: " + fEx.Message);
                parsedDate = "";
            }
            return parsedDate;
        }

        private static int max(int n1, int n2)
        {
            if (n1 > n2)
            {
                return n1;
            }
            else
            {
                return n2;
            }
        }

        //For sponsor List
        private static void printList(List<SponsorListItem> list)
        {
            System.Diagnostics.Debug.Write("[");
            foreach (SponsorListItem item in list)
            {
                System.Diagnostics.Debug.Write(item.SponsorName + ",");
            }
            System.Diagnostics.Debug.Write("]");
            System.Diagnostics.Debug.WriteLine("");
        }

       

        private static void printArr(string[] strArr)
        {
            System.Diagnostics.Debug.Write("[");
            foreach (string str in strArr)
            {
                System.Diagnostics.Debug.Write(str + ",");
            }
            System.Diagnostics.Debug.Write("]");
            System.Diagnostics.Debug.WriteLine("");
        }

        private static string combineStr(string[] stringArray)
        {
            string output = "";
            foreach (string str in stringArray)
            {
                output += str;
            }
            return output;
        }

        private static string combineStr(char[] charArray)
        {
            string output = "";
            foreach (char c in charArray)
            {
                output += c;
            }
            return output;
        }

        // ----- Unit Testing for StrComparator -----

        public static int test()
        {
            int result = 0;
            string[,] testcases = {     //  |   Case Description   | Expected Output |
                {"ABC", "ACD"},         //  Basic                           2
                {"ABC", "ABC"},         //  Identical strings               3
                {"ABC", "DEF"},         //  No Common Subsequence           0
                {"A", "A"},             //  Single Char Match               1
                {"A", "B"},             //  Single Char no Match            0   
                {"ABCDEF", "AB"},       //  Subsequence at start            2
                {"ABCDEF", "EF"},       //  Subsequence at end              2
                {"AXBYCZ", "ABC"},      //  Interleaved subsequence         3
                {"AABBA", "ABABA"},     //  Repeated Chars                  4
                {"AGGTAB", "GXTXAYB"},  //  Long Strings                    4
                {"abc", "ABC"},         //  Case Sensitivity                0
                {"A!B@C", "A@C"},       //  Special Characters              3
                {"12345", "135"},       //  Numerals                        3
                {"", ""},               //  Empty String                    0
                {"ABC", ""},            //  One Empty String                0
            };
            int[] expectedOut = {
                2,
                3,
                0,
                1,
                0,
                2,
                2,
                3,
                4,
                4,
                0,
                3,
                3,
                0,
                0
            };
            result = testerLCS(testcases, expectedOut);
            return result;
        }

        private static int testerLCS(string[,] testcases, int[] expectedOut)
        {
            System.Diagnostics.Debug.WriteLine(" -== LCS Algorithm Testing ==- ");
            System.Diagnostics.Debug.WriteLine(" ");
            int output = 0;
            int lcsOut = 0;
            for (int i = 0; i < expectedOut.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine("-------- Case: " + (i + 1) + " --------");
                System.Diagnostics.Debug.WriteLine("| " + testcases[i, 0] + " | " + testcases[i, 1] + " | Expected = " + expectedOut[i]);
                lcsOut = calcLCS(testcases[i, 0], testcases[i, 1], true);
                System.Diagnostics.Debug.Write("| Actual Output: " + lcsOut + " | Result: ");
                if (lcsOut == expectedOut[i])
                {
                    System.Diagnostics.Debug.Write("PASS");
                    output++;
                }
                else
                {
                    System.Diagnostics.Debug.Write("FAIL ");
                }
                System.Diagnostics.Debug.WriteLine("");
            }
            System.Diagnostics.Debug.WriteLine("-----------------------");
            System.Diagnostics.Debug.WriteLine("| Result: " + output + "/" + testcases.GetLength(0) + " Passed");
            return output;
        }
    }
}
