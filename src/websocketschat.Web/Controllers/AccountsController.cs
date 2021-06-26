using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreModels = websocketschat.Core.Models;
using websocketschat.Core.Services.Interfaces;
using websocketschat.Datatransfer.HttpDto;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using websocketschat.Web.Helpers.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;

namespace websocketschat.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountsController> _logger;
        private readonly AuthOptions _authOptions;
        public AccountsController(IUserService userService, 
                                  IMapper mapper, 
                                  ILogger<AccountsController> logger,
                                  IOptions<AuthOptions> authOptions)
        {
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
            _authOptions = authOptions.Value;
        }

        //POST api/Accounts/register
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RegisterUser([FromForm] PostUserDto userDto)
        {
            _logger.LogInformation("Invoked api/Accounts/register");

            CoreModels.User coreUser = _mapper.Map<CoreModels.User>(userDto);
            bool isCreated = await _userService.AddUserAsync(coreUser, userDto.Password);

            if (isCreated)
                return StatusCode(201);

            return StatusCode(400);
        }

        //POST api/Accounts/token
        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> Token([FromForm] PostUserDto userDto)
        {

            _logger.LogInformation("Invoked api/Accounts/token");

            ClaimsIdentity identity = await GetIdentity(userDto.Username, userDto.Password);

            if (identity == null)
            {
                return StatusCode(400, new { Message = "Invalid username or password." });
            }

            var now = DateTime.UtcNow;

            // create token
            var jwt = new JwtSecurityToken(
                    issuer: _authOptions.Issuer,
                    audience: _authOptions.Audience,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(_authOptions.Lifetime)),
                    signingCredentials: new SigningCredentials(
                                        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_authOptions.SecretKey)),
                                            SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };

            return StatusCode(200, new { access_token = response.access_token, username = response.username });
        }

        private async Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            // check for existing in database.
            CoreModels.User coreUser = await _userService.GetUserWithRoleByUsernameAsync(username);

            // check for stored password in database.
            bool isAuthenticated = await _userService.Authenticate(username, password);

            if (coreUser != null && isAuthenticated)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, coreUser.Username),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, coreUser.Role.Name),

                    // Id
                    new Claim("Guid", coreUser.Id.ToString())
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            return null;
        }
    }
}
