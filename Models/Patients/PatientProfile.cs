namespace PFMS_MI04.Models.Patients
{
    public class PatientProfile
    {
        public string patientFullName {  get; set; }
        public string patientName {  get; set; }
        public string patientNRIC { get; set; }
        public string patientID { get; set; }
        public int patientAge { get; set; }
        public string patientGender { get; set; }
        public string patientEthnicity { get; set; }
        public string patientOccupation { get; set; }
        public string patientFinance { get; set; }
        public string patientPhoneNum { get; set; }
        public string patientDialSche { get; set; }
        public string dialCenterBranch { get; set; }
        public string aboGrouping { get; set; }
        public string rhesusFactor { get; set; }
        public string patientAttendance { get; set; }
        public string patientActivity { get; set; }
        public string patientProfilePic { get; set; }

        public PatientProfile()
        {
            patientFullName = "No Name";
            patientName = "No Name";
            patientNRIC = "0000000000000";
            patientID = "0";
            patientAge = 0;
            patientGender = "Male";
            patientEthnicity = "None";
            patientOccupation = "Unemployed";
            patientFinance = "None";
            patientPhoneNum = "016123456789";
            patientDialSche = "None";
            dialCenterBranch = "None";
            aboGrouping = "None";
            rhesusFactor = "None";
            patientActivity = "None";
            patientAttendance = "None";
            patientProfilePic = "None";
        }

        public PatientProfile(string patientFullName, string patientName, string patientNRIC, string patientID, int patientAge, string patientGender, string patientEthnicity, string patientOccupation, string patientFinance, string patientPhoneNum, string patientDialSche, string dialCenterBranch, string aboGrouping, string rhesusFactor, string patientAttendance, string patientActivity, string patientProfilePic)
        {
            this.patientFullName = patientFullName;
            this.patientName = patientName;
            this.patientNRIC = patientNRIC;
            this.patientID = patientID;
            this.patientAge = patientAge;
            this.patientGender = patientGender;
            this.patientEthnicity = patientEthnicity;
            this.patientOccupation = patientOccupation; 
            this.patientFinance = patientFinance;
            this.patientPhoneNum = patientPhoneNum;
            this.patientDialSche = patientDialSche;
            this.dialCenterBranch = dialCenterBranch;
            this.aboGrouping = aboGrouping;
            this.rhesusFactor = rhesusFactor;
            this.patientAttendance = patientAttendance;
            this.patientActivity = patientActivity;
            this.patientProfilePic = patientProfilePic;
        }

        public PatientProfile(PatientProfile copy)
        {
            patientFullName = copy.patientFullName;
            patientName = copy.patientName;
            patientNRIC = copy.patientNRIC;
            patientID = copy.patientID;
            patientAge = copy.patientAge;
            patientGender = copy.patientGender;
            patientEthnicity = copy.patientEthnicity;
            patientOccupation = copy.patientOccupation; 
            patientFinance = copy.patientFinance;
            patientPhoneNum = copy.patientPhoneNum;
            patientDialSche = copy.patientDialSche;
            dialCenterBranch = copy.dialCenterBranch;
            aboGrouping = copy.aboGrouping;
            rhesusFactor = copy.rhesusFactor;
            patientAttendance = copy.patientAttendance;
            patientActivity = copy.patientActivity;
            patientProfilePic = copy.patientProfilePic;
        }

        public PatientListItem toListItem()
        {
            return new PatientListItem(patientID, patientNRIC, patientFullName, patientPhoneNum, patientActivity, patientFinance);
        }
    }
}
