using System.Net.WebSockets;
using Halero.Models.GameManagement;

namespace Halero.Services.GameManagement;

public interface IGameManager{
    public GameSessionCard CreateSession(Guid UserId, WebSocket socket);
    public bool JoinSession(Guid UserId, ref GameSessionCard joinedGame, WebSocket socket);
    
    /// <summary>Joins or creates a session depending on the sessions avaliablilty</summary>
    public GameSessionCard QuickStart(Guid userId, WebSocket socket, string? gameSessionFromCookie);
    public void EndSession(Guid sessionID);

    public void UpdateSessionData(Guid sessionId, Guid userId, GameUpdate gameUpdate);
    public void UpdateSessionSocket(Guid sessionId, Guid userId, WebSocket newSocker);
    public GameSessionData GetSession(Guid sessionID);

    public GameSessionCard? GetCurrentSessionByUserID(Guid userId);
    public GameSessionCard? GetCurrentSessionBySessionID(Guid sessionId);
}