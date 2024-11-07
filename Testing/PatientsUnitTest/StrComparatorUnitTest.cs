using PFMS_MI04.Models;
using PFMS_MI04.Models.Patients;

namespace PFMS_MI04.Testing.PatientsUnitTest
{
    public class StrComparatorUnitTest
    {
        // Unit Testing (Self Made) for StrComparator
        // By: Ariel Starling | 700041728 
        // Tested Algorithms/Functions: 3
        // - LCS Algorithm
        // - Date Input Parsing Function
        // - Date LCS Function
        // - Expanded Searching Functionality

        private StrComparator testCore = new StrComparator();

        // Unit Testing Wrapper
        public string test()
        {
            int r1 = test1();
            println(" ");
            int r2 = test2();
            println(" ");
            int r3 = test3();

            string results =
                " ---------- StrComparator Testing ----------\n" +
                " Testing LCS Result: " + r1 + "\n" +
                " Testing Date-Matching Result: " + r2 + "\n" + 
                " Testing Expanded Searching Result: " + r3 + "\n"
            ;
            return results;
        }

        // Testing LCS Algorithm
        private int test1()
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

        // Testing Search-By-Date Core functions
        private int test2()
        {
            int result = 0;
            //Test Detect User Input (Includes Expected Out at testcases2[0..n][m-1]))
            string[,] testcases1 = {        //  Expected FormatNum 
                {"05-08-2024","0"},         //          0
                {"05-08-24","1"},           //          1
                {"5-08-2024","2"},          //          2
                {"5-08-24","3"},            //          3
                {"05-August-2024","4"},     //          4
                {"05-August-24","5"},       //          5
                {"Aug-2024","6"},           //          6
                {"Aug-24","7"},             //          7
                {"August-2024","8"},        //          8
                {"August-24","9"},          //          9
                {"05-08","10"},             //          10
                {"05-Aug","11"},            //          11
                {"05-August","12"},         //          12
                {"Aug","13"},               //          13
                {"August","14"},            //          14
                {"2024","15"}               //          15
            };
            string[] types1 = {
                "/",
                ".",
                " ",
                "_",
                "|",
                "+"
            };

            // Testing Date Comparer (Includes Expected Out at testcases2[0..n][m-1])
            string[,] testcases2 = {                            // Expected Out
                {"05/08/2024","05/08/2024","day","2"},          //      2       Same Date
                {"05/08/2024","05/08/2024","month","2"},        //      2
                {"05/08/2024","05/08/2024","year","4"},         //      4
                {"05/08/2024","05/08/2024","monthyear","6"},    //      6
                {"05/08/2024","05/08/2024","daymonth","4"},     //      4
                {"05/08/2024","05/08/2024","all","8"},          //      8   
                {"05/08/2024","25/12/2020","day","1"},          //      1       Completely Different Dates
                {"05/08/2024","25/12/2020","month","0"},        //      0
                {"05/08/2024","25/12/2020","year","3"},         //      3
                {"05/08/2024","25/12/2020","monthyear","3"},    //      3
                {"05/08/2024","25/12/2020","daymonth","1"},     //      1
                {"05/08/2024","25/12/2020","all","4"}           //      4
            };
            result += testerDateS(testcases1, types1);
            println("");
            result += testerDateComparer(testcases2);
            return result;
        }

