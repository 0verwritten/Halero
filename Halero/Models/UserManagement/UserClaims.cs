using System.Net.Mail;

namespace Halero.Models.UserManagement;

public class UserClaims{
    public string UserName { get; set; }
    public string ProfileName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }

    UserClaims(){
        UserName = String.Empty;
        ProfileName = String.Empty;
        Email = String.Empty;
        PasswordHash = String.Empty;
    }

    public override string ToString(){
        return $"{UserName}.{Email}";
    }
    public List<Exception> Validate(bool validatePassword = true){
        List<Exception> exceptions = new List<Exception>();
        if(Email == String.Empty)
            exceptions.Add(new Exception("Email cannot be empty"));
        else if(new MailAddress(Email).Address != Email)
            exceptions.Add(new Exception("Invalid email format"));
        if(UserName == String.Empty)
            exceptions.Add(new Exception("Enter valid username"));
        if(ProfileName == String.Empty)
            exceptions.Add(new Exception("You should enter a profile name :("));
        if(PasswordHash == String.Empty && validatePassword)
            exceptions.Add(new Exception("Please make a passord to sign up o_- "));

        return exceptions;
    }
}