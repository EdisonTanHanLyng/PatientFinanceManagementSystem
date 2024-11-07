namespace PFMS_MI04.Models
{
    public class ManageAccountUser
    {
        public string? AccName { get; set; }
        public string? UserId { get; set; }
        public string? Contact { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; } // Added for online/offline status
    }
}
