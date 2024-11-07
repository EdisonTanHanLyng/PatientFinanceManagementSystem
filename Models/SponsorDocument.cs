namespace PFMS_MI04.Models
{
    public class SponsorDocument
    {
        public int docID { get; set; }
        public string date { get; set; }
        public string docName { get; set; }
        public string docRef { get; set; }
        public int isPreviewable { get; set; }
        public string remarks { get; set; }
        public byte[] documentByteCode { get; set; }
        public string sponsorId { get; set; }
        public string? base64String { get; set; }

        public SponsorDocument()
        {
            docID = 0;
            date = "";
            docName = "Document 1234";
            docRef = "";
            isPreviewable = 0;
            remarks = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis ac dui vestibulum urna commodo molestie at sed urna. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. In porta malesuada lacus ac luctus.";
            documentByteCode = new byte[1];
            sponsorId = "";
            base64String = null;
        }

        public SponsorDocument(int docID, string date, string docName, string docRef, int isPreviewable, string remarks, byte[] documentByteCode, string sponsorId, string base64String)
        {
            this.docID = docID;
            this.date = date;
            this.docName = docName;
            this.docRef = docRef;
            this.isPreviewable = isPreviewable;
            this.remarks = remarks;
            this.documentByteCode = documentByteCode;
            this.sponsorId = sponsorId;
            this.base64String = base64String;
        }

        public SponsorDocument(SponsorDocument copy)
        {
            docID = copy.docID;
            date = copy.date;
            docName = copy.docName;
            docRef = copy.docRef;
            isPreviewable = copy.isPreviewable;
            remarks = copy.remarks;
            documentByteCode = copy.documentByteCode;
            sponsorId = copy.sponsorId;
            base64String = copy.base64String;
        }

        public SponsorDocument(SponsorDoc copy, byte[] documentByteCode)
        {
            docID = copy.docID;
            date = copy.date;
            docName = copy.docName;
            docRef = copy.docRef;
            isPreviewable = copy.isPreviewable;
            remarks = copy.remarks;
            sponsorId = copy.sponID;
            base64String = copy.base64String;
            this.documentByteCode = documentByteCode;
        }

        public SponsorDoc toPatientDoc()
        {
            return new SponsorDoc(docID, date, docName, docRef, isPreviewable, remarks, sponsorId, base64String);
        }
    }
}
