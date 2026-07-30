using Nadixa.Core.Entities;

namespace Nadixa.Application.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
