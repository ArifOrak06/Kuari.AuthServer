using Kuari.AuthServer.Core.DTOs;
using Kuari.AuthServer.SharedLibrary.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuari.AuthServer.Core.Services
{
    public interface IAuthenticationService
    {
        Task<Response<TokenDto>> CreateToken(LoginDto loginDto);
        Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken);
        Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken); // tokenı düşürmek/silmek için
        Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto);

    }
}
