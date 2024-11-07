using System.Security.Cryptography.X509Certificates;

namespace PFMS_MI04.Models
{
    public class AddReminderItem
    {
        public string Name { get; set; }
        public string Identification { get; set; }
        public string Dependency { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }

        public AddReminderItem()
        {
            this.Name = "";
            this.Identification = "";
            this.Dependency = "";
            this.Phone = "";
            this.Email = "";
            this.UserType = "";
        }

        public AddReminderItem(string name, string identification, string dependency, string phone, string email, string userType)
        {
            this.Name = name;
            this.Identification = identification;
            this.Dependency = dependency;
            this.Phone = phone;
            this.Email = email;
            this.UserType = userType;
        }



    }
}
