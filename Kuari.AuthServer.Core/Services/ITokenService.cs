using Kuari.AuthServer.Core.Configuration;
using Kuari.AuthServer.Core.DTOs;
using Kuari.AuthServer.Core.Models;

namespace Kuari.AuthServer.Core.Services
{
    public interface ITokenService
    {
        TokenDto CreateToken(UserApp userApp);
        ClientTokenDto CreateTokenByClient(Client client);

    }
}
