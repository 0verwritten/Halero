namespace Halero.Services.UserManagement;

public interface ITokenSigner{
    string SignToken( string payload, string secret );
    string Base64Encoder( string payload );
    string Base64Decoder( string basePayload);
    string ToString();
}