namespace SmartCafe.Models
{
    public class UserProfileModel
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool Status { get; set; }
        public DateOnly? JoinDate { get; set; }
    }
}
