using Microsoft.AspNetCore.Mvc;

namespace PFMS_MI04.Models
{
    public class MainMenuModel_Reminder
    {
        public string DueDate { get; set; }

        public string Name { get; set; }

        public string UserType {  get; set; }

        public string Description {  get; set; }

        public string reminderMessageReturn() 
        {
            return $"{DueDate} , {Name} , {UserType} , {Description}";
        }


    }
}
