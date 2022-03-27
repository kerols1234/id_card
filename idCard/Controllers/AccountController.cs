using idCard.Data;
using idCard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace idCard.Controllers
{
    [Route("api/[controller]/[action]")]
    [EnableCors]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        //private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration, AppDbContext db, UserManager<User> userManager/*, RoleManager<IdentityRole> roleManager*/)
        {
            _userManager = userManager;
            //_roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = GenerateJSONWebToken(model.Email);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(token.ValidTo, "Egypt Standard Time").ToString("dd-MM-yyyy hh:mm tt")
                });
            }
            return Unauthorized("Wrong email or password");
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetUserInfo()
        {
            var claim = User.Claims.FirstOrDefault(obj => obj.Type == "Email");
            if (claim == null)
            {
                return BadRequest("Wrong User email");
            }
            return Ok(_userManager.Users.Where(obj => obj.Email == claim.Value).Select(obj => new UserModel
            {
                Email = obj.Email,
                Name = obj.Name,
                PhoneNumber = obj.PhoneNumber
            }).FirstOrDefault());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UserModel model)
        {
            var claim = User.Claims.FirstOrDefault(obj => obj.Type == "Email");
            if (claim == null)
            {
                return BadRequest("Wrong User email");
            }

            var user = _userManager.Users.FirstOrDefault(obj => obj.Email == claim.Value);

            if (user == null)
            {
                return BadRequest("No user with this email");
            }

            if (model.PhoneNumber != null && model.PhoneNumber.Trim() != "")
            {
                if (_userManager.Users.Any(obj => obj.Email != user.Email && obj.PhoneNumber == model.PhoneNumber))
                {
                    return BadRequest("Phone number already exists!");
                }
                user.PhoneNumber = model.PhoneNumber;
            }

            if (model.Email != null && model.Email.Trim() != "" && claim.Value != model.Email)
            {
                if (_userManager.Users.Any(obj => obj.Email == model.Email))
                {
                    return BadRequest("Email already exists!");
                }
                user.Email = model.Email;
                user.UserName = model.Email;
            }

            if (model.Name != null && model.Name.Trim() != "")
            {
                user.Name = model.Name;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var token = GenerateJSONWebToken(user.Email);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(token.ValidTo, "Egypt Standard Time").ToString("dd-MM-yyyy hh:mm tt")
                });
            }

            var Descriptions = result.Errors.Select(obj => obj.Description);
            var errorMassege = string.Join(',', Descriptions);
            return BadRequest(errorMassege);
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            
            if (userExists != null)
            {
                return BadRequest("Email already exists!");
            }

            if (_userManager.Users.Any(obj => obj.PhoneNumber == model.PhoneNumber))
            {
                return BadRequest("PhoneNumber already exists!");
            }

            User user = new User()
            {
                Email = model.Email,
                UserName = model.Email,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = GenerateJSONWebToken(model.Email);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(token.ValidTo, "Egypt Standard Time").ToString("dd-MM-yyyy hh:mm tt")
                });
            }

            var Descriptions = result.Errors.Select(obj => obj.Description);
            var errorMassege = string.Join(',', Descriptions);
            return BadRequest(errorMassege);
        }

        private JwtSecurityToken GenerateJSONWebToken(string email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] { new Claim("Email", email) };

            var token = new JwtSecurityToken(_configuration["JWT:ValidIssuer"],
              _configuration["JWT:ValidAudience"],
              claims,
              expires: DateTime.Now.AddHours(8),
              signingCredentials: credentials);

            return token;
        }
    }
}