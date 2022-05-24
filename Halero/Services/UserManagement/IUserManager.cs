using System.Linq.Expressions;
using Halero.Models.UserManagement;

namespace Halero.Services.UserManagement;

interface IUserManager{
    UMException<HttpResponse> SignUpUser(UserClaims user, HttpResponse response, string nonHashedPassword = "", bool autoLogin = false);
    UMException<HttpResponse> LogInUser(UserProfile user, HttpResponse response);
    UMException<HttpResponse> LogInWithPassword( UserClaims user, string nonHashedPassword, HttpResponse response);

    Task<UMException<HttpResponse>> UpdateTokenAsync( HttpRequest request, HttpResponse response );
    Task<bool> VerifyTokenAsync( HttpRequest request );
    string GetEmailToken( UserProfile user );
    bool VerifyEmailToken( string emailToken, UserProfile user );

    UMException<UserProfile> FindUserBy( Expression<Func<UserProfile, bool>> predicate );
    UMException<UserProfile> FindUserByUserName( string userName );
}