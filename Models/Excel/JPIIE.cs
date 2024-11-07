using Microsoft.AspNetCore.Mvc;

namespace PFMS_MI04.Models
{
    public class JPIIE
    {
        public string Name { get; set; }
        public string Ic { get; set; }
        public string TheirReference { get; set; }
        public string OurReference { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public List<TreatmentInfoJPIIE> Treatments { get; set; }
        public float TotalTreatment { get; set; }

        public JPIIE(string Name, string Ic, string TheirReference, string OurReference, string Month, string Year, float TotalTreatment)
        {
            this.Name = Name;
            this.Ic = Ic;
            this.TheirReference = TheirReference;
            this.OurReference = OurReference;
            this.Month = Month;
            this.Year = Year;
            this.Treatments = new List<TreatmentInfoJPIIE>();
            this.TotalTreatment = TotalTreatment;
        }

        public JPIIE()
        {
            this.Name = "";
            this.Ic = "";
            this.TheirReference = "";
            this.OurReference = "";
            this.Month = "";
            this.Year = "";
            this.Treatments = new List<TreatmentInfoJPIIE>();
            this.TotalTreatment = 0;
        }

    }

    public class TreatmentInfoJPIIE
    {
        public string TreatmentDate { get; set; }
        public float TreatmentCost { get; set; }

        public TreatmentInfoJPIIE(string treatmentDate, float treatmentCost)
        {
            TreatmentDate = treatmentDate;
            TreatmentCost = treatmentCost;
        }

        public TreatmentInfoJPIIE()
        {
            TreatmentDate = "";
            TreatmentCost = 0;
        }
    }
}
