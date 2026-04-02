using Nadixa.Core.Entities;

namespace Nadixa.Core.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
