using Kuari.AuthServer.Core.Configuration;
using Kuari.AuthServer.Core.DTOs;
using Kuari.AuthServer.Core.Models;
using Kuari.AuthServer.Core.Repositories;
using Kuari.AuthServer.Core.Services;
using Kuari.AuthServer.Core.UnitOfWork;
using Kuari.AuthServer.SharedLibrary.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuari.AuthServer.Service.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        // client classını biz veritabanında tutmak yerine appSettings.json'da tuttuk,daha sonra json objesi ile konuşacak classı oluşturduk, bu nedenle
        // bir servis üzerinden dependency Injection olarak ele almadık, çünkü veritabanında böyle bir entity yok. 
        private readonly List<Client> _clients; 
        private readonly ITokenService _tokenService;
        private readonly UserManager<UserApp> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _genericRepository;
        // refreshToken'ları veritabanına kaydedeceğimiz için IGenericRepository<UserRefreshToken>

        public AuthenticationService(IOptions<List<Client>> optionsClient, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> genericRepository )
        {
            _clients = optionsClient.Value;
            _tokenService = tokenService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _genericRepository = genericRepository;
        }
        public async Task<Response<TokenDto>> CreateToken(LoginDto loginDto)
        {
            // loginDto nullsa
            if (loginDto == null)
            {
                throw new ArgumentNullException(nameof(loginDto));
            }
            //Email kontrolü
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user.Email == null) return Response<TokenDto>.Fail("Email or Password is wrong", 400,true);
            // yukarıda email'de doğrulanmış ise demekki user doğru  artık passwordu kontrol edeceğiz.

            if( !await _userManager.CheckPasswordAsync(user,loginDto.Password))
            {
                return Response<TokenDto>.Fail("Email or Password is wrong", 400, true);
            }
            // eğerki yukarıdaki validasyon kontrolünüde client geçmiş olursa  artık AccessToken üretelim
            var token = _tokenService.CreateToken(user);
            // tokenı oluşturduktan sonra RefreshToken'ı veritabanına kaydedeceğiz ancak öncelikle veritabanında refreshToken var mı yok mu onu
            // kontrol etmemiz gerekmektedir.
            var userRefreshToken = await _genericRepository.Where(x => x.UserId == user.Id).SingleOrDefaultAsync(); // varsa true, yoksa false döner

            // Eğerki daha önce veritabanında böyle bir refreshToken yok ise artık yeni oluşturduğumuz AccessToken içerisindeki refreshTokenı veritabanına
            //kaydediyoruz.
            if (userRefreshToken == null)
            {
                await _genericRepository.AddAsync(new UserRefreshToken { UserId = user.Id, Code = token.RefreshToken, Expiration = token.RefreshTokenExpiration });
            }
            else   // eğerki userRefreshToken null değilse veritabanında mevcutsa onu güncellemeliyiz yeni oluşturduğumuz token'a göre
            {
                userRefreshToken.Code = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
            }
            // yukarıdaki validasyonlar sonucu token oluşturuldu, refreshToken kontrolü yapıldı ve dönen duruma göre  refresh token oluşturuldu varsa
            // güncellemesi yapıldı daha sonra artık veritabanına değişiklikleri yansıtmaya geldik.
            await _unitOfWork.CommitAsync();
            return Response<TokenDto>.Success(token, 200);
        }

        public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var client = _clients.SingleOrDefault(x => x.Id == clientLoginDto.ClientId && x.Secret == clientLoginDto.ClientSecret);
            if (client == null)
            {
                return Response<ClientTokenDto>.Fail("Secret or ClientId not found", 404, true);
            }
            var token = _tokenService.CreateTokenByClient(client);
            return Response<ClientTokenDto>.Success(token, 200);
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken)
        {
            // Bu methodda ki senaryomuz  client tarafından bize bir refreshToken gönderilir, bizde gelen refreshToken'a bakarız veritabanında kayıtlı ise
            // yeni accessToken oluşturup veririz. Ancak kayıtlı değilse böyle bir kullanıcı yok hatasını döneriz.
            // Önce Kontrolümüzü yapalım. Var mı yok mu ?
            var existRefreshToken = await _genericRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

            if (existRefreshToken == null)
            {
                return Response<TokenDto>.Fail("Refresh token not found", 404, true);
            }
            // refreshToken yoksa yukarıda hatayı fırlattık, ama varsa burada refreshToken'ın ait olduğu kullanıcıyı seçeceğiz.
            // Seçtikten sonra var mı yok mu kontrolü yapılacak ve varsa yeni gönderilen refreshToken stringini alıp, o kullanıcıya yeni accessToken 
            // göndereceğiz. Yani eskisi ile yeni oluşturulanı güncelleyeceğiz veritabanına yansıtacağız.
            var user = await _userManager.FindByIdAsync(existRefreshToken.UserId);
            if (user == null)
            {
                return Response<TokenDto>.Fail("UserId not found", 404, true);
            }
           
            var tokenDto = _tokenService.CreateToken(user);
            existRefreshToken.Code = tokenDto.RefreshToken;
            existRefreshToken.Expiration = tokenDto.RefreshTokenExpiration;

            // eski tokenın üzerine yeni tokenın bilgileri ile değişecek şekilde   güncellemesi yapıldı ve veritabanına yansıtalım

            await _unitOfWork.CommitAsync();
            return Response<TokenDto>.Success(tokenDto, 200);





        }

        public async Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken)
        {
            //kullanıcıdan parametre olarak gelen string formatındaki refreshTokenın veritabanımızda olup olmadığını kontrol edeceğiz varsa null ile set
            //edeceğiz, düşüreceğiz.

            var existRefrestoken = await _genericRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();
            if (existRefrestoken == null)
            {
                return Response<NoDataDto>.Fail("refresh Token not found", 404, true);
            }

            _genericRepository.Remove(existRefrestoken);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(200);
        }
    }
}
