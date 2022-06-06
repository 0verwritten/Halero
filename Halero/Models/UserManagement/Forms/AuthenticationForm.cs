namespace Halero.Models.UserManagement.Forms;

public class AuthenticationForm{
    public UserToken? Token { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}