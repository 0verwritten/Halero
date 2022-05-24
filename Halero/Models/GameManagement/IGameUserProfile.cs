using Halero.Models.UserManagement;

namespace Halero.Models.GameManagement;

interface IGameUserProfile{
    public UserProfile UserName { get; set; }
    public IGameSession Session { get; set; }

}