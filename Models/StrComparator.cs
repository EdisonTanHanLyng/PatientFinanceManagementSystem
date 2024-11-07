using System.Globalization;
using Newtonsoft.Json;
using PFMS_MI04.Models.Patients;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace PFMS_MI04.Models
{
    public class StrComparator
    {
        private static char[] aValues = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        // ----- Console Printing Service ----- //
        // Println
        public static void println(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        public static void print(string msg)
        {
            System.Diagnostics.Debug.Write(msg);
        }

        // Print a Dictionary object of type <string, object>
        public static void printDictionary(Dictionary<string, object> dict)
        {
            foreach(KeyValuePair<string, object> kvp in dict)
            {
                System.Diagnostics.Debug.WriteLine("Key: " + kvp.Key + " Value: " + kvp.Value.ToString());
            }
        }

        // Print a List of objects
        public static void printList(List<object> list)
        {
            foreach(object obj in list)
            {
                System.Diagnostics.Debug.WriteLine(obj.ToString());
            }
        }

        // Print a list of type <PatientHDRecord>
        public static void printList(List<PatientHDRecord> list)
        {
            foreach(PatientHDRecord item in list)
            {
                System.Diagnostics.Debug.WriteLine(item.toString());
            }
        }

        // ----- Wrapper Methods ----- //

        // Wrapper method for lcs
        public int calcLCS(string str1, string str2, bool caseSens)
        {
            int str1Len = str1.Length;
            int str2Len = str2.Length;
            int output = 0;

            output = lcs(str1, str2, str1Len, str2Len, caseSens);
            return output;
        }

        // Wrapper method for patient item sorting 
        public List<PatientListItem> sortPatients(List<PatientListItem> patients, string keyword)
        {
            // Used to detect if the search string is a number OR string (ID or Name)
            try
            {
                Double.Parse(keyword);
                //Search by ID
                patients = sortByBestMatch(patients, keyword, "id");
            }
            catch (FormatException ex)
            {
                //Filter by Name
                patients = sortByBestMatch(patients, keyword, "name");
                println("Search by Name Detected: " + ex.Message);
            }
            return patients;
        }

        // Wrapper method for sorting patients alphabetically
        public List<PatientListItem> sortPatients(List<PatientListItem> patients)
        {
            List<PatientListItem> resList = sortPatientListItemAlpha(patients, "name");
            return resList;
        }

        // Wrapper method for sorting patient profiles alphabetically
        public List<PatientProfile> sortPatients(List<PatientProfile> patients)
        {
            List<PatientProfile> resList = sortPatientListProfileAlpha(patients);
            return resList;
        }

        // Wrapper method for expanded patient item sorting
        public List<PatientListItem> sortPatients(List<PatientProfile> patients, ExpPatientSearch expSearch)
        {
            List<PatientListItem> resList = sortByBestMatch(patients, expSearch); 
            return resList;
        }

        // Wrapper method for patient's HD Record Sorting by Date
        public List<PatientHDRecord> sortHDRecords(List<PatientHDRecord> records, string keyword)
        {
            //keyword = parseToStanString(keyword);
            println("keyword before: " + keyword);
            detectDateInputType(keyword, out string outputDate, out int num);
            println("Sorting Hd Records");
            println("keyword after: " + keyword);
            records = sortByBestMatch(records, keyword);
           
            return records;
        }

        public List<PatientDocument> sortDocuments(List<PatientDocument> docs, string searchStr)
        {
            docs = filterByBestMatch(docs, searchStr);
            return docs;
        }

        public List<SponsorDocument> sortDocuments(List<SponsorDocument> docs, string searchStr)
        {
            docs = filterByBestMatch(docs, searchStr);
            return docs;
        }

        // Given an input string (preferably a long hard-to-read string of numbers), 
        // will insert dashes depending on a format string such as: "3-3-3" to the 'format' parameter
        // Example: input: "123456789", "3-3-3" => output: "123-456-789"
        public string dashSplitter(string inputStr, string format)
        {
            string[] numstr = format.Split('-');
            int splitAt = 0;
            for (int i = 0; i < numstr.Length-1; i++)
            {
                try
                {
                    splitAt += int.Parse(numstr[i]);
                    inputStr = inputStr.Insert(splitAt ,"-");
                    splitAt++;
                }
                catch(FormatException ex)
                {
                    System.Diagnostics.Debug.WriteLine("FormatException: " + ex.Message);
                }
            }
            return inputStr;
        }

        public string inputSanitizer(string inputStr)
        {
            StrComparator.println("Input String: " + inputStr);
            inputStr = string.Join("", inputStr.Split('#', '<', '>', '"', '`'));
            StrComparator.println("Sanitized: " + inputStr);
            return inputStr;
        }

        // Wrapper method for Patient Profile Parser
        public PatientProfile patientProfParser(object json)
        {
            PatientProfile profile = ppP(json);
            return profile;
        }

        // Wrapper Method for HD Record Parsing functionality
        public PatientHDRecord hdrParser(object json)
        {
            PatientHDRecord hdR = hdrP(json);
            return hdR;
        }

        // Wrapper method for Expanded Search Parser
        public ExpPatientSearch expSearchParser(object json)
        {
            ExpPatientSearch exp = epP(json);
            return exp;
        }

        // ----- Internal Functions ----- //

        // - Ariel Starling: Customized MergeSort that rejects items that lower than a calculated threshold
        // Sorting Patient Items
        private List<PatientListItem> sortByBestMatch(List<PatientListItem> patients, string keyword, string sortBy) 
        {
            for(int i = 0; i < patients.Count; i++)
            {
                if(!basicMatcher(keyword, chosenField(patients[i], sortBy)))
                {     
                    patients.RemoveAt(i);
                    if(i == 0)
                    {
                        i = -1;
                    }
                    else if(i > 0)
                    {
                        i--;
                    }
                }
            }

            // Replaced with mergesort
            patients = sortPatientListItem(patients, keyword, sortBy);
            return patients;
        }

        // Basic non case-sensitive string matcher/contains
        private bool basicMatcher(string key, string str)
        {
            key = key.ToUpper();
            str = str.ToUpper();
            return str.Contains(key);
        }

        // Filtering and Sorting Patient List Items by the given expanded search object
        // Effective time complexity: O(nm(xy)) n = number of active fields | m = number of patientProfiles | (xy) = O(nm) of LCS algorithm
        private List<PatientListItem> sortByBestMatch(List<PatientProfile> patients, ExpPatientSearch expSearch)
        {
            // Intersection filtering
            int[] exp = expSearch.getAvailableFields(); 
            for(int set = 0; set < exp.Length; set++) // Incremental/Intersection filtering
            {
                if (exp[set] != 0) // If the search field is active
                {
                    int fSet = set + 1; // fSet => field set => chosen field 
                    if(fSet == 1 || fSet == 4 || fSet == 5 || fSet == 6) // If the specified fields are suited for LCS
                    {
                        // Setting threshold values here is dependent on the chosen field
                        int threshold;
                        if (expSearch.getByFieldNum(fSet).Length < 2)
                        {
                            threshold = 1;
                        }
                        else
                        {
                            threshold = expSearch.getByFieldNum(fSet).Length - 1;
                        }

                        for (int i = 0; i < patients.Count; i++)
                        {
                            //threshold = chosenField(patients[i], fSet).Length - 1; // Uncovered by Unit Testing
                            //if (calcLCS(expSearch.getByFieldNum(fSet), chosenField(patients[i], fSet), false) < threshold)
                            if(!basicMatcher(expSearch.getByFieldNum(fSet), chosenField(patients[i], fSet)))
                            {
                                // Remove item from list
                                patients.RemoveAt(i);
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
                    }
                    else // Normal Filtering => String Matching
                    {
                        for(int i = 0; i < patients.Count; i++)
                        {
                            if(expSearch.getByFieldNum(fSet) != chosenField(patients[i], fSet))
                            {
                                // Remove item from list
                                patients.RemoveAt(i);
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
                    }
                }
            }

            List<PatientListItem> patientList; 
            if(expSearch.searchName != String.Empty) // If the search keyword is empty
            {
                // Sort by Name
                patientList = sortPatientListItem(convertToPatientList(patients), expSearch.searchName, "name");
            }
            else
            {
                // Return the list as is
                patientList = convertToPatientList(patients);
            }
            return patientList;
        }

        // Sorting HD Records by date
        private List<PatientHDRecord> sortByBestMatch(List<PatientHDRecord> records, string keyword)
        {
            int threshold = 0; // threshold represents the amount of allowed mismatch for filtering

            // Filter
            for (int i = 0; i < records.Count; i++)
            {
                int res = inteDateComparer(keyword, parseToStanString(records[i].date), out int format);
                threshold = dynamicThres(format);
                if (res < threshold) 
                {
                    println("Removed: " + parseToStanString(records[i].date) + " | Threshold: " + threshold);
                    records.RemoveAt(i);
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

            // Insertion Sort
            if (records.Count > 0)
            {
                PatientHDRecord[] recordsArr = records.ToArray();
                //System.Diagnostics.Debug.WriteLine("Sorting");
                for (int i = 1; i < recordsArr.Length; ++i)
                {
                    string parsedDate = parseToStanString(records[i].date);
                    int key = inteDateComparer(keyword, parsedDate, out int formatNum);
                    PatientHDRecord keyItem = recordsArr[i];
                    int j = i - 1;

                    while (j >= 0 && calcLCS(keyword, parsedDate, false) < key)
                    {
                        recordsArr[j + 1] = recordsArr[j];
                        j = j - 1;
                    }
                    recordsArr[j + 1] = keyItem;
                }
                records = recordsArr.ToList();
            }
            return records;
        }

        // Filters patient document objects by a search string
        private List<PatientDocument> filterByBestMatch(List<PatientDocument> docs, string keyword)
        {
            // Filter
            for (int i = 0; i < docs.Count; i++)
            {
                if (!basicMatcher(keyword, docs[i].docName))
                {
                    docs.RemoveAt(i);
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
            return docs;
        }

        // Filters sponsor document objects by a search string
        private List<SponsorDocument> filterByBestMatch(List<SponsorDocument> docs, string keyword)
        {
            // Filter
            for (int i = 0; i < docs.Count; i++)
            {
                if (!basicMatcher(keyword, docs[i].docName))
                {
                    docs.RemoveAt(i);
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
            return docs;
        }

        // Iterative matrix approach to longest common subsequence(LCS) 
        // Higher the return value, the better the match
        private int lcs(string str1, string str2, int str1Len, int str2Len, bool caseSens)
        {
            int[, ] lcsMatrix = new int[str1Len + 1, str2Len + 1];
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

            // O(nm)
            for(int i = 0; i <= str1Len; i++)
            {
                for(int j = 0; j <= str2Len; j++)
                {
                    if(i == 0 || j == 0) //forming the '0' boundary at index 0 for both strings
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
            return lcsMatrix[str1Len,str2Len];
        }

        // Utilises LCS Algorithm to compare dates by each field
        // Input format: dd/MM/yyyy for str1 (Usually user input) and str2 | searchBy flags: "all", "day", "month", "year", "daymonth","monthyear" |
        private int dateComparer(string str1, string str2, string searchBy)
        {
            int similarity = 0;
            string[] dateArr1 = str1.Split("/"); // [0] = dd, [1] = MM, [2] = yyyy
            string[] dateArr2 = str2.Split("/");
            switch (searchBy)
            {
                case "all":
                    similarity += calcLCS(dateArr1[0], dateArr2[0], false); 
                    similarity += calcLCS(dateArr1[1], dateArr2[1], false);
                    similarity += calcLCS(dateArr1[2], dateArr2[2], false);
                    break;

                case "day":
                    similarity += calcLCS(dateArr1[0], dateArr2[0], false);
                    break;

                case "daymonth":
                    similarity += calcLCS(dateArr1[0], dateArr2[0], false);
                    similarity += calcLCS(dateArr1[1], dateArr2[1], false);
                    break;

                case "month":
                    similarity += calcLCS(dateArr1[1], dateArr2[1], false);
                    break;

                case "monthyear":
                    similarity += calcLCS(dateArr1[1], dateArr2[1], false);
                    similarity += calcLCS(dateArr1[2], dateArr2[2], false);
                    break;

                case "year":
                    similarity += calcLCS(dateArr1[2], dateArr2[2], false);
                    break;
            }
            //println("str 1=[" + str1 + "] str 2=[" + str2 + "] flag=[" + searchBy + "] OUT = [" + similarity + "]");
            return similarity;
        }

        // Function overloading in case no flag is given, defaults to "all"
        private int dateComparer(string str1, string str2)
        {
            return dateComparer(str1, str2, "all");
        }

        // Calculates the similarity depending on the given input format from the user
        private int inteDateComparer(string str1, string str2, out int formatNum)
        {
            int output = 0;
            string str1Out;
            formatNum = -1;
            detectDateInputType(str1, out str1Out, out formatNum);
            //println("format Num Value " + formatNum);

            if(formatNum >= 0 && formatNum < 6)
            {
                output = dateComparer(str1Out, str2, "all");
            }
            else if(formatNum < 10)
            {
                output = dateComparer(str1Out, str2, "monthyear");
            }
            else if(formatNum < 13)
            {
                output = dateComparer(str1Out, str2, "daymonth");
            }
            else if(formatNum < 15)
            {
                output = dateComparer(str1Out, str2, "month");
            }
            else if(formatNum == 15)
            {
                output = dateComparer(str1Out, str2, "year");
            }
            return output;
        }

        // ----- JSON Parsers ----- //

        // Parses Expanded Search JSON into ExpPatientSearch Object
        private ExpPatientSearch epP(object json)
        {
            ExpPatientSearch expSearch = new ExpPatientSearch();
            var exp = JsonConvert.DeserializeObject<Dictionary<string, string>>(json.ToString());
            if(exp != null)
            {
                try
                {
                    expSearch.searchName = exp["searchBarValue"].ToString();
                    expSearch.dialScheduling = exp["dialysisSchedule"].ToString();
                    expSearch.searchStatus = exp["activityStatus"].ToString();
                    expSearch.searchRace = exp["ethnicity"].ToString();
                    expSearch.searchTel = exp["phoneNumber"].ToString();
                    expSearch.searchId = exp["patientNRIC"].ToString();
                    expSearch.searchSpons = exp["sponsor"].ToString();
                }
                catch(FormatException fEx)
                {
                    println("ERROR: Error in Expanded Search Parser Function: " + fEx.Message);
                }
            }
            return expSearch;
        }

        // Parses Patient Profiles into Patient Profile Object
        private PatientProfile ppP(object json)
        {
            //println("Enter patLParser");
            PatientProfile profile = new PatientProfile();
            var prof = JsonConvert.DeserializeObject<Dictionary<string, string>>(json.ToString());
            if(prof != null)
            {
                try
                {
                    profile.patientFullName = prof["FullName"].ToString();
                    profile.patientNRIC = prof["NRIC"].ToString();
                    profile.patientID = prof["PatientID"].ToString();
                    profile.patientAge = Int32.Parse(prof["Age"].ToString());
                    profile.patientGender = prof["Gender"].ToString();
                    profile.patientEthnicity = prof["Ethnic"].ToString();
                    profile.patientOccupation = prof["CurrentOccupation"].ToString();
                    profile.patientFinance = prof["SponsorList"].ToString();
                    profile.patientPhoneNum = prof["PhoneNumber"].ToString();
                    profile.patientDialSche = prof["DialysisSchedule"].ToString();
                    profile.dialCenterBranch = prof["DialysisCenterBranchLocation"].ToString();
                    profile.aboGrouping = prof["ABOGrouping"].ToString();
                    profile.rhesusFactor = prof["RhesusFactor"].ToString();
                    profile.patientAttendance = prof["AttendanceType"].ToString();
                    profile.patientActivity = prof["ActivityStatus"].ToString();
                    profile.patientProfilePic = prof["ProfilePicture"].ToString();
                }
                catch(FormatException fEx)
                {
                    println("ERROR: Error in Patient Profile Parser Function: " + fEx.Message);
                }
                prof.Clear();
            }
            return profile;
        }

        // Parses HD Records coming from the API Endpoint into a PatientHDRecord Object
        private PatientHDRecord hdrP(object json)
        {
            //println("Enter hdrParser");
            PatientHDRecord record = new PatientHDRecord();
            var hdr = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());
            if (hdr != null)
            {
                try
                {
                    record.date = hdr["date"].ToString();
                    record.time = hdr["time"].ToString();
                    record.tdMin = Int32.Parse(hdr["td_min"].ToString());
                    var weight = JsonConvert.DeserializeObject<Dictionary<string, string>>(hdr["weight"].ToString());
                    if (weight != null)
                    {
                        record.weightPre = Double.Parse(weight["pre"]);
                        record.weightPost = Double.Parse(weight["post"]);
                    }
                    record.dw = Double.Parse(hdr["dw"].ToString());
                    record.idw = Double.Parse(hdr["idw"].ToString());
                    record.dwPercent = Double.Parse(hdr["%dw"].ToString());
                    record.ufGoal = Double.Parse(hdr["uf_goal"].ToString());
                    var preBP = JsonConvert.DeserializeObject<Dictionary<string, string>>(hdr["pre_bp"].ToString());
                    if (preBP != null)
                    {
                        record.preBP_SBP = Int32.Parse(preBP["sbp"]);
                        record.preBP_DBP = Int32.Parse(preBP["dbp"]);
                    }
                    var postBP = JsonConvert.DeserializeObject<Dictionary<string, string>>(hdr["post_bp"].ToString());
                    if (postBP != null)
                    {
                        record.postBP_SBP = Int32.Parse(postBP["sbp"]);
                        record.postBP_DBP = Int32.Parse(postBP["dbp"]);
                    }
                    var pulse = JsonConvert.DeserializeObject<Dictionary<string, string>>(hdr["pulse"].ToString());
                    if (pulse != null)
                    {
                        record.prePulse = Int32.Parse(pulse["pre"]);
                        record.postPulse = Int32.Parse(pulse["post"]);
                    }
                    record.bfr = Int32.Parse(hdr["bpr"].ToString());
                    var epo = JsonConvert.DeserializeObject<Dictionary<string, string>>(hdr["epo"].ToString());
                    if (epo != null)
                    {
                        record.epoType = epo["type"];
                        record.epoDosage = Double.Parse(epo["dosage"]);
                        record.epoQty = Int32.Parse(epo["quantity"]);
                    }
                    var ocm = JsonConvert.DeserializeObject<Dictionary<string, string>>(hdr["ocm_dialysate"].ToString());
                    if (ocm != null)
                    {
                        record.ocm_KtV = Double.Parse(ocm["kt_v"]);
                        record.dial_Cal = ocm["calcium"];
                    }
                    record.dialyzer = hdr["haemofilter_dialyzer_used"].ToString();
                }
                catch (FormatException fEx)
                {
                    println("ERROR: Error in HDR Parser Function: " + fEx.Message);
                }
            }
            return record;
        }

        // ----- Helper Methods ----- //

        // --- Merge Sort PatientListItem --- //
        // Merge 
        private PatientListItem[] mergeP(PatientListItem[] patients, string keyword, string sortBy ,int left, int mid, int right, bool alphabet)
        {
            int len1 = mid - left + 1;
            int len2 = right - mid;
            PatientListItem[] l = new PatientListItem[len1];
            PatientListItem[] r = new PatientListItem[len2];
            int i, j;

            for(i = 0; i < len1; ++i) { l[i] = patients[left + i]; }
            for (i = 0; i < len2; ++i) { r[i] = patients[mid + 1 + i]; }

            i = 0;  
            j = 0;  

            // Insert elements into the main array 
            int k = left;
            while(i < len1 && j < len2)
            {
                if (alphabet)
                {
                    if (getStringValue(chosenField(l[i], sortBy)) >= getStringValue(chosenField(r[j], sortBy)))
                    {
                        patients[k] = l[i];
                        i++;
                    }
                    else
                    {
                        patients[k] = r[j];
                        j++;
                    }
                }
                else
                {
                    if (calcLCS(keyword, chosenField(l[i], sortBy), false) >= calcLCS(keyword, chosenField(r[j], sortBy), false))
                    {
                        patients[k] = l[i];
                        i++;
                    }
                    else
                    {
                        patients[k] = r[j];
                        j++;
                    }
                }
                k++;
            }

            // Copy in the remaining elements from both array into the main array
            while(i < len1)
            {
                patients[k] = l[i];
                i++;
                k++;
            }

            while(j < len2)
            {
                patients[k] = r[j];
                j++;
                k++;
            }
            //println("Merged: Left: " + left + " | Right: " + right);
            return patients;
        }

        // --- Merge Sort --- //
        // This is the recursive mergesort method
        private PatientListItem[] mergeSortP(PatientListItem[] patients, string keyword, string sortBy, int left, int right, bool alphabet)
        {
            if(patients != null)
            {
                if(left < right)
                {
                    int mid = (left + (right - 1)) / 2;
                    if (right == patients.Length - 1)
                    {
                        mid = (left + right) / 2;
                    }

                    // Sort left and right halves
                    //println(left + " | " + right);
                    patients = mergeSortP(patients, keyword, sortBy, left, mid, alphabet);
                    patients = mergeSortP(patients, keyword, sortBy, mid + 1, right, alphabet);

                    patients = mergeP(patients, keyword, sortBy, left, mid, right, alphabet);
                }
            }
            return patients;
        }

        // --- Merge Sort --- //
        // Sorting for PatientListItem
        // Implemented Sorting Algorithm: Merge Sort
        private List<PatientListItem> sortPatientListItem(List<PatientListItem> patients, string keyword, string sortBy)
        {
            if (patients.Count > 0)
            {
                PatientListItem[] patientsArr = patients.ToArray();
                //println("Sorting: patientsArr Length: " + patientsArr.Length);
                // Sort
                patientsArr = mergeSortP(patientsArr, keyword, sortBy, 0, patientsArr.Length-1, false);
                patients = patientsArr.ToList();
            }
            return patients;
        }

        // Sorting PatientListItem objects alphabetically
        private List<PatientListItem> sortPatientListItemAlpha(List<PatientListItem> patients, string sortBy)
        {
            if(patients.Count > 0)
            {
                PatientListItem[] patientsArr = patients.ToArray();
                //Sort
                patientsArr = mergeSortP(patientsArr, "", sortBy, 0, patientsArr.Length - 1, true);
                patients = patientsArr.ToList();
            }
            return patients;
        }

        // --- Merge Sort PatientProfile --- //
        // Merge 
        private PatientProfile[] mergeA(PatientProfile[] patients, int left, int mid, int right)
        {
            int len1 = mid - left + 1;
            int len2 = right - mid;
            PatientProfile[] l = new PatientProfile[len1];
            PatientProfile[] r = new PatientProfile[len2];
            int i, j;

            for (i = 0; i < len1; ++i) { l[i] = patients[left + i]; }
            for (i = 0; i < len2; ++i) { r[i] = patients[mid + 1 + i]; }

            i = 0;
            j = 0;

            // Insert elements into the main array 
            int k = left;
            while (i < len1 && j < len2)
            {
                if (getStringValue(chosenField(l[i], 1)) >= getStringValue(chosenField(r[j], 1)))
                {
                    patients[k] = l[i];
                    i++;
                }
                else
                {
                    patients[k] = r[j];
                    j++;
                }
                k++;
            }

            // Copy in the remaining elements from both array into the main array
            while (i < len1)
            {
                patients[k] = l[i];
                i++;
                k++;
            }

            while (j < len2)
            {
                patients[k] = r[j];
                j++;
                k++;
            }
            //println("Merged: Left: " + left + " | Right: " + right);
            return patients;
        }

        // --- Merge Sort --- //
        // This is the recursive mergesort method
        private PatientProfile[] mergeSortA(PatientProfile[] patients, int left, int right)
        {
            if (patients != null)
            {
                if (left < right)
                {
                    int mid = (left + (right - 1)) / 2;
                    if (right == patients.Length - 1)
                    {
                        mid = (left + right) / 2;
                    }

                    // Sort left and right halves
                    //println(left + " | " + right);
                    patients = mergeSortA(patients, left, mid);
                    patients = mergeSortA(patients, mid + 1, right);

                    patients = mergeA(patients, left, mid, right);
                }
            }
            return patients;
        }

        // Sorting Patient Profile objects alphabetically
        private List<PatientProfile> sortPatientListProfileAlpha(List<PatientProfile> patients)
        {
            List<PatientProfile> finalPat = new List<PatientProfile>();
            if (patients.Count > 0)
            {
                //PatientProfile[] patientsArr = patients.ToArray();
                //Sort
                //patientsArr = mergeSortA(patientsArr, 0, patientsArr.Length - 1);
                //List<PatientProfile> finalPat = new List<PatientProfile>();
                List<PatientProfile>[] groups = groupPatientsByAlphabet(patients);
                for(int i = 0; i < groups.Length; i++)
                {
                    PatientProfile[] patientsArr = groups[i].ToArray();
                    patientsArr = mergeSortA(patientsArr, 0, patientsArr.Length - 1);
                    finalPat.AddRange(patientsArr);
                }
                GC.Collect();
            }
            return finalPat;
        }

        // Used to group patients together into alphabetical groups
        private List<PatientProfile>[] groupPatientsByAlphabet(List<PatientProfile> patients)
        {
            StrComparator.println("Grouping");
            List<PatientProfile>[] groups = new List<PatientProfile>[26]; // 26 letters of the alphabet

            for(int i = 0; i < groups.Length; i++) // Initialise groups
            {
                groups[i] = new List<PatientProfile>();
            }

            foreach(PatientProfile profile in patients) // Group all patients to alphabetical groups
            {
                string name = chosenField(profile, 1).ToUpper();
                char letter = name.ToCharArray()[0];
                for(int i = 0; i < aValues.Length; i++)
                {
                    if(letter == aValues[i])
                    {
                        groups[i].Add(profile);
                    }
                }
            }

            return groups;
        }

        //Determines the value of a given word
        private int getStringValue(string givenString)
        {
            char[] chars = givenString.ToUpper().ToCharArray();
            int strLen = 0;
            if(chars.Length < 2)
            {
                strLen = 1;
            }
            else
            {
                strLen = 2;
            }

            int value = 1;
            for(int i = 0; i < strLen; i++)
            {
                value *= getAlphabetValue(chars[i], i);
            }
            //StrComparator.println("| " + givenString + " | Value: " + value);
            return value;
        }

        // Determines the value of the alphabet letter
        // if iteration is -1, the iteration 'debuff' is ignored | Note: iteration is 0-based
        private int getAlphabetValue(char givenChar, int iteration)
        {
            //char[] aValues = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
            //int[] values = { 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            int alphabetValue = 1;
            if (givenChar != null)
            {
                for(int i = 0; i < aValues.Length; i++)
                {
                    givenChar = char.ToUpper(givenChar);
                     
                    if (givenChar == aValues[i])
                    {
                        if (iteration < 0)
                        {
                            int calc = i - (iteration + 1);
                            if(calc < 0)
                            {
                                calc = 0;
                            }
                            else if(calc > 25)
                            { 
                                calc = 25;
                            }
                            alphabetValue = 91 - ((int)aValues[i]);
                        }
                        else
                        {
                            alphabetValue = 91 - ((int)aValues[i]);
                        }
                    }
                }
                if(alphabetValue <= 0)
                {
                    alphabetValue = 1;
                }
            }
            return alphabetValue;
        }

        // Returns a snippet of the top N items in a given list
        private List<PatientListItem> snippetCreator(List<PatientListItem> patient, int snippetSize)
        {
            List<PatientListItem> newList = new List<PatientListItem>();
            for(int i = 0; i < snippetSize && i < patient.Count; i++) // Handles cases where the input list is shorter than the desired snippet size
            {
                newList.Add(patient[i]);
            }
            return newList;
        }

        // returns a chosen field of a patientListItem
        private string chosenField(PatientListItem patient, string field)
        {
            string ret = "";
            switch (field)
            {
                case "id":
                    ret = patient.PatientId;
                    break;
                case "name":
                    ret = patient.PatientName;
                    break;
                case "phone":
                    ret = patient.PatientPhone;
                    break;
                case "Sponsor":
                    ret = patient.Sponsor;
                    break;
                default:
                    ret = "null";
                    break;
            }
            return ret;
        }

        // Returns a chosen field of a patientProfile items (Currently only suiting specific cases)
        private string chosenField(PatientProfile patient, int field)
        {
            string ret = "";
            switch (field)
            {
                case 1:
                    ret = patient.patientFullName;
                    break;
                case 2:
                    ret = patient.patientDialSche;
                    break;
                case 3:
                    ret = patient.patientActivity;
                    break;
                case 4:
                    ret = patient.patientEthnicity;
                    break;
                case 5:
                    ret = patient.patientPhoneNum;
                    break;
                case 6:
                    ret = patient.patientNRIC;
                    break;
                case 7:
                    ret = patient.patientFinance;
                    break;
                default:
                    ret = "Invalid_Chosen_Field";
                    break;
            }
            return ret;
        }

        // Converts a list of PatientProfiles into a list of PatientListItems
        private List<PatientListItem> convertToPatientList(List<PatientProfile> list)
        {
            List<PatientListItem> newList = new List<PatientListItem>();
            foreach(PatientProfile profile in list)
            {
                newList.Add(profile.toListItem());
            }
            return newList;
        }

        // Returns the threshold value depending on a given format number value given by user input detection
        private int dynamicThres(int formatNum)
        {
            int output = 0;
            if (formatNum >= 0 && formatNum < 6) 
            {
                //all | 8 // -1
                output = 7;
            }
            else if (formatNum < 10)
            {
                //monthyear | 6 // -1
                output = 6;
            }
            else if (formatNum < 13)
            {
                //daymonth | 4 // -1
                output = 3;
            }
            else if (formatNum < 15)
            {
                //month | 2 // -0
                output = 2;
            }
            else if (formatNum == 15)
            {
                //year | 4 // -0
                output = 4;
            }
            return output;
        }

        // Parses a string to follow a standardized format for comparison
        // desired format: dd-mm-yyyy
        private string parseToStanString(string dateStr)
        {
            DateOnly dateObj = parseDateToDateOnly(dateStr);
            string parsedDate = dateObj.ToString("dd/MM/yyyy");
            //parsedDate = parsedDate.Replace("/","");
            parsedDate = parsedDate.Replace("-", "/");
            parsedDate = parsedDate.Replace(".", "/");
            parsedDate = parsedDate.Replace(" ", "/");
            //println("Stan: " + parsedDate);
            return parsedDate;
        }

        // Parses a string into a DateOnly object, then into a string again
        private string parseDateToString(string dateStr)
        {
            string parsedDate = "";
            try
            {
                parsedDate = DateOnly.Parse(dateStr).ToString();
                println("Parsed: " + parsedDate);
            }
            catch (FormatException fEx)
            {
                println("FormatException: " + fEx.Message);
                parsedDate = "";
            }
            return parsedDate;
        }

        // Parses a string into a DateOnly object
        private DateOnly parseDateToDateOnly(string dateStr)
        {
            DateOnly parsedDate = new DateOnly();
            try
            {
                parsedDate = DateOnly.Parse(dateStr);
                println("Parsed: " + parsedDate.ToString());
            }
            catch (FormatException fEx)
            {
                println("FormatException: " + fEx.Message);
            }
            return parsedDate;
        }

        //dd = 25
        //MM = 08 MMM = Aug MMMM = August 
        //yy = 24 yyyy = 2024
        // This function will attempt to interpret raw user input
        private void detectDateInputType(string userDateInput, out string dateString, out int formatNum)
        {
            userDateInput = string.Join("/", userDateInput.Split('-','.','_','|','+',' '));
            DateOnly dateObj = new DateOnly();
            CultureInfo culture = CultureInfo.InvariantCulture;

            bool found = false;
            formatNum = -1;
            dateString = "";

            string[] formats = {
                "dd/MM/yyyy",       // 0
                "dd/MM/yy",         // 1
                "d/MM/yyyy",        // 2    
                "d/MM/yy",          // 3    
                "dd/MMMM/yyyy",     // 4    
                "dd/MMMM/yy",       // 5    
                "MMM/yyyy",         // 6
                "MMM/yy",           // 7 
                "MMMM/yyyy",        // 8
                "MMMM/yy",          // 9
                "dd/MM",            // 10   dd = 25
                "dd/MMM",           // 11   MM = 08 MMM = Aug MMMM = August 
                "dd/MMMM",          // 12   yy = 24 yyyy = 2024
                "MMM",              // 13
                "MMMM",             // 14
                "yyyy"              // 15
            };

            //println("InputValue for detection: " + userDateInput);
            for(int i = 0; i < formats.Length; i++)
            {
                //println(" i value: " + i);
                if (DateOnly.TryParseExact(userDateInput, formats[i], out dateObj) && !found)
                {
                    formatNum = i;
                    dateString = dateObj.ToString("dd/MM/yyyy"); //standarze date string
                    found = true;
                    //println("Format Num interior: " + formatNum + " | found: " + found);
                }
                else if (i > 12 && i != 15 && !found)
                {
                    try
                    {
                        int month = DateOnly.ParseExact(userDateInput, formats[i], culture).Month;
                        dateObj = new DateOnly(0001, month, 01);
                        formatNum = i;
                        dateString = dateObj.ToString("dd/MM/yyyy"); //standarze date string
                        found = true;
                        //println("Format Num interior: " + formatNum + " | found: " + found);
                    }
                    catch (FormatException ex1)
                    {
                        //Ignore ex1
                        println(ex1.Message);
                    }
                }
                else if (i == 15 && !found)
                {
                    try
                    {
                        int year = DateOnly.ParseExact(userDateInput, formats[i], culture).Year;
                        dateObj = new DateOnly(year, 01, 01);
                        formatNum = i;
                        dateString = dateObj.ToString("dd/MM/yyyy"); //standarze date string
                        //println("15: " + dateString);
                        found = true;
                        //println("Format Num interior: " + formatNum + " | found: " + found);
                    }
                    catch (FormatException ex1)
                    {
                        //Ignore ex1
                        println(ex1.Message);
                    }
                }
            }
            //println("FINAL Format Num interior: " + formatNum + " | DateString: " + dateString + " | found: " + found);
        }

        // Returns the max between 2 values
        private int max(int n1, int n2)
        {
            if(n1 > n2)
            {
                return n1;
            }
            else
            {
                return n2;
            }
        }

        // Prints a List of PatientList Items
        private void printList(List<PatientListItem> list)
        {
            System.Diagnostics.Debug.Write("[");
            foreach (PatientListItem item in list)
            {
                System.Diagnostics.Debug.Write(item.PatientName + ",");
            }
            System.Diagnostics.Debug.Write("]");
            System.Diagnostics.Debug.WriteLine("");
        }

        // Prints an a given string array
        private void printArr(string[] strArr)
        {
            System.Diagnostics.Debug.Write("[");
            foreach (string str in strArr)
            {
                System.Diagnostics.Debug.Write(str + ",");
            }
            System.Diagnostics.Debug.Write("]");
            System.Diagnostics.Debug.WriteLine("");
        }

        // combines a string array into one string
        private string combineStr(string[] stringArray)
        {
            string output = "";
            foreach (string str in stringArray) 
            {
                output += str;
            }
            return output;
        }

        // combines a char array into one string
        private string combineStr(char[] charArray)
        {
            string output = "";
            foreach(char c in charArray)
            {
                output += c;
            }
            return output;
        }

        // ----- Testing Purposes for StrComparator ----- //

        public void dDIT_Testing(string useKey, string userDateInput, out string dateString, out int formatNum)
        {
            dateString = "";
            formatNum = -1;
            if (useKey.Equals("PFMS_Mi04_2024_T"))
            {
                detectDateInputType(userDateInput, out dateString, out formatNum);
            }
            else
            {
                perm_Violation("dDIT: Usage Key is Invalid!");
            }
        }

        public int dC_Testing(string useKey, string str1, string str2, string searchBy)
        {
            int sim = -1;
            if (useKey.Equals("PFMS_Mi04_2024_T"))
            {
                sim = dateComparer(str1, str2, searchBy);
            }
            else
            {
                perm_Violation("dC: Usage Key is Invalid!");
            }
            return sim;
        }

        public List<PatientListItem> eSBBM_Testing(string useKey, List<PatientProfile> profiles, ExpPatientSearch search)
        {
            List<PatientListItem> patList = null;
            if (useKey.Equals("PFMS_Mi04_2024_T"))
            {
                patList = sortByBestMatch(profiles, search);
            }
            else
            {
                perm_Violation("eSBBM: Usage Key is Invalid!");
            }
            return patList;
        }

        public List<PatientListItem> sPLI_Testing(string useKey, List<PatientListItem> patients, string keyword, string sortBy)
        {
            List<PatientListItem> patList = null;
            if (useKey.Equals("PFMS_Mi04_2024_T"))
            {
                patList = sortPatientListItem(patients, keyword, sortBy);
            }
            else
            {
                perm_Violation("sPLI: Usage Key is Invalid!");
            }
            return patList;
        }

        // Throws a permission violation exception when an unauthorised program attempts to call these testing functions
        private static void perm_Violation(string msg)
        {
            throw new MethodAccessException(msg);
        }
    }
}
