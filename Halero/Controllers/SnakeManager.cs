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
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
        bool gameStarted = false;
        bool isPlayerInitiator = true;
        Guid? gameID = null;
        GameSessionData? gameSession = null;

        while (!receiveResult.CloseStatus.HasValue)
        {
                                            // removing all the zeroes in the end
            var message = string.Join("", Encoding.UTF8.GetString( buffer ).TakeWhile((character) => character != 0));
            if(!string.IsNullOrEmpty(message)){
                var updateMessage =  JsonSerializer.Deserialize<GameUpdate>( message );
                if(updateMessage != null){
                    byte[] responceBytes = new byte[0];
                    if(!gameStarted){
                        var gameInfo = _gameManager.QuickStart( user.ID, webSocket );
                        if(gameInfo.SecondPlayer == null){
                            var responce = JsonSerializer.Serialize(new GameUpdate(){
                                ID = gameInfo.ID,
                                gameOver = false
                            });
                            responceBytes = Encoding.UTF8.GetBytes(responce);

                        }else{
                            gameSession = _gameManager.GetSession(gameInfo.ID);
                            var responce = JsonSerializer.Serialize<GameSessionData>( _gameManager.GetSession(gameInfo.ID) );
                            responceBytes = Encoding.UTF8.GetBytes(responce);
                            isPlayerInitiator = false;
                            

                            await gameSession.FirstUser.Socket.SendAsync(
                                new ArraySegment<byte>(responceBytes, 0, responceBytes.Length),
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
                    }else{
                        try{
                        if(gameSession == null)
                            gameSession = _gameManager.GetSession((Guid)gameID!);
                        }catch (KeyNotFoundException){
                            break;
                        }
                        if(updateMessage.direction is not null || updateMessage.candies is not null){
                            await gameSession!.GetUserSession(!isPlayerInitiator).Socket.SendAsync(
                                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);

                            buffer = Enumerable.Repeat<byte>(0, 10000).ToArray();
                            receiveResult = await webSocket.ReceiveAsync(
                                new ArraySegment<byte>(buffer), CancellationToken.None);

                            try{
                                _gameManager.UpdateSessionData(gameSession.ID, user.ID, updateMessage);
                            }catch (KeyNotFoundException){
                                break;
                            }
                            continue;
                        }
                    }
                }
            }
            
            buffer = Enumerable.Repeat<byte>(0, 10000).ToArray();
            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        _logger.LogWarning("session ended");
        _gameManager.EndSession((Guid)gameID!);
        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }    

}