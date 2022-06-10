namespace Halero.Models.UserManagement.Forms;

public class UserProfileCard{
    public string userName {get; set;}
    public string profileName {get; set;}
    public string pictureUri {get; set;}

    public UserProfileCard(string username, string profileName, string pictureUri){
        this.userName = username;
        this.profileName = profileName;
        this.pictureUri = pictureUri;
    }
}