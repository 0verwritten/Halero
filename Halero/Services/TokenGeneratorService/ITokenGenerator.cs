using Halero.Models.UserManagement;
using Halero.Models.UserManagement.TokenManagement;

namespace Halero.Services.UserManagement;

interface ITokenGenerator{
    string GenerateToken<T>( T payload );
    Task<string> GenerateTokenAsync<T>( T payload );
    UserToken GenerateUserToken( UserProfile user );
    Task<UserToken> GenerateUserTokenAsync( UserProfile user );
    UMException<bool> IsTokenValid( UserToken token, UserProfile user );
     Task<TokenBody?> ExtractDataAsync( string token );
}