using System.Runtime.CompilerServices;

namespace PFMS_MI04.Models.Patients
{
    public class PatientDoc
    {
        public int docID { get; set; }
        public string date { get; set; }
        public string docName { get; set; }
        public string docRef { get; set; }
        public int isPreviewable { get; set; }
        public string remarks { get; set; }
        public string patId { get; set; }
        public string? base64String { get; set; }

        public PatientDoc() 
        {
            docID = 0;
            date = "";
            docName = "Document 1234";
            docRef = "";
            isPreviewable = 0;
            remarks = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis ac dui vestibulum urna commodo molestie at sed urna. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. In porta malesuada lacus ac luctus.";
            patId = "0";
            base64String = "";
        }

        public PatientDoc(int docID, string date, string docName, string docRef, int isPreviewable, string remarks, string patId, string? base64String)
        {
            this.docID = docID;
            this.date = date;
            this.docName = docName;
            this.docRef = docRef;
            this.isPreviewable = isPreviewable;
            this.remarks = remarks;
            this.patId = patId;
            this.base64String = base64String;
        }

        public PatientDoc(PatientDoc copy)
        {
            docID = copy.docID;
            date = copy.date;
            docName = copy.docName;
            docRef = copy.docRef;
            isPreviewable = copy.isPreviewable;
            remarks = copy.remarks;
            patId = copy.patId;
            base64String = copy.base64String;
        }

        public void sanitizeRemarks()
        {
            remarks = remarks.Replace("&", "&amp;");
            remarks = remarks.Replace("<", "&lt;");
            remarks = remarks.Replace(">", "&gt;");
            remarks = remarks.Replace("\"", "&quot;");
            remarks = remarks.Replace("'", "&#039;");
        }
    }
}
