using Microsoft.AspNetCore.Mvc;
using Halero.Services.UserManagement;
using Halero.Models.UserManagement;
using Halero.Models.UserManagement.Forms;
using System.Security.Cryptography;
using System.Text;

namespace Halero.Controllers;

[ApiController]
[Route("/api/[controller]/[action]")]
public class AuthenticationController: ControllerBase{
    public readonly ILogger<AuthenticationController> _logger;
    public readonly IUserManager _userManager;

    public AuthenticationController(ILogger<AuthenticationController> logger, IUserManager userManager){
        _logger = logger;
        _userManager = userManager;
    }

    [ActionName("Register")]
    [HttpPost]
    public AuthenticationForm Register([FromBody] RegisterForm registerForm){
        var result = _userManager.SignUpUser(new UserClaims{
            UserName = registerForm.userName,
            ProfileName = registerForm.userProfileName,
            Email = registerForm.email
        }, registerForm.password);
        

        return UMException<UserToken>.ToAuthenticationForm(result);
    }

    [ActionName("Login")]
    [HttpPost]
    public AuthenticationForm Login([FromBody] LoginForm loginForm ){
        var result = _userManager.LogInWithPassword( new UserClaims(){ UserName = loginForm.userName }, loginForm.password );

        return UMException<UserToken>.ToAuthenticationForm(result);
    }

    [ActionName("update")]
    [HttpPost]
    public async Task<AuthenticationForm> UpdateTokenAsync(){
        var token = await _userManager.UpdateTokenAsync(Request);
        return UMException<UserToken>.ToAuthenticationForm(token);
    }

    [ActionName("checkToken")]
    [HttpPost]
    public async Task<bool> isTokenValidAync(){
        return await _userManager.VerifyTokenAsync(Request);
    }

    private string CreateMD5Hash(string input)
    {
        MD5 hasher = MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = hasher.ComputeHash(inputBytes);
        
        StringBuilder buildingString = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            buildingString.Append(hashBytes[i].ToString("X2"));
        }
        return buildingString.ToString();
    }

    [ActionName("ProfileInfo")]
    [HttpPost]
    public async Task<UserProfileCard?> GetProfileInfoAsync(){
        var user = await _userManager.GetCurrentUserAsync(Request);
        if(user.IsSuccessful()){
            MD5 hasher = MD5.Create();
            var userCard = new UserProfileCard {
                profileName = user.Value.ProfileName,
                userName = user.Value.UserName,
                pictureUri = $"https://www.gravatar.com/avatar/{ CreateMD5Hash(user.Value.Email) }?d=identicon&s=500"
            };
            return userCard;
        }

        Response.StatusCode = 404;
        return null; 
    }
}