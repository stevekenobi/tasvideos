﻿namespace TASVideos.Api.Controllers;

/// <summary>
/// The game systems supported by TASVideos.
/// </summary>
[AllowAnonymous]
[Route("api/v1/[controller]")]
public class SystemsController(IGameSystemService db) : Controller
{
	/// <summary>
	/// Returns a game system with the given id.
	/// </summary>
	/// <response code="200">Returns the list of publications.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="404">A system with the given id was not found.</response>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(SystemsResponse), 200)]
	public async Task<IActionResult> Get(int id)
	{
		var system = (await db.GetAll())
			.SingleOrDefault(p => p.Id == id);

		return system is null
			? NotFound()
			: Ok(system);
	}

	/// <summary>
	/// Returns a list of available game systems, including supported framerates.
	/// </summary>
	/// <response code="200">Returns the list of systems.</response>
	[HttpGet]
	[ProducesResponseType(typeof(IEnumerable<SystemsResponse>), 200)]
	public async Task<IActionResult> GetAll()
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		return Ok(await db.GetAll());
	}
}
