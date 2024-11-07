using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace PFMS_MI04.Models.Security
{
    public class SecLog
    {
        public string userID { get; private set; } // Logged UserID
        public string atPage { get; private set; } // Which page was the user in
        public string atFunc { get; private set; } // What is the calling func
        public string atFile { get; private set; } // What is the calling file
        public string atTime { get; private set; } // Records the time logged
        public string atDate { get; private set; } // Records the date logged
        public string logType { get; private set; } // AUTH, ACCESS, API, ERROR, CHANGE, SECURITY
        public string activityMsg { get; private set; } // Log short description
        public int blocker { get; private set; } // A flag preventing further modification

        public SecLog(string userID)
        {
            this.userID = userID;
            atPage = "Unset";
            atFunc = "Unset";
            atFile = "Unset";
            atTime = "Unset";
            atDate = "Unset";
            logType = "Unset";
            activityMsg = "Unset";
        }

        public SecLog(string userID, string atPage, string atFunc, string atFile, string atTime, string atDate, string logType, string activityMsg, int blocker)
        {
            if (blocker == 1234)
            {
                this.userID = userID;
                this.atPage = atPage;
                this.atFunc = atFunc;
                this.atFile = atFile;
                this.atTime = atTime;
                this.atDate = atDate;
                this.logType = logType;
                this.activityMsg = activityMsg;
                this.blocker = blocker;
            }
            else
            {
                throw new MethodAccessException("Not allowed to create a new instance of SecLog in this form!");
            }
        }

        // Can only be called once per object!
        public void setMsg(string activityMsg, string atPage, string atFunc, string atFile, string logType)
        {
            if (blocker != 1234)
            {
                atDate = DateOnly.FromDateTime(DateTime.Now).ToString("dd-MM-yyyy");
                atTime = TimeOnly.FromDateTime(DateTime.Now).ToString();
                this.atPage = atPage;
                this.logType = logType;
                this.activityMsg = activityMsg;
                this.atFunc = atFunc;
                this.atFile = atFile;
                blocker = 1234;
            }
            else
            {
                throw new MethodAccessException("SecLog.setMsg() can only be called once per instance");
            }
        }

        // If the SecLog is ready for upload
        public bool isReady()
        {
            bool returnVal = true;
            if (atPage.Equals("Unset") || atTime.Equals("Unset") || atDate.Equals("Unset") || atFunc.Equals("Unset") || atFile.Equals("Unset") || logType.Equals("Unset") || activityMsg.Equals("Unset"))
            {
                returnVal = false;
            }
            return returnVal;
        }

        public override string ToString()
        {
            return "|" + logType + "|" + userID + "|" + atDate + "|" + atTime + "|" + atPage + "|" + atFunc + "|" + atFile + "|\n" + "Msg:[" + activityMsg + "]";
        }
    }
}
