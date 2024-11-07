using System.IO;

namespace PFMS_MI04.Models
{
    public class SponsorDoc
    {
        public int docID { get; set; }
        public string date { get; set; }
        public string docName { get; set; }
        public string docRef { get; set; }
        public int isPreviewable { get; set; }
        public string remarks { get; set; }
        public string sponID { get; set; }
        public string? base64String { get; set; }

        public SponsorDoc()
        {
            docID = 0;
            date = "";
            docName = "Document 1234";
            docRef = "";
            isPreviewable = 0;
            remarks = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis ac dui vestibulum urna commodo molestie at sed urna. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. In porta malesuada lacus ac luctus.";
            sponID = "0";
            base64String = "";
        }

        public SponsorDoc(int docID, string date, string docName, string docRef, int isPreviewable, string remarks, string sponID, string? base64String)
        {
            this.docID = docID;
            this.date = date;
            this.docName = docName;
            this.docRef = docRef;
            this.isPreviewable = isPreviewable;
            this.remarks = remarks;
            this.sponID = sponID;
            this.base64String = base64String;
        }

        public SponsorDoc(SponsorDoc copy)
        {
            docID = copy.docID;
            date = copy.date;
            docName = copy.docName;
            docRef = copy.docRef;
            isPreviewable = copy.isPreviewable;
            remarks = copy.remarks;
            sponID = copy.sponID;
            base64String = copy.base64String;
        }
    }
}
