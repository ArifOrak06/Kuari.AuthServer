using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuari.AuthServer.SharedLibrary.DTOs
{
    public class ErrorDto
    {
        // örneğin kullanıcı arayüzden login olmak için inputları dolduruken hem usernamede hem passwordde hata yaptı,
        // bu iki hata durumu da kullanıcıya dönmem gerekecek, bu nedenle gelin string tutmak yerine hata dizisi tutalım.
        public List<String> Errors { get; private set; }
        public bool IsShow { get; private set; } // sadece kullanıcının anlayacağı bir hata ise göstereceğiz, yazılımcısının anlayacağı hata ise false'a setleyip kullanıcıya 
                                         // göstermeyeceğiz.

        public ErrorDto()
        {
            Errors = new List<string>();
        }
        public ErrorDto(string error, bool isShow)
        {
            Errors.Add(error);
            isShow = true;

        }
        public ErrorDto(List<string> errors, bool isShow)
        {
            Errors=errors;
            IsShow = isShow;

        }
    }
}
