using System.Security.Cryptography.X509Certificates;

namespace PFMS_MI04.Models
{
    public class ReminderItem
    {
        public string? Id { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
        public string Priority { get; set; }
        public DateOnly DueDate { get; set; }
        public TimeOnly DueTime { get; set; }
        public string Description { get; set; }
        public string EmailContent { get; set; }

        public ReminderItem()
        {
            this.Id = "1";
            this.Title = "Title";
            this.UserName = "UserName";
            this.Email = "Email";
            this.UserType = "Type";
            this.Priority = "Priority";
            this.DueDate = new DateOnly();
            this.DueTime = new TimeOnly();
            this.Description = "Description";
            this.EmailContent = "Email Content";
        }

        public ReminderItem(string title, string userName, string email, string userType, string priority, DateOnly dueDate, TimeOnly dueTime, string description, string emailContent)
        {
            Title = title;
            UserName = userName;
            Email = email;
            UserType = userType;
            Priority = priority;
            DueDate = dueDate;
            DueTime = dueTime;
            Description = description;
            EmailContent = emailContent;
        }

        public ReminderItem(string id, string title, string userName, string email, string userType, string priority, DateOnly dueDate, TimeOnly dueTime, string description, string emailContent)
        {
            Id = id;
            Title = title;
            UserName = userName;
            Email = email;
            UserType = userType;
            Priority = priority;
            DueDate = dueDate;
            DueTime = dueTime;
            Description = description;
            EmailContent = emailContent;
        }

    }
}
