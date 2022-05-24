using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace Halero.Services.UserManagement;

public class TokenHMACSHA256Signer: ITokenSigner{

    public string SignToken( string payload, string secret ){
       
        HMACSHA256 cryproMaster = new HMACSHA256( Encoding.UTF8.GetBytes(secret) );     
        string tokenBody = $"{Base64Encoder(this.ToString())}.{Base64Encoder(payload)}";

        return  tokenBody + "." + Convert.ToBase64String(
            cryproMaster.ComputeHash(
                Encoding.UTF8.GetBytes(
                    tokenBody
                )
            ));
    }

    public string Base64Encoder( string payload ) => Convert.ToBase64String( Encoding.UTF8.GetBytes( payload ) );
    public string Base64Decoder( string basePayload) => Encoding.UTF8.GetString( Convert.FromBase64String( basePayload ) );

    public override string ToString(){
        return JsonSerializer.Serialize(
            new {
                algo = "HS256",
                typ = "JWT" 
            });
    }
}