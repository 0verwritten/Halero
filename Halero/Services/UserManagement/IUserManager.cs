using System.Linq.Expressions;
using Halero.Models.UserManagement;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Halero.Services.UserManagement;

public interface IUserManager{
    UMException<UserToken> SignUpUser(UserClaims user, string nonHashedPassword = "", bool autoLogin = false);
    UMException<UserToken> LogInUser(UserProfile user);
    UMException<UserToken> LogInWithPassword( UserClaims user, string nonHashedPassword);

    Task<UMException<UserToken>> UpdateTokenAsync( HttpRequest request );
    Task<bool> VerifyTokenAsync( HttpRequest request );
    string GetEmailToken( UserProfile user );
    bool VerifyEmailToken( string emailToken, UserProfile user );

    Task<UMException<UserProfile>> GetCurrentUserAsync(HttpRequest request);
    UMException<UserProfile> FindUserBy( Expression<Func<UserProfile, bool>> predicate );
    UMException<UserProfile> FindUserByUserName( string userName );
}