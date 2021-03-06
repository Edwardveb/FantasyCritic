using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TokenController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly FantasyCriticUserManager _userManager;

        public TokenController(ITokenService tokenService, FantasyCriticUserManager userManager)
        {
            _tokenService = tokenService;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Refresh([FromBody] TokenRefreshRequest request)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.Token);
            var username = principal.Identity.Name; //this is mapped to the Name claim by default

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return BadRequest();
            }

            var refreshTokens = await _userManager.GetRefreshTokens(user);
            if (!refreshTokens.Contains(request.RefreshToken))
            {
                return BadRequest();
            }

            string issuedTimeString = principal.Claims.FirstOrDefault(x => x.Type == "nbf")?.Value;
            if (issuedTimeString == null)
            {
                return BadRequest("Invalid JWT.");
            }

            Instant issuedTime = Instant.FromUnixTimeSeconds(Convert.ToInt64(issuedTimeString));
            if (issuedTime < user.LastChangedCredentials)
            {
                return StatusCode(401, "No Retry");
            }

            var newJwtToken = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _userManager.RemoveRefreshToken(user, request.RefreshToken);
            await _userManager.AddRefreshToken(user, newRefreshToken);

            var newJwtString = new JwtSecurityTokenHandler().WriteToken(newJwtToken);

            return new ObjectResult(new
            {
                token = newJwtString,
                refreshToken = newRefreshToken,
                expiration = newJwtToken.ValidTo
            });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Revoke()
        {
            var username = User.Identity.Name;

            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return BadRequest();

            await _userManager.RemoveAllRefreshTokens(user);

            return NoContent();
        }

    }
}
