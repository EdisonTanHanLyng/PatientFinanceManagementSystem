namespace PFMS_MI04.Models
{
    public class ManageUserAccountItemList
    {

        public class ManageUser
        {
            public string Name { get; set; } 
            public string UserId { get; set; }
            public string Contact { get; set; }
            public string Role { get; set; }
            public string Gender { get; set; }
            public string Race { get; set; }
            public string Address { get; set; }
            public string Description { get; set; }
            public string Username { get; set; } 
            public string? Password { get; set; } 
        }
    }
}
