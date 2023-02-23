using Kuari.AuthServer.SharedLibrary.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kuari.AuthServer.API.Controllers
{
 
    public class CustomBaseController : ControllerBase
    {
       public IActionResult ActionResultInstance<T>(Response<T> response)
        {
            return new ObjectResult(response)
            {
                StatusCode = response.StatusCode
            };
            
        }
    }
}
