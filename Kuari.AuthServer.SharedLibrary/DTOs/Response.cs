using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kuari.AuthServer.SharedLibrary.DTOs
{
    public class Response<T>
    {
        public T Data { get; private set; }
        public int StatusCode { get; private set; }

        [JsonIgnore]
        public bool IsSuccessfull { get; private set; } // hatayı statuscode'a bakarak anlamak yerine buradan anlayacağız.
        public ErrorDto Error { get; private set;}

        public static Response<T> Success(T data, int statusCode)
        {
            return new Response<T> { Data = data, StatusCode = statusCode, IsSuccessfull= true };
        }
        public static Response<T> Success(int statusCode)
        {
            return new Response<T> { Data=default, StatusCode=statusCode, IsSuccessfull = true };
        }
        // çoklu hata alındıysa
        public static Response<T> Fail(ErrorDto errorDto, int statusCode)
        {
            return new Response<T> { Error = errorDto, StatusCode = statusCode, IsSuccessfull = false };
         
        }
        //Tek Hata Alınmış ise sadece bir hata dönemmiz gerekmektedir.
        public static Response<T> Fail(string errorMessage,int statusCode, bool isShow)
        {
            var errorDto = new ErrorDto(errorMessage, isShow);

            return new Response<T> { Error = errorDto, StatusCode = statusCode, IsSuccessfull = false };
        }
    }
}

