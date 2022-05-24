namespace Halero.Models.UserManagement;

public enum AuthorityRate{
    Default = 0,
    Admin = 4
}

public class UserRole{
    public Guid ID { get; set; }
    public string RoleName { get; set; }
    public AuthorityRate AccessRate { get; set; }
}