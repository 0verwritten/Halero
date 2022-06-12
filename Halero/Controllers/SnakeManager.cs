using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Halero.Services.UserManagement;
using Halero.Models.UserManagement;
using Microsoft.Extensions.Primitives;
using Halero.Models.UserManagement.Forms;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Halero.Models.GameManagement;
using Halero.Services.GameManagement;

namespace Halero.Controllers;

[ApiController]
[Route("/api/[controller]/[action]")]
public class SnakeManagerController: ControllerBase
{
    public readonly ILogger<SnakeManagerController> _logger;
    public readonly IUserManager _userManager;
    public readonly IGameManager _gameManager;

    public SnakeManagerController(ILogger<SnakeManagerController> logger, IUserManager userManager, IGameManager gameManager){
        _logger = logger;
        _userManager = userManager;
        _gameManager = gameManager;
    }

    [ActionName("clearGames")]
    [HttpGet]
    public async Task<bool> ClearActiveGamesAsync() => await _gameManager.ClearActiveSessionsAsync();

    [ActionName("getUpdate")]
    [HttpGet]
    public async Task WebSocketReciever()
    {
        

        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var userSearch = await _userManager.GetCurrentUserAsync(Request);
            if(userSearch.IsSuccessful()){
                var user = userSearch.Value;
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await SocketHandler(webSocket, user);
            }else{
                Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }

    private async Task SocketHandler(WebSocket webSocket, UserProfile user)
    {
        const int BUFSIZE = 1024 * 4;
        var buffer = new byte[BUFSIZE];
        WebSocketReceiveResult receiveResult;
        bool gameStarted = false;
        bool isPlayerInitiator = true;
        Guid? gameID = null;
        GameSessionData? gameSession = null;

        do
        {
            buffer = Enumerable.Repeat<byte>(0, 10000).ToArray();
            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

                                            // removing all the zeroes in the end
            var message = string.Join("", Encoding.UTF8.GetString( buffer ).TakeWhile((character) => character != 0));
            if(string.IsNullOrEmpty(message))
                continue;
                
            _logger.LogInformation(message);
            var updateMessage =  JsonSerializer.Deserialize<GameUpdate>( message );

            if( updateMessage is  null )
                continue;
        
            byte[] responceBytes = new byte[0];

            if( !gameStarted ){
                GameSessionCard? gameInfo = null;
                
                if(updateMessage.ID != null)
                    gameInfo = _gameManager.RejoinSession(user.ID, webSocket, updateMessage.ID );
                    _logger.LogInformation($"rejoined Session initiation: { JsonSerializer.Serialize( gameInfo ) } -- {updateMessage.ID}");
                if(gameInfo == null){
                    if(updateMessage.ID != null)
                        continue;
                    gameInfo = _gameManager.QuickStart( user.ID, webSocket );
                    _logger.LogInformation($"new Session initiation: { JsonSerializer.Serialize( gameInfo ) }");
                }
                

                if(gameInfo.SecondPlayer == null){ // Created new session
                    responceBytes = Encoding.UTF8.GetBytes(
                                        JsonSerializer.Serialize(
                                            new GameUpdate(){
                                                ID = gameInfo.ID,
                                                gameOver = false
                                            }
                                        )
                                    );
                }
                else{
                    gameSession = _gameManager.GetSession(gameInfo.ID);

                    responceBytes = Encoding.UTF8.GetBytes(
                                        JsonSerializer.Serialize<GameSessionData>( 
                                            gameSession 
                                        )
                                    );

                    isPlayerInitiator = gameSession.FirstUser.ID == user.ID;

                    _gameManager.UpdateSessionSocket(gameInfo.ID, user.ID, webSocket);

                    var reloadMessage = updateMessage.ID == null ? null : Encoding.UTF8.GetBytes(
                                            JsonSerializer.Serialize<GameUpdate>( new GameUpdate{ 
                                                ID = gameSession.ID, 
                                                gamePause = false 
                                            })
                                        );

                    await gameSession.GetUserSession(!isPlayerInitiator).Socket!.SendAsync(
                        new ArraySegment<byte>(
                            updateMessage.ID == null ? responceBytes : reloadMessage!, 
                            0, 
                            updateMessage.ID == null ? responceBytes.Length : reloadMessage!.Length
                        ),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
                gameID = gameInfo.ID;
                gameStarted = true;

                await webSocket.SendAsync(
                            new ArraySegment<byte>(responceBytes, 0, responceBytes.Length),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
            }
            else{
                _logger.LogInformation(message);
                try{
                if(gameSession == null)
                    gameSession = _gameManager.GetSession((Guid)gameID!);
                }catch (KeyNotFoundException){
                    break;
                }
                if(updateMessage.direction is not null || updateMessage.candies is not null){
                    if(gameSession!.GetUserSession(!isPlayerInitiator).ID == user.ID)
                        isPlayerInitiator = !isPlayerInitiator;

                    await gameSession!.GetUserSession(!isPlayerInitiator).Socket!.SendAsync(
                        new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);

                    try{
                        _gameManager.UpdateSessionData(gameSession.ID, user.ID, updateMessage);
                    }catch (KeyNotFoundException){
                        break;
                    }
                    continue;
                }
            }

        }while (!receiveResult.CloseStatus.HasValue);

        _logger.LogWarning("session ended");
        try{
            var session = _gameManager.GetSession((Guid)gameID!);
            var pauseRequest = new GameUpdate(){
                ID = gameID,
                gamePause = true
            };

            await session.GetUserSession(!isPlayerInitiator).Socket!.SendAsync(
                new ArraySegment<byte>( 
                    Encoding.UTF8.GetBytes(
                        JsonSerializer.Serialize<GameUpdate>( pauseRequest )
                    ) 
                ),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            session.GetUserSession(isPlayerInitiator).Socket = null;
        }catch(KeyNotFoundException){
            _gameManager.EndSession((Guid)gameID!);
            await webSocket.CloseAsync(
                receiveResult.CloseStatus!.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
    }    

}