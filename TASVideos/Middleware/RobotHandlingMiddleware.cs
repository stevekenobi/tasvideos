﻿using System.Text;

namespace TASVideos.Middleware;

/// <summary>
/// Initializes a new instance of the <see cref="RobotHandlingMiddleware"/> class.
/// </summary>
public class RobotHandlingMiddleware(RequestDelegate request, IHostEnvironment env)
{
	public async Task Invoke(HttpContext context)
	{
		var sb = new StringBuilder();

		if (env.IsProduction())
		{
			sb.AppendLine("""

						User-agent: *
						Disallow: /forum/
						Disallow: /movies/
						Disallow: /submissions/
						Disallow: /media/
						Disallow: /MovieMaintenanceLog
						Disallow: /UserMaintenanceLog
						Disallow: /InternalSystem/
						Disallow: /*?revision=*
						Disallow: /Wiki/PageHistory

						User-agent: Mediapartners-Google
						Allow: /forum/

						User-agent: Fasterfox
						Disallow: /

						""");
		}
		else
		{
			sb
				.AppendLine("User-agent: *")
				.AppendLine("Disallow: / ");
		}

		context.Response.StatusCode = 200;
		context.Response.ContentType = "text/plain";
		await context.Response.WriteAsync(sb.ToString());
	}
}
