﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Spotyplace.Business.Managers;
using Spotyplace.Entities.DTOs;
using Spotyplace.Entities.Models;

namespace Spotyplace.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly AccountManager _accountManager;

        public AccountController(AccountManager accountManager)
        {
            _accountManager = accountManager;
        }

        [Route("login")]
        public IActionResult Login(string returnUrl = "/account")
        {
            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "/account";
            }

            return new ChallengeResult(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(LoginCallbackAsync), new { returnUrl })
            });
        }

        public async Task<IActionResult> LoginCallbackAsync(string returnUrl)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("Google");

            if (!authenticateResult.Succeeded)
            {
                return BadRequest();
            }

            var claimsIdentity = new ClaimsIdentity("Cookies");
            var emailClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Email);
            var nameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier);

            claimsIdentity.AddClaim(nameClaim);
            claimsIdentity.AddClaim(emailClaim);

            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));

            await _accountManager.CreateAccountAsync(new ApplicationUser()
            {
                UserName = emailClaim.Value,
                Email = emailClaim.Value,
                FullName = User.Identity.Name,
                EmailConfirmed = true
            });

            return LocalRedirect(returnUrl);
        }

        [Authorize]
        [Route("logout")]
        [HttpPost]
        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(true);
        }
        
        [Route("info")]
        public async Task<IActionResult> AccountInfoAsync()
        {
            if (User == null)
            {
                return Ok(null);
            }

            var user = await _accountManager.GetAccountInfoAsync(User.FindFirstValue(ClaimTypes.Email));
            if (user != null)
            {
                return Ok(new UserDto(user));
            }

            return Ok(null);
        }

        [Route("cookies")]
        public IActionResult CheckCookies()
        {
            // Check user consent
            var userConsentCookie = Request.Cookies["CookieConsent"];
            var consent = HttpContext.Features.Get<ITrackingConsentFeature>();

            if (userConsentCookie != null)
            {
                switch (userConsentCookie)
                {
                    case "0":
                        // The user has not accepted cookies - set strictly necessary cookies only
                        consent.WithdrawConsent();
                        break;

                    case "-1":
                        // The user is not within a region that requires consent - all cookies are accepted
                        consent.GrantConsent();
                        break;

                    default:
                        // The user has accepted one or more type of cookies
                        consent.GrantConsent();
                        break;
                }
            }
            else
            {
                // The user has not accepted cookies - set strictly necessary cookies only
                consent.WithdrawConsent();
            }

            return Ok();
        }
    }
}