using Halero.Models.UserManagement;
using Microsoft.AspNetCore.Http;
using Halero.Models;
using System.Web;
using System.Linq.Expressions;
using MongoDB.Bson;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

namespace Halero.Services.UserManagement;


class UserManager : IUserManager{
    private readonly IDatabaseManager<UserProfile> usersProfiles;
    private readonly IDatabaseManager<UserSession> usersSessions;
    private readonly IDatabaseManager<UserRole> usersRoles;
    private readonly IPasswordHasher passwordHasher;
    private readonly ITokenGenerator tokenGenerator;

    private const string refreshTokenName = "refreshToken";
    private const string accessTokenName = "accessToken";

    public UserManager( MongoDBSessionManager sessionManager, IPasswordHasher hasherOfPasswords, ITokenGenerator tokenGut ){
        usersRoles = new MongoDBManager<UserRole>(sessionManager, "UsersRoles");
        usersProfiles = new MongoDBManager<UserProfile>(sessionManager, "UsersProfiles");
        usersSessions = new MongoDBManager<UserSession>(sessionManager, "UsersSessions");
        passwordHasher = hasherOfPasswords;
        tokenGenerator = tokenGut;
    }

    public UMException<UserToken> SignUpUser(UserClaims user, string nonHashedPassword = "", bool autoLogin = false){
        UMException<UserToken> result = new UMException<UserToken>();

        // validating all the fields in UserClaims
        result.AddExceptionRange(user.Validate(nonHashedPassword == ""));

        { // checking for user existance in database
            var existing_user = usersProfiles.FindOne( val => val.UserName == user.UserName || val.Email == user.Email );
            Console.WriteLine(existing_user is not null && existing_user.UserName is not null && existing_user.UserName == user.UserName);
            if( existing_user is not null && existing_user.UserName is not null && existing_user.UserName == user.UserName){
                result.AddException(new Exception("This username is already used"));
            }
            if( existing_user is not null && existing_user.Email is not null && existing_user.Email == user.Email){
                result.AddException(new Exception("Your email is already used"));
            }
        }

        if(nonHashedPassword != "")
            user.PasswordHash = passwordHasher.GetHash(nonHashedPassword);

        Console.WriteLine($"password verification -> {passwordHasher.VerifyPassword(user.PasswordHash, nonHashedPassword)}");

        if(result.Erorrs.Count != 0)
            return result;

        UserRole? defaultRole = usersRoles.FindOne( ( UserRole user ) => user.AccessRate == AuthorityRate.Default && user.RoleName == "player" );
        if(defaultRole == null){
            defaultRole = new UserRole(){
                ID = Guid.NewGuid(),
                RoleName = "player",
                AccessRate = AuthorityRate.Default
            };
            usersRoles.InsertOne(defaultRole);
        }

        UserProfile newUser = new UserProfile{
            ID = Guid.NewGuid(),
            UserName = user.UserName,
            ProfileName = user.ProfileName,
            PasswordHash = user.PasswordHash,
            Email = user.Email,
            RoleID = defaultRole.ID
        };

        usersProfiles.InsertOne(newUser);

        if(autoLogin)
            result.AddRange(LogInUser(newUser));

        return result;
    }

    public UMException<UserToken> LogInUser(UserProfile user){
        UMException<UserToken> result = new UMException<UserToken>();

        if(usersProfiles.FindOne( (UserProfile dbUser) => dbUser.UserName == user.UserName && dbUser.Email == user.Email && dbUser.PasswordHash == user.PasswordHash ) != null){
            UserToken tokens = tokenGenerator.GenerateUserToken(user);
            UserSession session = new UserSession(){
                UserID = user.ID,
                Token = tokens,
                GenerationTime = DateTime.Now,
            };

            result.Value = tokens;

            usersSessions.InsertOne(session);

            // usersSessions.DeleteMany( ( UserSession ses ) => (DateTime.Now - (DateTime)(ses.GenerationTime.ToLocalTime())) > TimeSpan.FromDays(7) );
        }
        else{
            result.AddException(new Exception("No user found with those credentials"));
        }

        return result;

    }

