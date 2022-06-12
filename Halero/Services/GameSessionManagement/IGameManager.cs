using System.Net.WebSockets;
using Halero.Models.GameManagement;

namespace Halero.Services.GameManagement;

public interface IGameManager{
    public GameSessionCard CreateSession(Guid UserId, WebSocket socket);
    public bool JoinSession(Guid UserId, ref GameSessionCard joinedGame, WebSocket socket);
    public GameSessionCard? RejoinSession(Guid userId, WebSocket socket, Guid? lastGameSession);
    /// <summary>Joins or creates a session depending on the sessions avaliablilty</summary>
    public GameSessionCard QuickStart(Guid userId, WebSocket socket);
    public void EndSession(Guid sessionID);

    public void UpdateSessionData(Guid sessionId, Guid userId, GameUpdate gameUpdate);
    public void UpdateSessionSocket(Guid sessionId, Guid userId, WebSocket newSocker);
    public GameSessionData GetSession(Guid sessionID);

    public GameSessionCard? GetCurrentSessionByUserID(Guid userId);
    public GameSessionCard? GetCurrentSessionBySessionID(Guid sessionId);

    // For DEBUG only
    public Task<bool> ClearActiveSessionsAsync();
}