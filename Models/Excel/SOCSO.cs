using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace PFMS_MI04.Models
{
    public class SOCSO
    {
        public string Name { get; set; }
        public string Ic { get; set; }
        public string TheirReference { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string PaymentTo { get; set; }
        public List<TreatmentInfoSOCSO> Treatments { get; set; }


        public SOCSO(string Name, string Ic, string TheirReference, string Month, string Year, string PaymentTo)
        {
            this.Name = Name;
            this.Ic = Ic;
            this.TheirReference = TheirReference;
            this.Month = Month;
            this.Year = Year;
            this.PaymentTo = PaymentTo;
            this.Treatments = new List<TreatmentInfoSOCSO>();
        }

        public SOCSO()
        {
            this.Name = "";
            this.Ic = "";
            this.TheirReference = "";
            this.Month = "";
            this.Year = "";
            this.PaymentTo = "";
            this.Treatments = new List<TreatmentInfoSOCSO>();
        }

    }

    public class TreatmentInfoSOCSO
    {
        public string TreatmentDate { get; set; }
        public float DialysisCost { get; set; }
        public float MedicineCost { get; set; }
        public string Status { get; set; }

        public TreatmentInfoSOCSO(string treatmentDate, float dialysisCost, float medicineCost, string status)
        {
            TreatmentDate = treatmentDate;
            DialysisCost = dialysisCost;
            MedicineCost = medicineCost;
            Status = status;
        }

        public TreatmentInfoSOCSO()
        {
            TreatmentDate = "";
            DialysisCost = 0;
            MedicineCost = 0;
            Status = "Tiada";
        }
    }
}
