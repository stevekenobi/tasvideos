﻿using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.MoviesByAuthor)]
public class MoviesByAuthor(ApplicationDbContext db) : WikiViewComponent
{
	public List<string> NewbieAuthors { get; set; } = [];

	public List<PublicationEntry> Publications { get; set; } = [];

	public bool MarkNewbies { get; set; }
	public bool ShowClasses { get; set; }

	public async Task<IViewComponentResult> InvokeAsync(DateTime? before, DateTime? after, string? newbies, bool showTiers)
	{
		if (!before.HasValue || !after.HasValue)
		{
			return View();
		}

		var newbieFlag = newbies?.ToLower();
		var newbiesOnly = newbieFlag == "only";
		MarkNewbies = newbieFlag == "show";
		ShowClasses = showTiers;

		Publications = await db.Publications
			.ForDateRange(before.Value, after.Value)
			.Select(p => new PublicationEntry
			{
				Id = p.Id,
				Title = p.Title,
				Authors = p.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
				PublicationClassIconPath = p.PublicationClass!.IconPath
			})
			.ToListAsync();

		if (newbiesOnly || MarkNewbies)
		{
			NewbieAuthors = await db.Users
				.ThatArePublishedAuthors()
				.Where(u => u.Publications
					.OrderBy(p => p.Publication!.CreateTimestamp)
					.First().Publication!.CreateTimestamp.Year == after.Value.Year)
				.Select(u => u.UserName)
				.ToListAsync();
		}

		if (newbiesOnly)
		{
			Publications = Publications
				.Where(p => p.Authors.Any(a => NewbieAuthors.Contains(a)))
				.ToList();
		}

		return View();
	}

	public class PublicationEntry
	{
		public int Id { get; init; }
		public string Title { get; init; } = "";
		public IEnumerable<string> Authors { get; init; } = [];
		public string? PublicationClassIconPath { get; init; } = "";
	}
}
