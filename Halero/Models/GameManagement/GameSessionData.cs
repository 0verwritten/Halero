using System.Net.WebSockets;

namespace Halero.Models.GameManagement;

/// <summary>This serves to get the full game info</summary>
public class GameSessionData{
    public Guid ID { get; set; }
    public UserSessionData FirstUser  { get; set; }
    public UserSessionData SecondUser { get; set; }

    public List<int[]> candies { get; set; } = new List<int[]>() { new int[] { new Random().Next(20,280), new Random().Next(100,200) } };


    public GameSessionData(Guid id, UserSessionData first, UserSessionData second){
        this.ID = id;
        this.FirstUser = first;
        this.SecondUser = second;
    }
    public static GameSessionData FromGameSessionCard(GameSessionCard card, WebSocket firstOne, WebSocket secondOne){
            return new GameSessionData( 
                card.ID, 
                new UserSessionData(
                    card.FirstPlayer!.ID, 
                    new List<int[]>(){ new int[]{ 150, 50 } },
                    firstOne),
                new UserSessionData( 
                    card.SecondPlayer!.ID, 
                    new List<int[]>() { new int[] { 150, 250 } },
                    secondOne,
                    SnakeDirection.left)
                );
    }

    public UserSessionData GetUserSession(bool isInitiator){
        if(!isInitiator)
            return SecondUser;
        return FirstUser;
    }
}