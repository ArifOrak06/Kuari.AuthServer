using AutoMapper.Internal.Mappers;
using Kuari.AuthServer.Core.DTOs;
using Kuari.AuthServer.Core.Models;
using Kuari.AuthServer.Core.Services;
using Kuari.AuthServer.Service.Mappings.AutoMapper;
using Kuari.AuthServer.SharedLibrary.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuari.AuthServer.Service.Services
{
    public class UserService : IUserService
    {
        //Kullanıcı ile ilgil operasyonlar yapılacağı için İdentity Kütüphanesi ile gömülü gelen USerManager'a ihtiyacımız oalcaktır
        private readonly UserManager<UserApp> _userManager;

        public UserService(UserManager<UserApp> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = new UserApp { Email = createUserDto.Email, UserName = createUserDto.Username };
            // Yukarıda passwordu vermedik, çünkü passwordu İdentity kütüphanesi hashleme yaptıktan sonra veritabanına kayıt ediyor o yüzden passwordu
            // veritabanına ekleyeceği method içerisinde hashledikten sonra veritabanına kayıt ediyor. gelin şimdi onu yapalım.
            var result = await _userManager.CreateAsync(user,createUserDto.Password);
            // yukarıda veritabanına kayıt edeceği veriyi memory'e aldığında başarılı veya başarısız şeklinde bir result döner, hata varsa result içerisinde
            // bulunan errors kısmına hataları kaydeder, başarılı değilse hataları ele alıp Response nesnemiz ile birlikte client'a dönelim.
               
            if (!result.Succeeded)
            {
                // eğer başarılı bir şekilde kayıt edilmemişse;
                var errors = result.Errors.Select(x=> x.Description).ToList();
                return Response<UserAppDto>.Fail(new ErrorDto(errors, true),400);
            }
            return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user),200);
        }

        public async Task<Response<UserAppDto>> GetUserByNameAsync(string username)
        {
            // bu operasyonda ise öncelikle clienttan gelen username parametresine göre kullanıcı adına göre kulalnıcı var mı yok mu onu kontrol ediyoruz
            // akabinde dönen değer nullsa hata, değilse entity'i dtoya mapleyip APı'ye dönüyoruz.
            var user = await _userManager.FindByNameAsync(username);
            if(user == null)
            {
                return Response<UserAppDto>.Fail("Username not found", 404, true);
            }
            return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), 200);
        }
    }
}
