using Microsoft.AspNetCore.Mvc;
namespace PFMS_MI04.Models
{
    public class STEC
    {
        public string Name { get; set; }
        public string Ic { get; set; }
        public string Address { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public List<ItemsSTEC> Items { get; set; }
        public float TotalCost { get; set; }
        public STEC(string Name, string Ic, string Address, string Day, string Month, string Year, float totalCost)
        {
            this.Name = Name;
            this.Ic = Ic;
            this.Address = Address;
            this.Day = Day;
            this.Month = Month;
            this.Year = Year;
            this.Items = new List<ItemsSTEC>();
            this.TotalCost = totalCost;
        }
        public STEC()
        {
            this.Name = "";
            this.Ic = "";
            this.Address = "";
            this.Day = "";
            this.Month = "";
            this.Year = "";
            this.Items = new List<ItemsSTEC>();
            this.TotalCost = 0;
        }
    }
    public class ItemsSTEC
    {
        public string Particular { get; set; }
        public float Cost { get; set; }
        public ItemsSTEC(string particular, float cost)
        {
            Particular = particular;
            Cost = cost;
        }
        public ItemsSTEC()
        {
            Particular = "";
            Cost = 0;
        }
    }
}
