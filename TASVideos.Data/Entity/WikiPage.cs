﻿using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class WikiPage : BaseEntity, ISoftDeletable
{
	public int Id { get; set; }

	[StringLength(250)]
	public string PageName { get; set; } = "";

	public string Markup { get; set; } = "";

	public int Revision { get; set; } = 1;

	public bool MinorEdit { get; set; }

	[StringLength(1000)]
	public string? RevisionMessage { get; set; }

	public int? ChildId { get; set; }
	public virtual WikiPage? Child { get; set; } // The latest revision of a page is one with Child = null

	public bool IsDeleted { get; set; }

	[JsonIgnore]
	public NpgsqlTsVector SearchVector { get; set; } = null!;

	public int? AuthorId { get; set; }
	public virtual User? Author { get; set; }

	public bool IsCurrent() => !ChildId.HasValue && !IsDeleted;
}

public static class WikiQueryableExtensions
{
	public static IQueryable<WikiPage> ThatAreCurrent(this IQueryable<WikiPage> list)
	{
		return list.Where(wp => wp.ChildId == null);
	}

	public static IQueryable<WikiPage> ThatAreNotCurrent(this IQueryable<WikiPage> list)
	{
		return list.Where(wp => wp.ChildId != null);
	}

	/// <summary>
	/// Filters to pages at a specific indentation level
	/// Foo = 1
	/// Foo/Bar = 2
	/// Foo/Bar/Baz = 3
	/// </summary>
	public static IQueryable<WikiPage> ForPageLevel(this IQueryable<WikiPage> list, int indentationLevel)
	{
		int slashCount = indentationLevel - 1;
		return list.Where(wp => Regex.IsMatch(wp.PageName, $"^[^\\/]+(\\/[^\\/]+){{{slashCount}}}$"));
	}

	public static IQueryable<WikiPage> ForPage(this IQueryable<WikiPage> list, string pageName)
	{
		return list.Where(w => w.PageName == pageName);
	}

	public static IQueryable<WikiPage> Revision(this IQueryable<WikiPage> list, string pageName, int revision)
	{
		return list.Where(w => w.PageName == pageName && w.Revision == revision);
	}

	public static IQueryable<WikiPage> ExcludingMinorEdits(this IQueryable<WikiPage> list)
	{
		return list.Where(w => !w.MinorEdit);
	}

	public static IQueryable<WikiPage> CreatedBy(this IQueryable<WikiPage> list, string userName)
	{
		return list.Where(t => t.Author!.UserName == userName);
	}

	/// <summary>
	/// Filters the list of wiki pages to only pages that are nest beneath the given page.
	/// If no pageName is provided, then a master list of subpages is provided
	/// ex: /Foo/Bar, /Foo/Bar2 and /Foo/Bar/Baz are all subpages of /Foo.
	/// </summary>
	/// <param name="query">The query to filter.</param>
	/// <seealso cref="WikiPage"/>
	/// <param name="pageName">The name of the page to get Subpages from.</param>
	public static IQueryable<WikiPage> ThatAreSubpagesOf(this IQueryable<WikiPage> query, string? pageName)
	{
		pageName = (pageName ?? "").Trim('/');
		query = query
			.ThatAreNotDeleted()
			.ThatAreCurrent()
			.Where(wp => wp.PageName != pageName);

		if (!string.IsNullOrWhiteSpace(pageName))
		{
			query = query.Where(wp => wp.PageName.StartsWith(pageName + "/"));
		}

		return query;
	}

	/// <summary>
	/// Filters the list of wiki pages to only pages that are parents of the given page
	/// ex: /Foo is a parent of /Foo/Bar
	/// ex: /Foo and /Foo/Bar are parents of /Foo/Bar/Baz.
	/// </summary>
	/// <seealso cref="WikiPage"/>
	/// <param name="query">The query to filter.</param>
	/// <param name="pageName">The name of the page to get parent pages from.</param>
	public static IQueryable<WikiPage> ThatAreParentsOf(this IQueryable<WikiPage> query, string? pageName)
	{
		pageName = (pageName ?? "").Trim('/');
		if (string.IsNullOrWhiteSpace(pageName)
			|| !pageName.Contains('/')) // Easy optimization, pages without a / have no parents
		{
			return Enumerable.Empty<WikiPage>().AsQueryable();
		}

		return query
			.ThatAreNotDeleted()
			.ThatAreCurrent()
			.Where(wp => wp.PageName != pageName)
			.Where(wp => pageName.StartsWith(wp.PageName + "/"));
	}

	public static bool IsCurrent(this WikiPage? wikiPage)
	{
		return wikiPage is { ChildId: null, IsDeleted: false };
	}
}
