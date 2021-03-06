using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Halero.Models.UserManagement;
public enum AuthorityRate{
    Default = 0,
    Admin = 4
}

public class UserRole{
    [BsonId]
    public Guid ID { get; set; }
    public string RoleName { get; set; }
    public AuthorityRate AccessRate { get; set; }

    public UserRole(){
        RoleName = "player";
    }
}