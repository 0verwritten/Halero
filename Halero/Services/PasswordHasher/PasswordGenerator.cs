using System.Text;

namespace Halero.Services.UserManagement;

class PasswordGenerator{
    static public string Generate(int length = 20){
        byte[] arr = new byte[length];
        Random rnd = new Random();
        
        for(int i  = 0;i<length;i++)
            arr[i] = (byte)rnd.Next(33,126);

        return Encoding.ASCII.GetString(arr);
    }
}