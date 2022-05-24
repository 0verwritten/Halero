namespace Halero.Models.UserManagement;

public class UserToken{
    public string RefreshToken { get; set; } = String.Empty;
    public string AccessToken { get; set; } = String.Empty;
}