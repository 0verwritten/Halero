using Halero.Models.GameManagement;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace Halero.Services.GameManagement;

public class GameManager : IGameManager{
    private readonly IDatabaseManager<GameSessionCard> gameSessioner;
    private Dictionary<Guid, GameSessionData> activeSessions;
    private Dictionary<Guid, WebSocket> pendingSessions;

    public GameManager( MongoDBSessionManager sessionManager ){
        gameSessioner = new MongoDBManager<GameSessionCard>(sessionManager, "GameSessions");
        activeSessions = new Dictionary<Guid, GameSessionData>();
        pendingSessions = new Dictionary<Guid, WebSocket>();

        gameSessioner.DeleteMany( session => true ); // because sessions' details are saving localy in ram
    }

    public void UpdateSessionData(Guid sessionId, Guid userId, GameUpdate gameUpdate){
        if(activeSessions[sessionId].FirstUser.ID == userId){
            if(gameUpdate.direction is not null)
                activeSessions[sessionId].FirstUser.Direction = (SnakeDirection)gameUpdate.direction;
            if(gameUpdate.tail is not null)
                activeSessions[sessionId].FirstUser.Tail = gameUpdate.tail;
        }
        else if(activeSessions[sessionId].SecondUser.ID == userId){
            if(gameUpdate.direction is not null)
                activeSessions[sessionId].SecondUser.Direction = (SnakeDirection)gameUpdate.direction;
            if(gameUpdate.tail is not null)
                activeSessions[sessionId].SecondUser.Tail = gameUpdate.tail;
        }
        else
            throw new Exception("No such user in this session");
        if (gameUpdate.candies is not null)
            activeSessions[sessionId].candies = gameUpdate.candies;
    }

    public async Task<bool> ClearActiveSessionsAsync(){
        return await Task.Run(() =>{
            activeSessions.Clear();
            pendingSessions.Clear();
            gameSessioner.DeleteMany( session => true );
            return true;
        });
    }

    public void UpdateSessionSocket(Guid sessionId, Guid userId, WebSocket newSocker){
        if(activeSessions[sessionId].FirstUser.ID == userId)
            activeSessions[sessionId].FirstUser.Socket = newSocker;
        if(activeSessions[sessionId].SecondUser.ID == userId)
            activeSessions[sessionId].SecondUser.Socket = newSocker;
    }

    public GameSessionData GetSession(Guid sessionID) {
        if(gameSessioner.FindOne( card => card.ID == sessionID) is not null)
            return activeSessions[sessionID];
        else
            throw new KeyNotFoundException();
    }

    public GameSessionCard CreateSession(Guid UserId, WebSocket socket){
        GameSessionCard newSession = new GameSessionCard(
            new UserSessionCard(){ ID = UserId }
        );
        gameSessioner.InsertOne(newSession);
        pendingSessions.Add(newSession.ID, socket);
        return newSession;
    }
    public bool JoinSession(Guid UserId, ref GameSessionCard joinedGame, WebSocket socket){
        GameSessionCard? gameToJoin = gameSessioner.FindOne( session => session.FirstPlayer!.ID == UserId);
        if(gameToJoin is not null){
            joinedGame = gameToJoin;
            return true;
        }

        gameToJoin = gameSessioner.FindOne( session => session.SecondPlayer == null );
        if(gameToJoin is null)
            return false;


        gameToJoin.SecondPlayer = new UserSessionCard() { ID = UserId };
        gameSessioner.UpdateOne( session => session.ID == gameToJoin.ID, gameToJoin);
        joinedGame = gameToJoin;
        activeSessions.Add( joinedGame.ID, GameSessionData.FromGameSessionCard(gameToJoin, pendingSessions[gameToJoin.ID], socket) );
        return true;
    }

    public GameSessionCard? RejoinSession(Guid userId, WebSocket socket, Guid? lastGameSession){
        if(lastGameSession != null){
            try{
                return gameSessioner.FindOne( sesion => sesion.ID == lastGameSession );
            }
            catch(KeyNotFoundException) {}
        }
        GameSessionCard resulting = new GameSessionCard();
        if(!JoinSession(userId, ref resulting, socket))
            return null;

        return resulting;
    }
    
    public GameSessionCard QuickStart(Guid userId, WebSocket socket){
        GameSessionCard resulting = new GameSessionCard();
        if(!JoinSession(userId, ref resulting, socket))
            return CreateSession(userId, socket);

        return resulting;
    }

    public void EndSession(Guid sessionID){
        activeSessions.Remove(sessionID);
        gameSessioner.DeleteOne( card => card.ID == sessionID );
    }

    public GameSessionCard? GetCurrentSessionByUserID(Guid userId) => gameSessioner.FindOne( session => session.FirstPlayer!.ID == userId || session.SecondPlayer!.ID == userId );
    public GameSessionCard? GetCurrentSessionBySessionID(Guid sessionId) => gameSessioner.FindOne( session => session.ID == sessionId );
}