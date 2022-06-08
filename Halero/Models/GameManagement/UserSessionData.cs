using System.Net.WebSockets;
using System.IO;
using System.Runtime.Serialization;

namespace Halero.Models.GameManagement;

[Serializable]
public class UserSessionData{
    public Guid ID { get; set; }
    [NonSerialized]
    public WebSocket Socket;
    public SnakeDirection Direction { get; set; } = SnakeDirection.right;
    public List<int[]> Tail { get; set; }
    public UserSessionData( Guid id, List<int[]> tail, WebSocket socket, SnakeDirection direction = SnakeDirection.right){
        this.ID = id;
        this.Tail = tail;
        this.Direction = direction;
        this.Socket = socket;
    }
}