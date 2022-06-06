using MongoDB.Bson;

namespace Halero.Models.UserManagement;

public class UserSession{
    public Guid ID { get; set; } = Guid.NewGuid();
    public Guid UserID { get; set; } = Guid.NewGuid();
    public UserToken Token { get; set; } = new UserToken();
    public DateTime GenerationTime { get; set; } = DateTime.Now;
}