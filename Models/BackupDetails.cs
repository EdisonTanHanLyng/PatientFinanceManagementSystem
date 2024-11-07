namespace PFMS_MI04.Models
{
    public class BackupDetails
    {
        public DateTime date { get; set; }
        public string type { get; set; }
        public string remarks { get; set; }

        public BackupDetails(DateTime date, string type, string remarks)
        {
            this.date = date;
            this.type = type;
            this.remarks = remarks;
        }


        override
        public string ToString()
        {
            string formattedDate = date.ToString("dd MMMM, yyyy - HH:mm");
            return formattedDate + ": " + type + ": " + remarks;
        }
    }
}
