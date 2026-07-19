namespace Infrastructure.Documentation;

using System.Net;
using System.Text;
using Application.Calculator.Documentation;

public sealed class HtmlDocumentationRenderer : IDocumentationRenderer
{
    public string Render(IReadOnlyList<FunctionDoc> functions, IReadOnlyList<OperatorDoc> operators)
    {
        var groups = functions
            .GroupBy(f => f.Category)
            .OrderBy(g => g.Key)
            .ToList();

        var html = new StringBuilder();

        html.Append("<!doctype html>\n<html lang=\"en\">\n<head>\n<meta charset=\"utf-8\">\n");
        html.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n");
        html.Append("<title>Axoloop Calculator &mdash; Documentation</title>\n");
        html.Append("<style>\n").Append(Css).Append("\n</style>\n");
        html.Append("</head>\n<body>\n");

        html.Append("<header>\n<h1>Axoloop Calculator</h1>\n");
        html.Append("<p class=\"subtitle\">Documentation for functions and operators</p>\n");
        html.Append("<input type=\"search\" id=\"search\" placeholder=\"Search a function or operator...\" autofocus>\n");
        html.Append("</header>\n");

        html.Append("<nav id=\"toc\">\n<ul>\n");
        foreach (var group in groups)
            html.Append($"<li><a href=\"#cat-{Encode(group.Key.ToString())}\">{Encode(group.Key.ToString())}</a></li>\n");
        html.Append("<li><a href=\"#operators\">Operators</a></li>\n");
        html.Append("</ul>\n</nav>\n");

        html.Append("<main>\n");

        foreach (var group in groups)
        {
            html.Append($"<section class=\"category\" id=\"cat-{Encode(group.Key.ToString())}\">\n");
            html.Append($"<h2>{Encode(group.Key.ToString())}</h2>\n");

            foreach (var fn in group.OrderBy(f => f.Name, StringComparer.Ordinal))
            {
                var searchText = Encode($"{fn.Name} {fn.Signature} {fn.Description}".ToLowerInvariant());
                html.Append($"<article class=\"card\" id=\"fn-{Encode(fn.Name)}\" data-search=\"{searchText}\">\n");
                html.Append($"<h3><code>{Encode(fn.Signature)}</code></h3>\n");
                html.Append($"<p class=\"description\">{Encode(fn.Description)}</p>\n");

                if (fn.Examples.Count > 0)
                {
                    html.Append("<ul class=\"examples\">\n");
                    foreach (var example in fn.Examples)
                        html.Append($"<li><code>{Encode(example)}</code></li>\n");
                    html.Append("</ul>\n");
                }

                html.Append("</article>\n");
            }

            html.Append("</section>\n");
        }

        html.Append("<section class=\"category\" id=\"operators\">\n<h2>Operators</h2>\n");
        html.Append("<table>\n<thead>\n<tr>");
        html.Append("<th>Symbol</th><th>Type</th><th>Precedence</th><th>Associativity</th>");
        html.Append("<th>Description</th><th>Example</th></tr>\n</thead>\n<tbody>\n");

        foreach (var op in operators)
        {
            var searchText = Encode($"{op.Symbol} {op.Description}".ToLowerInvariant());
            html.Append($"<tr class=\"operator-row\" data-search=\"{searchText}\">");
            html.Append($"<td><code>{Encode(op.Symbol)}</code></td>");
            html.Append($"<td>{Encode(op.Kind.ToString())}</td>");
            html.Append($"<td>{(op.Precedence.HasValue ? op.Precedence.Value.ToString() : "&mdash;")}</td>");
            html.Append($"<td>{(op.Associativity.HasValue ? Encode(op.Associativity.Value.ToString()) : "&mdash;")}</td>");
            html.Append($"<td>{Encode(op.Description)}</td>");
            html.Append($"<td><code>{Encode(op.Example)}</code></td>");
            html.Append("</tr>\n");
        }

        html.Append("</tbody>\n</table>\n</section>\n");
        html.Append("</main>\n");

        html.Append("<script>\n").Append(Js).Append("\n</script>\n");
        html.Append("</body>\n</html>\n");

        return html.ToString();
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private const string Css = @"
:root {
    color-scheme: light dark;
    --bg: #ffffff;
    --fg: #1a1a1a;
    --muted: #5a5a5a;
    --border: #dddddd;
    --accent: #2b6cb0;
    --card-bg: #f7f7f8;
    --code-bg: #eef0f2;
}
@media (prefers-color-scheme: dark) {
    :root {
        --bg: #14161a;
        --fg: #e8e8e8;
        --muted: #a0a0a0;
        --border: #33363b;
        --accent: #6ea8ff;
        --card-bg: #1c1f24;
        --code-bg: #23262b;
    }
}
* { box-sizing: border-box; }
body {
    margin: 0;
    font-family: -apple-system, Segoe UI, Roboto, Helvetica, Arial, sans-serif;
    background: var(--bg);
    color: var(--fg);
    line-height: 1.5;
}
header {
    padding: 1.5rem 2rem;
    border-bottom: 1px solid var(--border);
}
header h1 { margin: 0 0 0.25rem; font-size: 1.5rem; }
.subtitle { margin: 0 0 1rem; color: var(--muted); }
#search {
    width: 100%;
    max-width: 32rem;
    padding: 0.6rem 0.9rem;
    font-size: 1rem;
    border: 1px solid var(--border);
    border-radius: 0.4rem;
    background: var(--card-bg);
    color: var(--fg);
}
#toc {
    padding: 1rem 2rem;
    border-bottom: 1px solid var(--border);
}
#toc ul { list-style: none; margin: 0; padding: 0; display: flex; flex-wrap: wrap; gap: 1rem; }
#toc a { color: var(--accent); text-decoration: none; font-weight: 600; }
#toc a:hover { text-decoration: underline; }
main { padding: 1rem 2rem 3rem; max-width: 60rem; margin: 0 auto; }
section.category { margin-top: 2rem; }
section.category h2 {
    border-bottom: 2px solid var(--accent);
    padding-bottom: 0.3rem;
}
.card {
    background: var(--card-bg);
    border: 1px solid var(--border);
    border-radius: 0.5rem;
    padding: 1rem 1.25rem;
    margin-bottom: 1rem;
}
.card h3 { margin: 0 0 0.5rem; font-size: 1.05rem; }
.card .description { margin: 0 0 0.5rem; }
.examples { margin: 0.5rem 0 0; padding-left: 1.25rem; }
code {
    background: var(--code-bg);
    padding: 0.1rem 0.35rem;
    border-radius: 0.25rem;
    font-family: SFMono-Regular, Consolas, Menlo, monospace;
    font-size: 0.92em;
}
table { width: 100%; border-collapse: collapse; margin-top: 1rem; }
th, td {
    border-bottom: 1px solid var(--border);
    padding: 0.5rem 0.6rem;
    text-align: left;
    vertical-align: top;
}
th { color: var(--muted); font-weight: 600; }
.hidden { display: none !important; }
";

    private const string Js = @"
(function () {
    var input = document.getElementById('search');
    input.addEventListener('input', function () {
        var query = input.value.trim().toLowerCase();
        var items = document.querySelectorAll('[data-search]');
        for (var i = 0; i < items.length; i++) {
            var el = items[i];
            var matches = query === '' || el.getAttribute('data-search').indexOf(query) !== -1;
            el.classList.toggle('hidden', !matches);
        }
    });
})();
";
}