        // Testing Expanded Search
        private int test3()
        {
            int result = 0;         //     Name          NRIC           Race        Sponsor        Tel        DialSche       Status
            string[,] testData = { // {0} => (1,2) | {1} => (3,4) | {2} => (7) | {3} => (9) | {4} => (10) | {5} => (11) | {6} => (16)
                {"Margaret", "123456789", "Iban", "Sponsor1", "013123123", "Tuesday-Thursday-Saturday", "Active"},
                {"Glenn", "123456789", "Malay", "Sponsor1", "013123123", "Tuesday-Thursday-Saturday", "Active"},
                {"Quagmire", "987654321", "Iban", "Sponsor1", "013123123", "Tuesday-Thursday-Saturday", "Inactive"},
                {"Peter", "123456789", "Chinese", "Independent", "012121212", "Tuesday-Thursday-Saturday", "Active"},
                {"Griffin", "121212121", "Chinese", "Independent", "013123123", "Tuesday-Thursday-Saturday", "Active"},
                {"Stewie", "1001001", "British", "Government", "013123123", "Monday-Wednesday-Friday", "Active"},
                {"Brian", "0110110", "Canine", "Independent", "013123123", "Monday-Wednesday-Friday", "Inactive"},
                {"Lois", "123456789", "American", "Government", "013123123", "Monday-Wednesday-Friday", "Active"},
                {"Chris", "0220220", "American", "Sponsor1", "013123123", "Tuesday-Thursday-Saturday", "Active"},
                {"Megatron", "2002002", "American", "None", "0321321321", "Monday-Wednesday-Friday", "Inactive"}
            };

            string[,] testcases = { // {0} => Name | {1} => dialSche | {2} => status | {3} => race | {4} => tel | {5} => id | {6} => sponsor
                {"Margaret","","","","","",""},                                                             // 1 Margaret
                {"Glenn","","","","","",""},                                                                // 2 Glenn
                {"Quagmire","","","","","",""},                                                             // 3 Quagmire
                {"","","","Iban","","",""},                                                                 // 4 Margaret-Quagmire-Lois-Chris-Megatron
                {"","","","Chinese","","",""},                                                              // 5 Peter-Griffin
                {"","","","American","","","Government"},                                                   // 6 Lois
                {"Margaret","","","Iban","","",""},                                                         // 7 Margaret
                {"", "Monday-Wednesday-Friday", "", "", "", "", ""},                                        // 8 Stewie-Brian-Lois-Megatron
                {"","","Inactive","","","",""},                                                             // 9 Quagmire-Brian-Megatron
                {"","","","","","0220220",""},                                                              // 10 Chris
                {"","","","","","123456789",""},                                                            // 11 Margaret-Glenn-Peter-Lois
                {"","","","","012121212","",""},                                                            // 12 Peter
                {"","","Inactive","","013123123","","Independent"},                                         // 13 Brian
                {"","","Inactive","","","","Sponsor1"},                                                     // 14 Quagmire
                {"", "", "", "Chinese", "", "121212121", ""},                                               // 15 Griffin
                {"Megatron","Monday-Wednesday-Friday","Active","American","0321321321","2002002","None"},   // 16 {None} 
                {"Megatron","Monday-Wednesday-Friday","Inactive","American","0321321321","2002002","None"}, // 17 Megatron 
                {"","","","","","",""}                                                                      // 18 Margaret-Glenn-Quagmire-Pater-Griffin-Stewie-Brian-Lois-Chris-Megatron
            };

            string[] testExp = {
                "Margaret",
                "Glenn",
                "Quagmire",
                "Margaret-Quagmire",
                "Peter-Griffin",
                "Lois",
                "Margaret",
                "Stewie-Brian-Lois-Megatron",
                "Quagmire-Brian-Megatron",
                "Chris",
                "Margaret-Glenn-Peter-Lois",
                "Peter",
                "Brian",
                "Quagmire",
                "Griffin",
                "",
                "Megatron",
                "Margaret-Glenn-Quagmire-Peter-Griffin-Stewie-Brian-Lois-Chris-Megatron"
            };

            List<PatientProfile> testProfiles = new List<PatientProfile>();
            List<ExpPatientSearch> searches = new List<ExpPatientSearch>();
            for(int i = 0; i < testData.GetLength(0); i++)
            {
                PatientProfile tProf = new PatientProfile(testData[i, 0], testData[i,0], testData[i,1], testData[i,1], 0, "", testData[i,2], "", testData[i,3], testData[i,4], testData[i,5], "", "", "", "", testData[i,6], "");
                testProfiles.Add(tProf);
            }
            for(int i = 0; i < testcases.GetLength(0); i++)
            {
                ExpPatientSearch tSearch = new ExpPatientSearch(testcases[i, 0], testcases[i, 1], testcases[i, 2], testcases[i, 3], testcases[i, 4], testcases[i, 5], testcases[i, 6]);
                searches.Add(tSearch);
            }

            // test
            result = testExpSearch(testProfiles, searches, testExp);
            return result;
        }

