using Halero.Models.UserManagement;
using Microsoft.AspNetCore.Http;
using Halero.Models;
using System.Web;
using System.Linq.Expressions;

namespace Halero.Services.UserManagement;


class UserManager : IUserManager{
    private readonly IDatabaseManager<UserProfile> usersProfiles;
    private readonly IDatabaseManager<UserSession> usersSessions;
    private readonly IDatabaseManager<UserRole> usersRoles;
    private readonly IPasswordHasher passwordHasher;
    private readonly ITokenGenerator tokenGenerator;

    private const string refreshTokenName = "sna_login";
    private const string accessTokenName = "sna_logout";

    public UserManager( MongoDBSessionManager sessionManager, IPasswordHasher hasherOfPasswords, ITokenGenerator tokenGut ){
        usersRoles = new MongoDBManager<UserRole>(sessionManager, "UsersRoles");
        usersProfiles = new MongoDBManager<UserProfile>(sessionManager, "UsersProfiles");
        usersSessions = new MongoDBManager<UserSession>(sessionManager, "UsersSessions");
        passwordHasher = hasherOfPasswords;
        tokenGenerator = tokenGut;
    }

    public UMException<HttpResponse> SignUpUser(UserClaims user, HttpResponse response, string nonHashedPassword = "", bool autoLogin = false){
        UMException<HttpResponse> result = new UMException<HttpResponse>();

        // validating all the fields in UserClaims
        result.AddExceptionRange(user.Validate(nonHashedPassword == ""));

        { // checking for user existance in database
            var existing_user = usersProfiles.FindOne( val => val.UserName == user.UserName || val.Email == user.Email );
            if( existing_user is not null && existing_user.UserName is not null && existing_user.UserName == user.UserName){
                result.AddException(new Exception("This username is already used"));
            }
            if( existing_user is not null && existing_user.Email is not null && existing_user.Email == user.Email){
                result.AddException(new Exception("Your email is already used"));
            }
        }

        if(nonHashedPassword != "")
            user.PasswordHash = passwordHasher.GetHash(nonHashedPassword);

        UserRole? defaultRole = usersRoles.FindOne( ( UserRole user ) => user.AccessRate == AuthorityRate.Default && user.RoleName == "player" );
        if(defaultRole == null){
            defaultRole = new UserRole(){
                ID = new Guid($"player.0"),
                RoleName = "player",
                AccessRate = AuthorityRate.Default
            };
            usersRoles.InsertOne(defaultRole);
        }

        UserProfile newUser = new UserProfile{
            ID = new Guid( $"{user.ToString()}.{DateTime.Now}" ),
            UserName = user.UserName,
            ProfileName = user.ProfileName,
            PasswordHash = user.PasswordHash,
            RoleID = defaultRole.ID
        };

        usersProfiles.InsertOne(newUser);

        if(autoLogin)
            result.AddRange(LogInUser(newUser, response));

        return result;
    }

    public UMException<HttpResponse> LogInUser(UserProfile user, HttpResponse response){
        UMException<HttpResponse> result = new UMException<HttpResponse>();

        if(usersProfiles.FindOne( (UserProfile dbUser) => dbUser == user ) != null){
            

            UserToken tokens = tokenGenerator.GenerateUserToken(user);
            UserSession session = new UserSession(){
                ID = new Guid($"{user.ID}.{DateTime.Now}"),
                UserID = user.ID,
                Token = tokens,
                GenerationTime = DateTime.Now,
            };
            response.Cookies.Append(accessTokenName, tokens.AccessToken);
            response.Cookies.Append(refreshTokenName, tokens.RefreshToken);

            result.Value = response;

            usersSessions.InsertOne(session);

            usersSessions.DeleteMany( ( UserSession ses ) => DateTime.Now - ses.GenerationTime > TimeSpan.FromDays(7) );

        }
        else{
            result.AddException(new Exception("No user found with those credentials"));
        }

        return result;

    }

    public UMException<HttpResponse> LogInWithPassword( UserClaims user, string nonHashedPassword, HttpResponse response){
        UMException<HttpResponse> result = new UMException<HttpResponse>();

        UserProfile? loginningUser = usersProfiles.FindOne( (UserProfile dbUser) => user.Email == dbUser.Email || user.UserName == dbUser.UserName );
        if(loginningUser is null){
            result.AddException(new Exception("User with those credentials not found :("));
        }
        else if( loginningUser.PasswordHash is not null && 
                 passwordHasher.VerifyPassword(
                    loginningUser.PasswordHash, nonHashedPassword
                )){
            result.AddRange(LogInUser(loginningUser, response));
        }

        return result;
    }
    private async Task<UMException<UserToken>> GetTokenFromRequestAsync( HttpRequest request ){
        return await Task.Run( () => {
        UMException<UserToken> result = new UMException<UserToken>();
                
        string? accessToken = String.Empty, 
                refreshToken = String.Empty;
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

        return result;
        });
    }
    public async Task<UMException<HttpResponse>> UpdateTokenAsync( HttpRequest request, HttpResponse response ){
        UMException<HttpResponse> result = new UMException<HttpResponse>();

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

    
        result.AddRange(LogInUser(user, response));
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


    public UMException<UserProfile> FindUserBy( Expression<Func<UserProfile, bool>> predicate ){
        UMException<UserProfile> result = new UMException<UserProfile>();
        result.Value = usersProfiles.FindOne(predicate)!;
        return result;
    }

    public UMException<UserProfile> FindUserByUserName( string userName ) => FindUserBy( (UserProfile user) => user.UserName == userName );

}