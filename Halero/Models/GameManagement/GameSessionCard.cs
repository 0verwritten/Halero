using MongoDB.Bson.Serialization.Attributes;

namespace Halero.Models.GameManagement;

public class GameSessionCard {
    [BsonId]
    public Guid ID { get; set; } = Guid.NewGuid();
    public UserSessionCard? FirstPlayer { get; set; }
    public UserSessionCard? SecondPlayer { get; set; }   

    public GameSessionCard() {}
    public GameSessionCard(UserSessionCard firstPlayer){
        FirstPlayer = firstPlayer;
    }

    public static GameSessionCard FromGameSessionGame(GameSessionData session){
        GameSessionCard sessionVisit = new GameSessionCard();
        sessionVisit.FirstPlayer = new UserSessionCard(){
            ID = session.FirstUser.ID
        };
        sessionVisit.SecondPlayer = new UserSessionCard(){
            ID = session.SecondUser.ID
        };
        sessionVisit.ID = session.ID;
        return sessionVisit;
    }
}