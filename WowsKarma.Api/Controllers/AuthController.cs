﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WowsKarma.Api.Services.Authentication.Wargaming;
using WowsKarma.Api.Services.Authentication.Jwt;
using Microsoft.Extensions.Configuration;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs;
using System.Collections.Generic;
using System.Text.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using static WowsKarma.Common.Utilities;
using System.Threading.Tasks;

namespace WowsKarma.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IConfiguration config;
		private readonly WargamingAuthService wargamingAuthService;
		private readonly JwtAuthService jwtService;

		public AuthController(IConfiguration config, WargamingAuthService wargamingAuthService, JwtAuthService jwtService)
		{
			this.config = config;
			this.wargamingAuthService = wargamingAuthService;
			this.jwtService = jwtService;
		}

		[HttpGet("test-auth")]
		public IActionResult TestAuth() => StatusCode(200, HttpContext.User.Claims.ToDictionary(kv => kv.Type, kv => kv.Value.FirstOrDefault()));

		[HttpGet("login")]
		public IActionResult Login() => wargamingAuthService.RedirectToLogin(Startup.ApiRegion, Request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.FirstOrDefault()));

		[HttpGet("wg-callback")]
		public async Task<IActionResult> WgAuthCallback()
		{
			/* 
			 * HACK: Callback cannot yet rely completely on remote server validation.
			 * MITM attacks are rare, but we must totaly eliminate the risk.
			 * 
			 * FIX THIS ASAP.
			 */
			Uri identityUri = new(Request.Query["openid.identity"].FirstOrDefault());

			if (!identityUri.ToString().Contains(WargamingAuthService.OpenIdDomain.Host))
			{
				return StatusCode(403);
			}

			WargamingIdentity identity = WargamingIdentity.FromUri(identityUri);
			JwtSecurityToken token = JwtAuthService.GenerateToken(identity.Claims.ToArray());

			List<UserClaimDTO> claims = identity.Claims.Select(c => new UserClaimDTO(c.Type, c.Value)).ToList();
			claims.Add(new("token", new JwtSecurityTokenHandler().WriteToken(token)));

			Response.Cookies.Append(
				config[$"Api:{Startup.ApiRegion.ToRegionString()}:CookieName"],
				JsonSerializer.Serialize(claims, CookieSerializerOptions),
				new()
				{
					Domain = config[$"Api:{Startup.ApiRegion.ToRegionString()}:DomainName"],
					HttpOnly = true,
					IsEssential = true,
					Expires = DateTimeOffset.UtcNow.AddDays(7)
				});

			return Request.Query["redirectUri"].FirstOrDefault() is string redirectUri 
				? Redirect(redirectUri)
				: StatusCode(200);
		}
	}
}