        // Testing Merge Sort // Incomplete
        private int test4()
        {
            int result = 0;         
            string[,,] testData = { 
                  // N |  Word  | Expected
                { // 1 | Alicia | Alice,Charlie,David,Eve,Frank,Grace,Hank,Ivy,Jack,Kathy,Leo,Bob
                    {"", "Alice", "", "", ""},
                    {"", "Bob", "", "", ""},
                    {"", "Charlie", "", "", ""},
                    {"", "David", "", "", ""},
                    {"", "Eve", "", "", ""},
                    {"", "Frank", "", "", ""},
                    {"", "Grace", "", "", ""},
                    {"", "Hank", "", "", ""},
                    {"", "Ivy", "", "", ""},
                    {"", "Jack", "", "", ""},
                    {"", "Kathy", "", "", ""},
                    {"", "Leo", "", "", ""},
                },
                { // 2 | Johnny | John,John,John
                    {"", "John", "", "", ""},
                    {"", "John", "", "", ""},
                    {"", "John", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                },
                { // 3 | Ann | Ann,Anna,Annabelle
                    {"", "Ann", "", "", ""},
                    {"", "Annabelle", "", "", ""},
                    {"", "Anna", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                    {"", "", "", "", ""},
                },
            };
            return result;
        }

        // Testing of LCS
        private int testerLCS(string[,] testcases, int[] expectedOut)
        {
            println(" ---== LCS Algorithm Testing ==--- ");
            println(" ");
            int output = 0;
            int lcsOut = 0;
            println(" -----------------------------------------------");
            for (int i = 0; i < expectedOut.Length; i++)
            {
                //println("-------- Case: " + (i + 1) + " --------");
                //println("| " + testcases[i, 0] + " | " + testcases[i, 1] + " | Expected = " + expectedOut[i] + " |");
                lcsOut = testCore.calcLCS(testcases[i, 0], testcases[i, 1], true);
                //System.Diagnostics.Debug.Write("| Actual Output: " + lcsOut + " | Result: ");
                print("|Case " + (i + 1) + "| IN1:[" + testcases[i, 0] + "] | IN2:[" + testcases[i, 1] + "] | CaseSens:[" + true + "] | " + "Expected:[" + expectedOut[i] + "] | OUT:[" + lcsOut + "] | Result: ");
                if (lcsOut == expectedOut[i])
                {
                    println("PASS");
                    output++;
                }
                else
                {
                    println("FAIL ");
                }
            }
            println(" -----------------------------------------------");
            println("| Result: " + output + "/" + testcases.GetLength(0) + " Passed");
            return output;
        }

        // Testing of Date Input Parsing
        private int testerDateS(string[,] testcases, string[] types)
        {
            println(" ---== Date Input Parser Testing ==--- ");
            println(" ");
            int output = 0;
            int expOut = 0;
            int caseN = 1;

            println(" -----------------------------------------------");
            for (int i = 0; i < types.Length; i++)
            {
                for (int j = 0; j < testcases.GetLength(0); j++)
                {
                    try
                    {
                        expOut = Int32.Parse(testcases[j, 1]);
                        testCore.dDIT_Testing("PFMS_Mi04_2024_T",testcases[j, 0].Replace("-", types[i]), out string dateStr, out int parserOut);
                        print("|Case " + caseN + " | IN:[" + testcases[j, 0].Replace("-", types[i]) + "] | Str_Out:[" + dateStr + "] | Exp_Format_Out:[" + testcases[j, 1] + "] | Act_Format_Out:[" + parserOut + "] | Result: ");
                        caseN++;
                        if (expOut == parserOut)
                        {
                            println("PASS");
                            output++;
                        }
                        else
                        {
                            println("FAIL");
                        }
                    }
                    catch (FormatException ex)
                    {
                        println("Error in Date Input Parsing Testing: " + ex.Message);
                    }
                }
            }
            println("-----------------------------------------------");
            println("| Result: " + output + "/" + (testcases.GetLength(0) * types.Length) + " Passed");
            return output;
        }

        // Testing of Date Comparer
        private int testerDateComparer(string[,] testcases)
        {
            println(" ---== Date Comparer Testing ==--- ");
            println(" ");
            int output = 0;
            int expOut = 0;
            int caseN = 1;

            println(" -----------------------------------------------");
            for (int i = 0; i < testcases.GetLength(0); i++)
            {
                try
                {
                    expOut = Int32.Parse(testcases[i, 3]);
                    int compOut = testCore.dC_Testing("PFMS_Mi04_2024_T", testcases[i, 0], testcases[i, 1], testcases[i, 2]);
                    print("|Case " + caseN + " | IN1:[" + testcases[i, 0] + "] | IN2:[" + testcases[i, 1] + "] | Type:[" + testcases[i, 2] + "] | Exp_Out:[" + testcases[i, 3] + "] | Act_Out:[" + compOut + "] | Result: ");
                    caseN++;
                    if (compOut == expOut)
                    {
                        print("PASS\n");
                        output++;
                    }
                    else
                    {
                        print("FAIL\n");
                    }
                }
                catch (FormatException ex)
                {
                    println("Error in Date Comparer Testing: " + ex.Message);
                }
            }
            println("-----------------------------------------------");
            println("| Result: " + output + "/" + testcases.GetLength(0) + " Passed");
            return output;
        }

        private int testExpSearch(List<PatientProfile> testProfiles, List<ExpPatientSearch> testSearches, string[] expected)
        {
            int output = 0;
            println(" ---== Expanded Search Functionality Testing ==--- ");
            println(" ");

            println(" -----------------------------------------------");
            for(int i = 0; i < testSearches.Count(); i++)
            {
                try
                {
                    List<PatientProfile> tempProfiles = new List<PatientProfile>();
                    foreach(PatientProfile item in testProfiles)
                    {
                        tempProfiles.Add(new PatientProfile(item));
                    }
                    List<PatientListItem> sortedL = testCore.eSBBM_Testing("PFMS_Mi04_2024_T", tempProfiles, testSearches[i]);
                    string[] expectedOut = expected[i].Split("-");
                    List<string> actualOut = new List<string>();
                    foreach(PatientListItem item in sortedL)
                    {
                        actualOut.Add(item.PatientName);
                    }
                    
                    print("| Case " + (i+1) + " | IN:[" + testSearches[i].ToString() + "] | Expected:[" + expected[i] + "] | OUT:" + stringArr(actualOut.ToArray()) + " | Result: ");
                    if (actualOut.Count() == expectedOut.Length)
                    {
                        int pass = 0;
                        for(int j = 0; j < expectedOut.Length; j++)
                        {
                            if (actualOut[j].Equals(expectedOut[j]))
                            {
                                pass++;
                            }
                        }

                        if(pass == expectedOut.Length)
                        {
                            println("PASS");
                            output++;
                        }
                        else
                        {
                            println("FAIL (passes:" + pass + ")");
                        }
                    }
                    else if(actualOut.Count() == 0 && expectedOut.Length == 1 && expectedOut[0].Equals(""))
                    {
                        println("PASS");
                        output++;
                    }
                    else
                    {
                        println("FAIL (Length:" + actualOut.Count() + " vs " + expectedOut.Length + ")");
                    }
                }
                catch (FormatException fE)
                {
                    println("Error in Expanded Search Testing: " + fE.Message);
                }
                catch(MethodAccessException mAE)
                {
                    println("Error in Expanded Search Testing: " + mAE.Message);
                }
            }
            println(" -----------------------------------------------");
            println("| Result: " + output + "/" + testSearches.Count() + " Passed");

            return output;
        }

        // ----- Printing Purposes ----- //

        private static void println(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        private static void print(string msg)
        {
            System.Diagnostics.Debug.Write(msg);
        }

        private string stringArr(string[] strArr)
        {
            string str = "";
            str += ("[");
            foreach (string stri in strArr)
            {
                str += (stri + ",");
            }
            str += ("]");
            return str;
        }

    }
}
