using Halero.Models.UserManagement.Forms;

namespace Halero.Models.UserManagement;

//
/// <summary>
/// That's user management service exception
/// </summary>
public class UMException<T> {
    public List<Exception> Erorrs { get; private set; }
    private T? _val;

    /// <summary> <exception>
    /// Throws exception when there were errors during before the result
    /// </exception> </summary>
    public T Value { get{
        if(Erorrs.Count() == 0 && _val != null)
            return _val;
        throw new Exception("Error on data getting");
    } set{
        _val = value;
    }}

    public UMException(T? value = default(T)) {
        _val = value;
        Erorrs = new List<Exception>();
    }

    public void AddException(Exception newException){
        Erorrs.Add(newException);
    }
    public void AddExceptionRange(IEnumerable<Exception> exceptions){
        Erorrs.AddRange(exceptions);
    }

    /// <summary>adds two instances of UserManagementException</summary>
    public void AddRange(UMException<T> otherOne){
        Erorrs.AddRange(otherOne.Erorrs);
        try{
            Value = otherOne.Value;
        }catch {}
    }

    public bool IsSuccessful(){
        return Erorrs.Count() == 0 && _val != null;
    }

    public static AuthenticationForm ToAuthenticationForm(UMException<UserToken> authenticationResponce){
        var result = new AuthenticationForm();
        if(authenticationResponce.IsSuccessful())
            result.Token = (UserToken)authenticationResponce.Value;
        else{
            result.Errors = new List<string>();
            foreach(var error in authenticationResponce.Erorrs){
                result.Errors = result.Errors.Append(error.Message);
            }
        }
        return result; 
    }
}