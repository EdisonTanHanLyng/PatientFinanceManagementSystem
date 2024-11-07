using Microsoft.AspNetCore.Mvc;

namespace PFMS_MI04.Models
{
    public class SponsorPatient
    {
        public string Name { get; set; }
        public string Ic { get; set; }
        public int NumDialisis { get; set; }
        public decimal CostDialisis { get; set; }
        public int NumEPO { get; set; }
        public decimal CostEPO { get; set; }
    }
}
