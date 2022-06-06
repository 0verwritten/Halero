using System.Text.Json;
using System.Text.Json.Serialization;

namespace Halero.Models.UserManagement.TokenManagement;

public class TokenBody{
    public DateTime GenerationTime { get; set; }
    public Guid UserID { get; set; }
    public int lifeTimeSpan { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SuperSecret { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}