using MongoDB.Bson.Serialization.Attributes;

namespace Halero.Models.UserManagement;

public class UserProfile{
    [BsonId]
    public Guid ID { get; set; }
    public Guid RoleID { get; set; }
    public string UserName { get; set; }
    public string ProfileName { get; set; }
    public string Email { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsOnline { get; set; } = false;
    public bool IsVerified { get; set; } = false;
    public string? VerificationCode { get; set; } = null;
    public DateTime LastLogin { get; set; } = DateTime.Now;
    public UserProfile() {
        UserName = String.Empty;
        ProfileName = String.Empty;
        Email = String.Empty;
    }
}