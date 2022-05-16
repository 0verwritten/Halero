using Halero.Models.UserManagement;

namespace Halero.Services.UserManagement;

interface IUserManager{
    public IEnumerable<Exception> SignUpUser( UserClaims user );
    public IEnumerable<Exception> LogInUser( UserProfile user );
    public IEnumerable<Exception> LogInWithPassword( UserClaims user );

    public UserToken UpdateToken( UserToken oldToken );
    public bool VerifyToken( UserToken token );
    public string GetEmailToken( UserProfile user );
    public bool VerifyEmailToken( string emailToken );

    public UserProfile FindUserBy( Func<UserProfile, bool> predicate );
    public UserProfile FindUserByUserName( string userName );
}