    public UMException<UserToken> LogInWithPassword( UserClaims user, string nonHashedPassword){
        UMException<UserToken> result = new UMException<UserToken>();

        UserProfile? loginningUser = usersProfiles.FindOne( (UserProfile dbUser) => user.Email == dbUser.Email || user.UserName == dbUser.UserName );
        if(loginningUser is null){
            result.AddException(new Exception("User with those credentials not found :("));
        }
        else if( loginningUser.PasswordHash is not null && 
                 passwordHasher.VerifyPassword(
                    loginningUser.PasswordHash, nonHashedPassword
                )){
            result.AddRange(LogInUser(loginningUser));
        }else{
            result.AddException(new Exception("Your credentials don't match. ()_-"));
        }

        return result;
    }
    private async Task<UMException<UserToken>> GetTokenFromRequestAsync( HttpRequest request ){
        return await Task.Run( () => {
            UMException<UserToken> result = new UMException<UserToken>();

            string? accessToken = String.Empty;
            string? refreshToken = String.Empty;
            
            if(!request.Cookies.TryGetValue(accessTokenName, out accessToken) 
                    || 
                !request.Cookies.TryGetValue(refreshTokenName, out refreshToken)){
                result.AddException(new Exception("Not enough tokens found"));
                return result;
            }

            UserToken tokonUser = new UserToken(){
                AccessToken = accessToken!,
                RefreshToken = refreshToken!
            };

            result.Value = tokonUser;

            return result;
        });
    }
    public async Task<UMException<UserToken>> UpdateTokenAsync( HttpRequest request ){
        UMException<UserToken> result = new UMException<UserToken>();

        UserToken tokonUser;
        var extractionResult = await GetTokenFromRequestAsync(request);
        try{
            tokonUser = extractionResult.Value;
        }catch {
            result.AddExceptionRange(extractionResult.Erorrs);
            return result;
        }
        
        var tokenData = await tokenGenerator.ExtractDataAsync(tokonUser.AccessToken);
        var user = usersProfiles.FindOne( ( UserProfile person ) => person.ID == tokenData!.UserID );
        if( user == null ){
            result.AddException(new Exception("User not found"));
            return result;
        }

        {
            var validationResult = tokenGenerator.IsTokenValid(tokonUser, user);
            if(!validationResult.IsSuccessful()){
                result.AddExceptionRange( validationResult.Erorrs );
            }
        }

    
        result.AddRange(LogInUser(user));
        return result;
    }
    public async Task<bool> VerifyTokenAsync(HttpRequest request){
        UserToken tokonUser;
        try{
            tokonUser = (await GetTokenFromRequestAsync(request)).Value;
        }catch {
            return false; // token verification failed
        }

        var tokenData = await tokenGenerator.ExtractDataAsync(tokonUser.AccessToken);
        var user = usersProfiles.FindOne( ( UserProfile person ) => person.ID == tokenData!.UserID );
        if( user == null )
            return false;

        var validationResult = tokenGenerator.IsTokenValid(tokonUser, user);
        
        if(!validationResult.IsSuccessful())
            return false;

        return true;
    }

    public string GetEmailToken( UserProfile user ){
        return tokenGenerator.GenerateToken(new { 
            Email = user.Email,
            DateStamp = DateTime.Now
        });
    }
    public bool VerifyEmailToken(string emailToken, UserProfile user) => user.VerificationCode == emailToken;


    public async Task<UMException<UserProfile>> GetCurrentUserAsync(HttpRequest request){
        var result = new UMException<UserProfile>();
        UserToken tokonUser;
        try{
            tokonUser = (await GetTokenFromRequestAsync(request)).Value;
        }catch {
            result.AddException(new Exception("Token verification failed"));
            return result;
        }

        var tokenData = await tokenGenerator.ExtractDataAsync(tokonUser.AccessToken);
        var user = usersProfiles.FindOne( ( UserProfile person ) => person.ID == tokenData!.UserID );
        if( user == null )
            result.AddException(new Exception("User not found"));
        else
            result.Value = user;

        return result;
    }
    public UMException<UserProfile> FindUserBy( Expression<Func<UserProfile, bool>> predicate ){
        UMException<UserProfile> result = new UMException<UserProfile>();
        result.Value = usersProfiles.FindOne(predicate)!;
        return result;
    }

    public UMException<UserProfile> FindUserByUserName( string userName ) => FindUserBy( (UserProfile user) => user.UserName == userName );

}