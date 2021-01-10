using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Authentication;
using WowsKarma.Common.Models.DTOs;



namespace WowsKarma.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class AccountController : ControllerBase
	{
		private readonly PlayerService service;

		public AccountController(PlayerService playerService)
		{
			service = playerService;
		}

		[HttpGet("Search/{query}")]
		public async Task<IActionResult> SearchAccount(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return StatusCode(400, new ArgumentNullException(nameof(query)));
			}

			IEnumerable<AccountListingDTO> accounts = await service.ListPlayersAsync(query);

			return accounts is null
				? StatusCode(204) 
				: StatusCode(200, accounts);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetAccount(uint id)
		{
			if (id is 0)
			{
				return StatusCode(400, new ArgumentException(null, nameof(id)));
			}

			PlayerProfileDTO playerProfile = await service.GetPlayerAsync(id);

			return playerProfile is null
				? StatusCode(204)
				: StatusCode(200, playerProfile);
		}

		[HttpPost("Karmas"), AccessKey]
		public async Task<IActionResult> FetchKarmas([FromBody] uint[] ids) => StatusCode(200, AccountKarmaDTO.ToDictionary(await service.GetPlayersFullKarmaAsync(ids)));

		[HttpPost("KarmasFull"), AccessKey]
		public async Task<IActionResult> FetchFullKarmas([FromBody] uint[] ids) => StatusCode(200, await service.GetPlayersFullKarmaAsync(ids));
	}
}
