using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ArellanoCore.Api.Services.Interfaces;

namespace ArellanoCore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        //[HttpPost]
        //public async Task<string> Login()
        //{
        //    var categories = await _loginService.Login(BaseDatos); 
        //    return categories;
        //}

    }
}
