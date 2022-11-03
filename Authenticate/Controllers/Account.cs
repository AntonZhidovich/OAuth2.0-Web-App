using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Authenticate.Models;

namespace Authenticate.Controllers
{
    [Route("{controller}")]
    public class Account : Controller
    {
        private static Role adminRole = new Role("Admin");
        private static Role userRole = new Role("User");

        private IEnumerable<Person> people = new List<Person>
        {
            new Person("tom@gmail.com", "12345", userRole),
            new Person("bob@gmail.com", "55555", userRole),
            new Person("anton.zhidovich@gmail.com", "7777", adminRole),
            new Person("alexlubenko172@gmail.com", "1313", userRole)
        };

        [HttpGet, Route("login")]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Content("You are already authorized!");
            }

            return View();
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login(string? returnUrl, string? email, string? password)
        {
            if(email is null || password is null)
            {
                return new BadRequestObjectResult("Email or password is empty.");
            }

            Person? res = people.FirstOrDefault(p => p.Email == email && p.Password == password);
            if(res is null)
            {
                return new UnauthorizedObjectResult("Invalid email or password.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, res.Email) ,
                new Claim(ClaimsIdentity.DefaultRoleClaimType, res.Role.Name)
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Password");
            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));
            string redirUrl = returnUrl ?? "/";
            return Redirect(redirUrl);
        }

        [Route("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse", "Account") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [Route("googleresponse")]
        public async Task<IActionResult> GoogleResponse(string? returnUrl)
        {
            var emailClaim = User.FindFirstValue(ClaimTypes.Email);
            var email = emailClaim ?? "";
            Person? res = people.FirstOrDefault(p => p.Email == email);
            if (res is null)
            {
                return Content("User doesn't exist.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, res.Email) ,
                new Claim(ClaimsIdentity.DefaultRoleClaimType, res.Role.Name)
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Google");
            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));
            return Redirect("/"); 
        }

        [HttpGet, Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Main", "Home");
        }

    }
}
