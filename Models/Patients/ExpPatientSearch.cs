
namespace PFMS_MI04.Models.Patients
{
    public class ExpPatientSearch
    {
        public string searchName {  get; set; } // lcs
        public string dialScheduling {  get; set; }
        public string searchStatus { get; set; }
        public string searchRace { get; set; }// lcs
        public string searchTel { get; set; }// lcs
        public string searchId { get; set; }// lcs
        public string searchSpons { get; set; }

        public ExpPatientSearch()
        {
            searchName = "";
            dialScheduling = "";
            searchStatus = "";
            searchRace = "";
            searchTel = "";
            searchId = "";
            searchSpons = "";
        }

        public ExpPatientSearch(string searchName, string dialScheduling, string searchStatus, string searchRace, string searchTel, string searchId, string searchSpons)
        {
            this.searchName = searchName;
            this.dialScheduling = dialScheduling;
            this.searchStatus = searchStatus;
            this.searchRace = searchRace;
            this.searchTel = searchTel;
            this.searchId = searchId;
            this.searchSpons = searchSpons;
        }

        public ExpPatientSearch(ExpPatientSearch copy)
        {
            searchName = copy.searchName;
            dialScheduling = copy.dialScheduling;
            searchStatus = copy.searchStatus;
            searchRace = copy.searchRace;
            searchTel = copy.searchTel;
            searchId = copy.searchId;
            searchSpons = copy.searchSpons;
        }

        // --- Class Functions --- //

        // Returns the non-empty fields (array of 1 and 0) within the object
        public int[] getAvailableFields()
        {
            int[] ret = {0,0,0,0,0,0,0};
            if(searchName != string.Empty)
            {
                ret[0] = 1;
            }
            if(dialScheduling != string.Empty)
            {
                ret[1] = 1;
            }
            if(searchStatus != string.Empty)
            {
                ret[2] = 1;
            }
            if(searchRace != string.Empty)
            {
                ret[3] = 1;
            }
            if(searchTel != string.Empty)
            {
                ret[4] = 1;
            }
            if(searchId != string.Empty)
            {
                ret[5] = 1;
            }
            if(searchSpons != string.Empty)
            {
                ret[6] = 1;
            }
            return ret; ;
        }

        // Returns the field depending on the assigned field number
        public string getByFieldNum(int fieldNum)
        {
            string ret = "";
            switch (fieldNum)
            {
                case 1:
                    ret = searchName;
                    break;
                case 2:
                    ret = dialScheduling;
                    break;
                case 3:
                    ret = searchStatus;
                    break;
                case 4:
                    ret = searchRace;
                    break;
                case 5:
                    ret = searchTel;
                    break;
                case 6:
                    ret = searchId;
                    break;
                case 7:
                    ret = searchSpons;
                    break;
                default:
                    ret = "Invalid";
                    break;
            }
            return ret;
        }

        // Returns an information string
        public string ToString(int type)
        {
            string ret = "";
            if (type == 1)
            {
                ret += "searchName: ";
                ret += searchName == string.Empty ? "EMPTY \n" : searchName + "\n";
                ret += "dialScheduling: ";
                ret += dialScheduling == string.Empty ? "EMPTY \n" : dialScheduling + "\n";
                ret += "searchStatus: ";
                ret += searchStatus == string.Empty ? "EMPTY \n" : searchStatus + "\n";
                ret += "searchRace: ";
                ret += searchRace == string.Empty ? "EMPTY \n" : searchRace + "\n";
                ret += "searchTel: ";
                ret += searchTel == string.Empty ? "EMPTY \n" : searchTel + "\n";
                ret += "searchId: ";
                ret += searchId == string.Empty ? "EMPTY \n" : searchId + "\n";
                ret += "searchSpons: ";
                ret += searchSpons == string.Empty ? "EMPTY \n" : searchSpons + "\n";
            }
            else
            {
                ret = "" + searchName + ", " + dialScheduling + ", " + searchStatus + ", " + searchRace + ", " + searchTel + ", " + searchId + ", " + searchSpons;
            }
            return ret;
        }

        public override string ToString()
        {
            return ("" + searchName + ", " + dialScheduling + ", " + searchStatus + ", " + searchRace + ", " + searchTel + ", " + searchId + ", " + searchSpons);
        }
    }
}
