namespace PFMS_MI04.Models
{
    public class Item
    {
        public string Description { get; set; }
        public int Qty { get; set; }
        public string UOM { get; set; }
        public decimal Price { get; set; }

        public Item (string description, int qty, string uOM, decimal price)
        {
            this.Description = description;
            this.Qty = qty;
            this.UOM = uOM;
            this.Price = price;
        }

        public Item()
        {
            this.Description = "";
            this.Qty = 0;
            this.UOM = "PER";
            this.Price = 0;
        }
    }
    public class SponsorListItem
    {
        //Types of sponsor info that is displayed and searched
        public long SponsorId { get; set; }
        public string SponsorName { get; set; }
        public string SponsorPhone { get; set; }
        public string ContactPerson { get; set; }
        public string SponsorCode { get; set; } //Elmer add
        public string SponsorAddress {  get; set; }//jason
        public string SponsorFax { get; set; }//jason
        public List<Item> Items { get; set; }

        public string toString()
        {
            return "Sponsor " + SponsorName + "ID" + SponsorId + "Code" + SponsorCode + "Contact" + ContactPerson + "Phone" + SponsorPhone;
        }


        public SponsorListItem()
        {
            SponsorId = 000000;
            SponsorName = "No Name";
            SponsorPhone = "016-123456789";
            ContactPerson = "No Name";
            SponsorFax = "0123456";
            SponsorAddress = "";
            Items = new List<Item>();
        }

        public SponsorListItem(int id, string name, string phone, string personName, string fax, string address)
        {
            SponsorId = id;
            SponsorName = name;
            SponsorPhone = phone;
            ContactPerson = personName;
            SponsorFax = fax;
            SponsorAddress = address;

            Items = new List<Item>();
        }
    }
}
