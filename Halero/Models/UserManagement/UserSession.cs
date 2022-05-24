namespace Halero.Models.UserManagement;

public class UserSession{
    public Guid ID { get; set; } = new Guid("");
    public Guid UserID { get; set; } = new Guid("");
    public UserToken Token { get; set; } = new UserToken();
    public DateTime GenerationTime { get; set; } = DateTime.Now;
}