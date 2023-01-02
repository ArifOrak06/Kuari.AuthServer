using Kuari.AuthServer.Core.Configuration;
using Kuari.AuthServer.Core.DTOs;
using Kuari.AuthServer.Core.Models;
using Kuari.AuthServer.Core.Services;
using Kuari.AuthServer.SharedLibrary.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kuari.AuthServer.Service.Services
{
    // TokenService'i API serviceLeri kullanmayacak, Core Katmanında sözleşmesini oluşturduğumuz IAuthenticationService kullanacak,API'lere dönecek.
    public class TokenService : ITokenService
    {
        // kullanıcılar ile ilgili işlem yapılacağı için IDentity library ile gömülü gelen UserManager'ı kullancağız
        private readonly UserManager<UserApp> _userManager;
        // Ayrıca token oluşturacağımız için ve CustomTokenOption classı içerisindeki ayarlara göre oluştrucağım için;
        private readonly CustomTokenOption _customTokenOption;

    

        public TokenService(UserManager<UserApp> userManager, IOptions<CustomTokenOption> options)
        {
            _userManager = userManager;
            _customTokenOption = options.Value;
        }
        private string CreateRefreshToken()
        {
            var numberByte = new Byte[32];
            using var rnd = RandomNumberGenerator.Create();
            rnd.GetBytes(numberByte);
            return Convert.ToBase64String(numberByte);
        }
        private IEnumerable<Claim> GetClaims(UserApp userApp, List<String> audiences)
        {
            var userList = new List<Claim>
            {
                // Kullanıcılar için  Bir tokenın payload kısmında olması gereken dataları belirttik.
                new Claim(ClaimTypes.NameIdentifier, userApp.Id),
                new Claim(JwtRegisteredClaimNames.Email, userApp.Email),
                new Claim(ClaimTypes.Name,userApp.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),// Token'a random TokenId ürettik, şart değil ama bestPractice
            
            };
            userList.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));
            return userList;
        }
        // clientlar yani üyelik sistemi gerekmeyen Apıler   oluşturulacak  bir tokenın payloadında olması gereken dataları tanımlayalım
        private IEnumerable<Claim> GetClaimsByClient(Client client)
        {
            var claims = new List<Claim>();
            claims.AddRange(client.Audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud,x)));
            //Bu Token'a random bir TokenId üretelim.
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
            // Bu Token'ıun öznesi yani kim için üretilecek Client için bu nedenle ClientId'sini Tokena bildirelim.
            new Claim(JwtRegisteredClaimNames.Sub,client.Id.ToString());
            return claims;
        }
        public TokenDto CreateToken(UserApp userApp)
        {
            // accessToken ömrü
            var accessTokenExpiration = DateTime.Now.AddMinutes(_customTokenOption.AccessTokenExpiration);
            // refreshToken ömrü
            var refreshTokenExpiration = DateTime.Now.AddMinutes(_customTokenOption.RefreshTokenExpiration);
            var securityKey = SignService.GetSymetricSecurityKey(_customTokenOption.SecurityKey);
            // tokenın imzasını da belirleyelim.
            SigningCredentials newSigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            // ve son olarak Token'ımızı oluşturalım
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _customTokenOption.Issuer, // token'ı yayınlayan kim ? bizim AuthServer API servisimiz
                expires: accessTokenExpiration,
                notBefore: DateTime.Now, // token'nın geçerli olacağı zaman
                claims: GetClaims(userApp, _customTokenOption.Audience),
                signingCredentials: newSigningCredentials);
            // Daha sonra token oluşturacak Class'tan nesne örneği alalım ve AccessToken'ımızı oluşturalım.
            var handler = new JwtSecurityTokenHandler();
            // tokenı oluşturalım, oluştururken WriteToken methodunu kullancağız yukarıda belirttiğim payload bilgilerine göre token üretecektir.
            var token = handler.WriteToken(jwtSecurityToken);
            // yukarıda üretilen tokenı bir dto nesnesine dönüştürmemiz lazım. 
            var tokenDto = new TokenDto
            {
                AccessToken = token,
                RefreshToken = CreateRefreshToken(),
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration

            };
            return tokenDto;
        }

        public ClientTokenDto CreateTokenByClient(Client client)
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(_customTokenOption.AccessTokenExpiration);
           
            var securityKey = SignService.GetSymetricSecurityKey(_customTokenOption.SecurityKey);
         
            SigningCredentials newSigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
      
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _customTokenOption.Issuer, 
                expires: accessTokenExpiration,
                notBefore: DateTime.Now, 
                claims: GetClaimsByClient(client),
                signingCredentials: newSigningCredentials);
         
            var handler = new JwtSecurityTokenHandler();
         
            var token = handler.WriteToken(jwtSecurityToken);
           
            var clientTokenDto = new ClientTokenDto
            {
                AccessToken = token,
                AccessTokenExpiration = accessTokenExpiration
              

            };
            return clientTokenDto;
        }
    }
}
