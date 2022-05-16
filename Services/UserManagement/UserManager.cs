using Halero.Models.UserManagement;
using Halero.Models;

namespace Halero.Services.UserManagement;


class UserManager : IUserManager{
    private readonly IDatabaseManager<UserProfile> usersProfiles;
    private readonly IDatabaseManager<UserSession> usersSessions;
    private readonly IDatabaseManager<UserRole> usersRoles;
    private readonly IPasswordHasher passwordHasher;

    public UserManager( MongoDBSessionManager sessionManager, IPasswordHasher hasherOfPasswords ){
        usersRoles = new MongoDBManager<UserRole>(sessionManager, "UsersRoles");
        usersProfiles = new MongoDBManager<UserProfile>(sessionManager, "UsersProfiles");
        usersSessions = new MongoDBManager<UserSession>(sessionManager, "UsersSessions");
        passwordHasher = hasherOfPasswords;
    }

    public IEnumerable<Exception> SignUpUser(UserClaims user, string nonHashedPassword = ""){
        List<Exception> exceptions = new List<Exception>();

        // validating all the fields in UserClaims
        exceptions = user.Validate(nonHashedPassword == "");

        { // checking for user existance in database
            var existing_user = usersProfiles.FindOne( val => val.UserName == user.UserName || val.Email == user.Email );
            if( existing_user is not null && existing_user.UserName is not null && existing_user.UserName == user.UserName){
                exceptions.Add(new Exception("This username is already used"));
            }
            if( existing_user is not null && existing_user.Email is not null && existing_user.Email == user.Email){
                exceptions.Add(new Exception("Your email is already used"));
            }
        }

        if(exceptions.Count() != 0)
            return exceptions;


        if(nonHashedPassword != "")
            user.PasswordHash = passwordHasher.GetHash(nonHashedPassword);

        UserRole defaultRole = new UserRole();
        UserProfile newUser = new UserProfile{
            ID = new Guid( $"{user.ToString()}.{DateTime.Now}" ),
            UserName = user.UserName,
            ProfileName = user.ProfileName,
            PasswordHash = user.PasswordHash,
            Role = defaultRole
        };


        return exceptions;
    }

    

}