namespace PFMS_MI04.Models.Patients
{
    public class PatientHDRecord
    {
        public string patientID { get; set; }
        public string patientNRIC { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public int tdMin { get; set; }
        public double weightPre { get; set; }
        public double weightPost { get; set; }
        public double dw { get; set; }
        public double idw { get; set; }
        public double dwPercent { get; set; }
        public double ufGoal { get; set; }
        public int preBP_SBP { get; set; }
        public int preBP_DBP { get; set; }
        public int postBP_SBP { get; set; }
        public int postBP_DBP { get; set; }
        public int prePulse { get; set; }
        public int postPulse { get; set; }
        public int bfr { get; set; }
        public string epoType { get; set; }
        public double epoDosage { get; set; }
        public int epoQty { get; set; }
        public double ocm_KtV { get; set; }
        public string dial_Cal { get; set; }
        public string dialyzer { get; set; }
        public string remarks { get; set; }

        public PatientHDRecord()
        {
            patientID = "";
            patientNRIC = "";
            date = DateOnly.FromDateTime(DateTime.Now).ToString();
            time = TimeOnly.FromDateTime(DateTime.Now).ToString();
            tdMin = 0;
            weightPre = 0.0;
            weightPost = 0.0;
            dw = 0.0;
            idw = 0.0;
            dwPercent = 0.0;
            ufGoal = 0.0;
            preBP_SBP = 0;
            preBP_DBP = 0;
            postBP_SBP = 0;
            postBP_DBP = 0;
            prePulse = 0;
            postPulse = 0;
            bfr = 0;
            epoType = "";
            epoDosage = 0.0;
            epoQty = 0;
            ocm_KtV = 0.0;
            dial_Cal = "";
            dialyzer = "";
            remarks = "";
        }

        public string toString()
        {
            string str = "" +
                "PatientID: " + patientID + "\n" +
                "PatientNRIC: " + patientNRIC + "\n" +
                "Date: " + date + "\n" +
                "Time: " + time + "\n" + 
                "Td (min): " + tdMin + "\n" +
                "Weight (pre): " + weightPre + "\n" +
                "Weight (post): " + weightPost + "\n" +
                "DW: " + dw + "\n" +
                "IDW: " + idw + "\n" +
                "%DW: " + dwPercent + "\n" +
                "UF Goal: " + ufGoal + "\n" +
                "PreBP (SBP): " + preBP_SBP + "\n" +
                "PreBP (DBP): " + preBP_DBP + "\n" +
                "PostBP (SBP): " + postBP_SBP + "\n" +
                "PostBP (DBP): " + postBP_DBP + "\n" +
                "Pulse (Pre): " + prePulse + "\n" +
                "Pulse (Post): " + postPulse + "\n" +
                "BFR: " + bfr + "\n" + 
                "EPO Type: " + epoType + "\n" +
                "EPO Dosage: " + epoDosage + "\n" +
                "EPO Qty: " + epoQty + "\n" +
                "OCM Kt/V: " + ocm_KtV + "\n" +
                "Calcium: " + dial_Cal + "\n" +
                "Dialyzer: " + dialyzer + "\n" +
                "Remarks: " + remarks + "\n";

            return str;
        }
    }
}
