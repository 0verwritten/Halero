using Halero.Models.UserManagement;
using Halero.Models.UserManagement.TokenManagement;
using Halero.Services;
using System.Text.Json;
using System.Text;

namespace Halero.Services.UserManagement;

class TokenGenerator : ITokenGenerator{

    private const string accessSecret = "5uvQqg+airdnR:yXu'JomT'EMq--!_NRcUAdWd-vgeb?d<RP4*_s4'Z,,Z>X29HnFUsN7K9S.Xjb=oT=xuF:WUMyNR-;=MEH>swkvHxuN?>LKN<9p'CpAbCructZowu:";
    private const string refreshSecret = "$z>.Tyqv==~Qhkp>p7U.uLN.@=paj?xHS/v$_g:XzRf7n?Lhv^sP#R/_^ha;2LMo4g9?=q^j.`tRHU+KM;hD^qZ*gYWtu-<V9jZ9E!ap?$d^+S-V.czLyM*JMWMQzZKk";
    private readonly ITokenSigner signer;

    public TokenGenerator(ITokenSigner signerService){
        signer = signerService;
    }
    public async Task<string> GenerateTokenAsync<T>( T payload ){
        MemoryStream jsonTokenData = new MemoryStream();
        await JsonSerializer.SerializeAsync(jsonTokenData, payload);
        var token = signer.SignToken( 
            Encoding.UTF8.GetString(jsonTokenData.ToArray()), accessSecret
        );
        return token.Replace('.', 'r');
    }
    public string GenerateToken<T>( T payload ) => GenerateTokenAsync(payload).Result;

    public async Task<UserToken> GenerateUserTokenAsync( UserProfile user ){
        // generating access token
        MemoryStream jsonTokenData = new MemoryStream();
        await JsonSerializer.SerializeAsync<TokenBody>(jsonTokenData, new TokenBody{
            GenerationTime = DateTime.Now,
            UserID = user.ID,
            lifeTimeSpan = 3 * 60 // for 3 hourse
        });
        var accessToken = signer.SignToken( 
            Encoding.UTF8.GetString(jsonTokenData.ToArray()), accessSecret
        );
        
        // generating refreshtoken
        await JsonSerializer.SerializeAsync<TokenBody>(jsonTokenData, new TokenBody{
            GenerationTime = DateTime.Now,
            UserID = user.ID,
            lifeTimeSpan = 3 * 60, // for 3 hourse
            SuperSecret = accessToken.Split(".")[2]
        });
        var refreshToken = signer.SignToken( 
            Encoding.UTF8.GetString(jsonTokenData.ToArray()), refreshSecret
        );

        return new UserToken { 
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
    public UserToken GenerateUserToken( UserProfile user ) => GenerateUserTokenAsync(user).Result;

    public UMException<bool> IsTokenValid( UserToken token, UserProfile user ){
        UMException<bool> exceptions = new UMException<bool>(true);
        var accessToken = token.AccessToken.Split(".");
        if(accessToken.Length != 3)
            exceptions.AddException(new Exception("Token format is invalid"));
        else if(signer.SignToken(accessToken[1], accessSecret) != accessToken[2])
            exceptions.AddException(new Exception("That token is fraud"));

        var refreshToken = token.RefreshToken.Split(".");
        if(refreshToken.Length != 3)
            exceptions.AddException(new Exception("Token format is invalid"));

        else if(signer.SignToken(refreshToken[1], accessSecret) != refreshToken[2])
            exceptions.AddException(new Exception("Token is not valid"));

        return exceptions;
    }

    public async Task<TokenBody?> ExtractDataAsync( string token ){

        var accessToken = token.Split(".");

        if( accessToken.Length != 3 )
            throw new Exception("Token format is invalid");
        if( accessToken[0] != signer.Base64Encoder(signer.ToString()) )
            throw new Exception("Token angorithm is not supported");


        return await JsonSerializer.DeserializeAsync<TokenBody>( new MemoryStream( Encoding.UTF8.GetBytes(accessToken[1]) ) );
    }

}