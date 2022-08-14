﻿using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.GameGroups.Models;

public class GameGroupEditModel
{
	[Required]
	[StringLength(255)]
	public string Name { get; set; } = "";

	// TODO: actually rename the column
	[Display(Name = "Abbreviation")]
	[Required]
	[StringLength(255)]
	public string SearchKey { get; set; } = "";

	[StringLength(2000)]
	public string? Description { get; set; }
}
