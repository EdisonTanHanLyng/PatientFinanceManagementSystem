namespace PFMS_MI04.Models.Patients
{
    public class PatientDocument
    {
        public int docID { get; set; }
        public string date { get; set; }
        public string docName { get; set; }
        public string docRef { get; set; }
        public int isPreviewable { get; set; }
        public string remarks { get; set; }
        public byte[] documentByteCode { get; set; }
        public string patientID { get; set; }
        public string? base64String { get; set; }

        public PatientDocument()
        {
            docID = 0;
            date = "";
            docName = "Document 1234";
            docRef = "";
            isPreviewable = 0;
            remarks = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis ac dui vestibulum urna commodo molestie at sed urna. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. In porta malesuada lacus ac luctus.";
            documentByteCode = new byte[1];
            patientID = "";
            base64String = null;
        }

        public PatientDocument(int docID, string date, string docName, string docRef, int isPreviewable, string remarks, byte[] documentByteCode, string patientID, string base64String)
        {
            this.docID = docID;
            this.date = date;
            this.docName = docName;
            this.docRef = docRef;
            this.isPreviewable = isPreviewable;
            this.remarks = remarks;
            this.documentByteCode = documentByteCode;
            this.patientID = patientID;
            this.base64String = base64String;
        }

        public PatientDocument(PatientDocument copy)
        {
            docID = copy.docID;
            date = copy.date;
            docName = copy.docName;
            docRef = copy.docRef;
            isPreviewable = copy.isPreviewable;
            remarks = copy.remarks;
            documentByteCode = copy.documentByteCode;
            patientID = copy.patientID;
            base64String = copy.base64String;
        }

        public PatientDocument(PatientDoc copy, byte[] documentByteCode)
        {
            docID = copy.docID;
            date = copy.date;
            docName = copy.docName;
            docRef = copy.docRef;
            isPreviewable = copy.isPreviewable;
            remarks = copy.remarks;
            patientID = copy.patId;
            base64String = copy.base64String;
            this.documentByteCode = documentByteCode;
        }

        public PatientDoc toPatientDoc()
        {
            return new PatientDoc(docID, date, docName, docRef, isPreviewable, remarks, patientID, base64String);
        }
    }
}
