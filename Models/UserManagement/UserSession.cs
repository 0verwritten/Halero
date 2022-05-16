namespace Halero.Models.UserManagement;

public class UserSession{
    public Guid ID { get; set; }
    public UserProfile User { get; set; }
    public UserToken Token { get; set; }
    public DateTime GenerationTime { get; set; }
    public UserSession(){}
}