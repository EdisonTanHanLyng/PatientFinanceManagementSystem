using Microsoft.AspNetCore.Mvc;

namespace PFMS_MI04.Models
{
    public class PricingModel
    {
        public string SponsorCode { get; set; }
        public float DialysisCost { get; set; }
        public float EPOCost { get; set; }
        public PricingModel()
        {
            this.SponsorCode = "";
            this.DialysisCost = 0;
            this.EPOCost = 0;
        }
        public PricingModel(string sponsorCode, float dialysisCost, float epoCost)
        {
            this.SponsorCode = sponsorCode;
            this.DialysisCost = dialysisCost;
            this.EPOCost = epoCost;
        }
    }
}
