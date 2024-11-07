using Microsoft.AspNetCore.Mvc;

namespace PFMS_MI04.Models
{
    public class IPPKKL
    {
        public string Name { get; set; }
        public string Ic { get; set; }
        public string TheirReference { get; set; }
        public string OurReference { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }

        public List<TreatmentInfoIPPKKL> Treatments { get; set; }
        public float TotalTreatment { get; set; }
        public float TotalMedicine { get; set; }

        public IPPKKL(string Name, string Ic, string TheirReference, string OurReference, string Month, string Year, float TotalTreatment, float TotalMedicine)
        {
            this.Name = Name;
            this.Ic = Ic;
            this.TheirReference = TheirReference;
            this.OurReference = OurReference;
            this.Month = Month;
            this.Year = Year;
            this.Treatments = new List<TreatmentInfoIPPKKL>();
            this.TotalTreatment = TotalTreatment;
            this.TotalMedicine = TotalMedicine;
        }

        public IPPKKL()
        {
            this.Name = "";
            this.Ic = "";
            this.TheirReference = "";
            this.OurReference = "";
            this.Month = "";
            this.Year = "";
            this.Treatments = new List<TreatmentInfoIPPKKL>();
            this.TotalTreatment = 0;
            this.TotalMedicine = 0;
        }

    }

    public class TreatmentInfoIPPKKL
    {
        public string TreatmentDate { get; set; }
        public float TreatmentCost { get; set; }
        public float MedicineCost { get; set; }

        public TreatmentInfoIPPKKL(string treatmentDate, float treatmentCost, float medicineCost)
        {
            TreatmentDate = treatmentDate;
            TreatmentCost = treatmentCost;
            MedicineCost = medicineCost;
        }

        public TreatmentInfoIPPKKL()
        {
            TreatmentDate = "";
            TreatmentCost = 0;
            MedicineCost = 0;
        }
    }
}
