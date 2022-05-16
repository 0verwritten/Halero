namespace Halero.Models.UserManagement;

public class UserProfile{
    public Guid ID { get; set; }
    public UserRole Role { get; set; }
    public string UserName { get; set; }
    public string ProfileName { get; set; }
    public string Email { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsOnline { get; set; } = false;
    public DateTime LastLogin { get; set; } = DateTime.Now;
    public UserProfile() {}
}