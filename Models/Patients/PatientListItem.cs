namespace PFMS_MI04.Models.Patients
{
    public class PatientListItem
    {
        public string PatientId { get; set; }
        public string PatientNRIC { get; set; }
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public string Status { get; set; }
        public string Sponsor { get; set; }


        public PatientListItem()
        {
            PatientId = "000000000000";
            PatientNRIC = "000000000000";
            PatientName = "No Name";
            PatientPhone = "016-123456789";
            Status = "Inactive";
            Sponsor = "None";
        }

        public PatientListItem(string id, string nric, string name, string phone, string state, string spons)
        {
            PatientId = id;
            PatientNRIC = nric;
            PatientName = name;
            PatientPhone = phone;
            Status = state;
            Sponsor = spons;
        }

        public PatientListItem(PatientListItem copy)
        {
            PatientId = copy.PatientId;
            PatientNRIC = copy.PatientNRIC;
            PatientName = copy.PatientName;
            PatientPhone = copy.PatientPhone;
            Status = copy.Status;
            Sponsor = copy.Sponsor;
        }
    }
}
