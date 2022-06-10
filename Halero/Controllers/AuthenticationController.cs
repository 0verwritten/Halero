using Microsoft.AspNetCore.Mvc;
using Halero.Services.UserManagement;
using Halero.Models.UserManagement;
using Halero.Models.UserManagement.Forms;
using System.Security.Cryptography;
using System.Text;
using Halero.Services;

namespace Halero.Controllers;

[ApiController]
[Route("/api/[controller]/[action]")]
public class AuthenticationController: ControllerBase{
    public readonly ILogger<AuthenticationController> _logger;
    public readonly IUserManager _userManager;
    public readonly MD5PasswordHasher _emailHasher;

    public AuthenticationController(ILogger<AuthenticationController> logger, IUserManager userManager){
        _logger = logger;
        _userManager = userManager;
        _emailHasher = new MD5PasswordHasher();
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

    [ActionName("ProfileInfo")]
    [HttpPost]
    public async Task<UserProfileCard?> GetProfileInfoAsync(){
        UMException<UserProfile>? user;
        try{
            user = await _userManager.GetCurrentUserAsync(Request);
            if(user.IsSuccessful()){
                MD5 hasher = MD5.Create();
                var userCard = new UserProfileCard (
                    user.Value.ProfileName,
                    user.Value.UserName,
                    $"https://www.gravatar.com/avatar/{ _emailHasher.GetHash(user.Value.Email) }?d=identicon&s=500"
                );
                return userCard;
            }
        } 
        catch ( Exception ){ }
            Response.StatusCode = 404;
            return null;
    }
}