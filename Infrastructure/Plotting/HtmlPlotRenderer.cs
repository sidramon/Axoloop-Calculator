namespace Infrastructure.Plotting;

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Application.Calculator.Plotting;
using Domain.Calculator.Algorithms;
using Domain.Calculator.Plotting;

/// <summary>
/// Renders a <see cref="PlotSeries"/> as a self-contained interactive HTML page
/// (inline CSS/JS, no CDN, no external dependency). The pre-sampled points, zeros and
/// extrema are serialized as JSON in the document; hover, zoom, pan and the notable-point
/// panel are plain client-side JavaScript working from that static data — the page can
/// never re-evaluate the function.
/// </summary>
public sealed class HtmlPlotRenderer : IPlotRenderer
{
    public PlotFormat Format => PlotFormat.Html;

    public string Render(PlotSeries series, PlotOptions options)
    {
        var maxima = series.Extrema
            .Where(e => e.Kind == ExtremumKind.Maximum)
            .Select(e => new ExtremumPointPayload(e.X, e.Y))
            .ToList();
        var minima = series.Extrema
            .Where(e => e.Kind == ExtremumKind.Minimum)
            .Select(e => new ExtremumPointPayload(e.X, e.Y))
            .ToList();

        var payload = new PlotPayload(
            series.FunctionName,
            series.Points.Select(p => new PlotPointPayload(p.X, p.Y)).ToList(),
            series.Zeros.ToList(),
            maxima,
            minima,
            new BoundsPayload(series.XMin, series.XMax, series.YMin, series.YMax),
            new BoundsPayload(
                options.VisibleXMin,
                options.VisibleXMax,
                options.VisibleYMin ?? series.YMin,
                options.VisibleYMax ?? series.YMax),
            options.ShowZeros,
            options.ShowGrid);

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        var html = new StringBuilder();

        html.Append("<!doctype html>\n<html lang=\"en\">\n<head>\n<meta charset=\"utf-8\">\n");
        html.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n");
        html.Append($"<title>Plot &mdash; {Encode(series.FunctionName)}</title>\n");
        html.Append("<style>\n").Append(Css).Append("\n</style>\n");
        html.Append("</head>\n<body>\n");

        html.Append("<header>\n");
        html.Append($"<h1>{Encode(series.FunctionName)}(x)</h1>\n");
        html.Append("<p class=\"legend\">\n");
        html.Append("<span class=\"legend-item\"><span class=\"legend-swatch legend-zero\"></span>Zero</span>\n");
        html.Append("<span class=\"legend-item\"><span class=\"legend-swatch legend-max\"></span>Maximum</span>\n");
        html.Append("<span class=\"legend-item\"><span class=\"legend-swatch legend-min\"></span>Minimum</span>\n");
        html.Append("</p>\n");
        html.Append("<p class=\"hint\">Drag to pan &middot; wheel to zoom &middot; shift-drag to box-zoom</p>\n");
        html.Append("</header>\n");

        html.Append("<main>\n");
        html.Append("<div id=\"plot-container\">\n");
        html.Append("<svg id=\"plot-svg\"></svg>\n");
        html.Append("<div id=\"tooltip\" class=\"hidden\"></div>\n");
        html.Append("<div id=\"range-warning\" class=\"hidden\">" +
                     "Beyond the pre-computed domain — no more data here.</div>\n");
        html.Append("</div>\n");

        html.Append("<aside id=\"notable-panel\">\n");
        html.Append(RenderSection("zeros", "Zeros", series.Zeros.Count));
        html.Append(RenderSection("maxima", "Maxima", maxima.Count));
        html.Append(RenderSection("minima", "Minima", minima.Count));
        html.Append("</aside>\n");
        html.Append("</main>\n");

        html.Append("<p class=\"note\">Zoom redraws from pre-computed samples only — this page cannot " +
                     "re-evaluate the function. Beyond the computed domain below, there is no more data, and " +
                     "very deep zoom will look increasingly angular rather than smooth.</p>\n");
        html.Append($"<p class=\"computed-domain\">Computed domain: x &isin; [{FormatNumber(series.XMin)}, " +
                     $"{FormatNumber(series.XMax)}]</p>\n");

        html.Append($"<script type=\"application/json\" id=\"plot-data\">{json}</script>\n");
        html.Append("<script>\n").Append(Js).Append("\n</script>\n");
        html.Append("</body>\n</html>\n");

        return html.ToString();
    }

    /// <summary>
    /// A native, closed-by-default &lt;details&gt; section for a non-empty category, or a
    /// grayed, non-expandable placeholder when it's empty — the absence of extrema (or
    /// zeros) is itself information, not something to hide.
    /// </summary>
    private static string RenderSection(string id, string label, int count)
    {
        if (count == 0)
            return $"<div class=\"notable-section empty\"><div class=\"notable-summary\">" +
                   $"{Encode(label)} (0)</div></div>\n";

        return $"<details class=\"notable-section\"><summary>{Encode(label)} ({count})</summary>" +
               $"<ul id=\"{id}-list\" class=\"scrollable\"></ul></details>\n";
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private static string FormatNumber(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    private sealed record PlotPayload(
        string FunctionName,
        List<PlotPointPayload> Points,
        List<double> Zeros,
        List<ExtremumPointPayload> Maxima,
        List<ExtremumPointPayload> Minima,
        BoundsPayload Domain,
        BoundsPayload Visible,
        bool ShowZeros,
        bool ShowGrid);

    private sealed record PlotPointPayload(double X, double? Y);

    private sealed record ExtremumPointPayload(double X, double Y);

    private sealed record BoundsPayload(double XMin, double XMax, double YMin, double YMax);

    private const string Css = @"
:root {
    color-scheme: light dark;
    --bg: #ffffff;
    --fg: #1a1a1a;
    --muted: #5a5a5a;
    --border: #dddddd;
    --accent: #2b6cb0;
    --curve: #2b6cb0;
    --zero: #d9822b;
    --max-marker: #2e9e5b;
    --min-marker: #d64545;
    --card-bg: #f7f7f8;
    --warn-bg: #fff3cd;
    --warn-fg: #7a5b00;
}
@media (prefers-color-scheme: dark) {
    :root {
        --bg: #14161a;
        --fg: #e8e8e8;
        --muted: #a0a0a0;
        --border: #33363b;
        --accent: #6ea8ff;
        --curve: #6ea8ff;
        --zero: #f0a94e;
        --max-marker: #4cc37f;
        --min-marker: #ea6b6b;
        --card-bg: #1c1f24;
        --warn-bg: #4a3b00;
        --warn-fg: #f0d27a;
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
header { padding: 1rem 1.5rem; border-bottom: 1px solid var(--border); }
header h1 { margin: 0 0 0.5rem; font-size: 1.3rem; font-family: SFMono-Regular, Consolas, Menlo, monospace; }
.legend { margin: 0 0 0.4rem; display: flex; gap: 1.1rem; font-size: 0.78rem; color: var(--muted); }
.legend-item { display: inline-flex; align-items: center; gap: 0.35rem; }
.legend-swatch { display: inline-block; width: 10px; height: 10px; }
.legend-zero { border-radius: 50%; background: var(--zero); }
.legend-max { background: var(--max-marker); clip-path: polygon(50% 0%, 0% 100%, 100% 100%); }
.legend-min { background: var(--min-marker); clip-path: polygon(50% 100%, 0% 0%, 100% 0%); }
.hint { margin: 0; font-size: 0.78rem; color: var(--muted); }
main { display: flex; gap: 1rem; padding: 1rem 1.5rem; align-items: flex-start; }
#plot-container { position: relative; flex: 1 1 auto; min-width: 0; }
#plot-svg {
    width: 100%;
    height: 60vh;
    display: block;
    background: var(--card-bg);
    border: 1px solid var(--border);
    border-radius: 0.4rem;
    cursor: crosshair;
    touch-action: none;
    user-select: none;
}
.axis-line { stroke: var(--muted); stroke-width: 1; }
.grid-line { stroke: var(--border); stroke-width: 1; }
.curve-path { stroke: var(--curve); stroke-width: 1.6; fill: none; }
.zero-marker { fill: var(--zero); cursor: pointer; }
.max-marker { fill: var(--max-marker); cursor: pointer; }
.min-marker { fill: var(--min-marker); cursor: pointer; }
.crosshair { stroke: var(--muted); stroke-width: 1; stroke-dasharray: 3 3; pointer-events: none; }
.selection-rect { fill: rgba(43,108,176,0.15); stroke: var(--accent); stroke-width: 1; }
.axis-label { fill: var(--muted); font-size: 11px; font-family: SFMono-Regular, Consolas, Menlo, monospace; }
#tooltip {
    position: absolute;
    pointer-events: none;
    background: var(--card-bg);
    border: 1px solid var(--border);
    border-radius: 0.3rem;
    padding: 0.25rem 0.5rem;
    font-size: 0.8rem;
    font-family: SFMono-Regular, Consolas, Menlo, monospace;
    transform: translate(-50%, -120%);
}
#range-warning {
    position: absolute;
    top: 0.5rem;
    left: 50%;
    transform: translateX(-50%);
    background: var(--warn-bg);
    color: var(--warn-fg);
    padding: 0.3rem 0.7rem;
    border-radius: 0.3rem;
    font-size: 0.8rem;
}
.hidden { display: none !important; }
#notable-panel {
    flex: 0 0 13rem;
    display: flex;
    flex-direction: column;
    gap: 0.6rem;
    max-height: 60vh;
    overflow-y: auto;
}
.notable-section { border: 1px solid var(--border); border-radius: 0.3rem; }
.notable-section summary {
    cursor: pointer;
    padding: 0.4rem 0.6rem;
    font-size: 0.85rem;
    font-weight: 600;
    list-style: none;
}
.notable-section summary::-webkit-details-marker { display: none; }
.notable-section summary::before {
    content: '\25B8';
    display: inline-block;
    margin-right: 0.4rem;
    transition: transform 0.1s;
}
.notable-section[open] summary::before { transform: rotate(90deg); }
.notable-section .notable-summary { padding: 0.4rem 0.6rem; font-size: 0.85rem; font-weight: 600; }
.notable-section.empty { opacity: 0.5; }
.notable-section ul.scrollable {
    list-style: none;
    margin: 0;
    padding: 0 0.6rem 0.6rem;
    max-height: 240px;
    overflow-y: auto;
}
.notable-section ul.scrollable li {
    padding: 0.3rem 0.5rem;
    border: 1px solid var(--border);
    border-radius: 0.3rem;
    margin-top: 0.4rem;
    cursor: pointer;
    font-family: SFMono-Regular, Consolas, Menlo, monospace;
    font-size: 0.85rem;
}
.notable-section ul.scrollable li:hover { border-color: var(--accent); color: var(--accent); }
.note, .computed-domain { margin: 0 1.5rem 0.3rem; font-size: 0.78rem; color: var(--muted); max-width: 60rem; }
";

    private const string Js = @"
(function () {
    var data = JSON.parse(document.getElementById('plot-data').textContent);
    var svg = document.getElementById('plot-svg');
    var svgNS = 'http://www.w3.org/2000/svg';
    var tooltip = document.getElementById('tooltip');
    var warning = document.getElementById('range-warning');
    var container = document.getElementById('plot-container');

    var initialView = {
        xMin: data.visible.xMin, xMax: data.visible.xMax,
        yMin: data.visible.yMin, yMax: data.visible.yMax
    };
    var view = Object.assign({}, initialView);

    function svgWidth() { return svg.clientWidth || 600; }
    function svgHeight() { return svg.clientHeight || 400; }

    function toPx(x) { return (x - view.xMin) / (view.xMax - view.xMin) * svgWidth(); }
    function toPy(y) { return svgHeight() - (y - view.yMin) / (view.yMax - view.yMin) * svgHeight(); }
    function toDataX(px) { return view.xMin + px / svgWidth() * (view.xMax - view.xMin); }
    function toDataY(py) { return view.yMin + (svgHeight() - py) / svgHeight() * (view.yMax - view.yMin); }

    function niceStep(range, targetTicks) {
        var rough = range / Math.max(targetTicks, 1);
        var mag = Math.pow(10, Math.floor(Math.log(rough) / Math.LN10));
        var norm = rough / mag;
        var step;
        if (norm < 1.5) step = 1;
        else if (norm < 3) step = 2;
        else if (norm < 7) step = 5;
        else step = 10;
        return step * mag;
    }

    function formatNumber(n) {
        var r = Math.round(n * 10000) / 10000;
        return String(r);
    }

    function clearSvg() {
        while (svg.firstChild) svg.removeChild(svg.firstChild);
    }

    function addEl(name, attrs) {
        var el = document.createElementNS(svgNS, name);
        for (var key in attrs) el.setAttribute(key, attrs[key]);
        svg.appendChild(el);
        return el;
    }

    function drawGrid() {
        if (!data.showGrid) return;

        var xStep = niceStep(view.xMax - view.xMin, 8);
        var yStep = niceStep(view.yMax - view.yMin, 6);

        for (var x = Math.ceil(view.xMin / xStep) * xStep; x <= view.xMax; x += xStep) {
            var px = toPx(x);
            addEl('line', { x1: px, y1: 0, x2: px, y2: svgHeight(), class: 'grid-line' });
            addEl('text', { x: px + 3, y: svgHeight() - 4, class: 'axis-label' }).textContent = formatNumber(x);
        }
        for (var y = Math.ceil(view.yMin / yStep) * yStep; y <= view.yMax; y += yStep) {
            var py = toPy(y);
            addEl('line', { x1: 0, y1: py, x2: svgWidth(), y2: py, class: 'grid-line' });
            addEl('text', { x: 3, y: py - 3, class: 'axis-label' }).textContent = formatNumber(y);
        }

        if (view.xMin <= 0 && 0 <= view.xMax) {
            var zx = toPx(0);
            addEl('line', { x1: zx, y1: 0, x2: zx, y2: svgHeight(), class: 'axis-line' });
        }
        if (view.yMin <= 0 && 0 <= view.yMax) {
            var zy = toPy(0);
            addEl('line', { x1: 0, y1: zy, x2: svgWidth(), y2: zy, class: 'axis-line' });
        }
    }

    function drawCurve() {
        var d = '';
        var drawing = false;

        for (var i = 0; i < data.points.length; i++) {
            var p = data.points[i];
            if (p.y === null || p.y === undefined) { drawing = false; continue; }

            var px = toPx(p.x), py = toPy(p.y);
            d += (drawing ? ' L ' : ' M ') + px.toFixed(2) + ',' + py.toFixed(2);
            drawing = true;
        }

        if (d) addEl('path', { d: d.trim(), class: 'curve-path' });
    }

    function drawZeros() {
        if (!data.showZeros) return;
        data.zeros.forEach(function (zx) {
            if (zx < view.xMin || zx > view.xMax) return;
            var zy = (view.yMin <= 0 && 0 <= view.yMax) ? toPy(0) : svgHeight();
            var el = addEl('circle', { cx: toPx(zx), cy: zy, r: 4, class: 'zero-marker' });
            el.addEventListener('click', function () { showPointInfo(zx, 0); });
        });
    }

    function triangleUpPoints(cx, cy, size) {
        return cx + ',' + (cy - size) + ' ' + (cx - size) + ',' + (cy + size) + ' ' + (cx + size) + ',' + (cy + size);
    }

    function triangleDownPoints(cx, cy, size) {
        return cx + ',' + (cy + size) + ' ' + (cx - size) + ',' + (cy - size) + ' ' + (cx + size) + ',' + (cy - size);
    }

    function drawExtrema() {
        data.maxima.forEach(function (p) {
            if (p.x < view.xMin || p.x > view.xMax) return;
            var el = addEl('polygon', { points: triangleUpPoints(toPx(p.x), toPy(p.y), 5), class: 'max-marker' });
            el.addEventListener('click', function () { showPointInfo(p.x, p.y); });
        });
        data.minima.forEach(function (p) {
            if (p.x < view.xMin || p.x > view.xMax) return;
            var el = addEl('polygon', { points: triangleDownPoints(toPx(p.x), toPy(p.y), 5), class: 'min-marker' });
            el.addEventListener('click', function () { showPointInfo(p.x, p.y); });
        });
    }

    function updateWarning() {
        var outOfRange = view.xMin < data.domain.xMin - 1e-9 || view.xMax > data.domain.xMax + 1e-9;
        warning.classList.toggle('hidden', !outOfRange);
    }

    function redraw() {
        clearSvg();
        drawGrid();
        drawCurve();
        drawZeros();
        drawExtrema();
        updateWarning();
    }

    function recenterOn(x) {
        var span = (initialView.xMax - initialView.xMin) / 4;
        view.xMin = x - span;
        view.xMax = x + span;
        redraw();
    }

    function renderList(listId, items, formatItem, x) {
        var list = document.getElementById(listId);
        if (!list) return; // rendered as an empty, non-expandable placeholder server-side
        items.forEach(function (item) {
            var li = document.createElement('li');
            li.textContent = formatItem(item);
            li.addEventListener('click', function () { recenterOn(x(item)); });
            list.appendChild(li);
        });
    }

    function renderNotablePanel() {
        renderList('zeros-list', data.zeros, function (z) { return 'x = ' + formatNumber(z); }, function (z) { return z; });
        renderList('maxima-list', data.maxima,
            function (p) { return 'x = ' + formatNumber(p.x) + ', y = ' + formatNumber(p.y); },
            function (p) { return p.x; });
        renderList('minima-list', data.minima,
            function (p) { return 'x = ' + formatNumber(p.x) + ', y = ' + formatNumber(p.y); },
            function (p) { return p.x; });
    }

    // Hover / click: crosshair + coordinates (interpolated linearly between samples for
    // hover; exact for a clicked marker).
    function findBracket(x) {
        var pts = data.points;
        if (!pts.length || x < pts[0].x || x > pts[pts.length - 1].x) return null;

        var lo = 0, hi = pts.length - 1;
        while (lo < hi) {
            var mid = (lo + hi) >> 1;
            if (pts[mid].x < x) lo = mid + 1; else hi = mid;
        }
        if (lo === 0) return pts[0].y === null ? null : { x: pts[0].x, y: pts[0].y };

        var a = pts[lo - 1], b = pts[lo];
        if (a.y === null || b.y === null || a.y === undefined || b.y === undefined) return null;
        if (b.x === a.x) return { x: a.x, y: a.y };

        var t = (x - a.x) / (b.x - a.x);
        return { x: x, y: a.y + (b.y - a.y) * t };
    }

    function showPointInfo(x, y) {
        var px = toPx(x), py = toPy(y);

        var existing = document.getElementById('crosshair-line');
        if (existing) existing.remove();

        var line = document.createElementNS(svgNS, 'line');
        line.setAttribute('id', 'crosshair-line');
        line.setAttribute('class', 'crosshair');
        line.setAttribute('x1', px); line.setAttribute('x2', px);
        line.setAttribute('y1', 0); line.setAttribute('y2', svgHeight());
        svg.appendChild(line);

        tooltip.classList.remove('hidden');
        tooltip.style.left = px + 'px';
        tooltip.style.top = py + 'px';
        tooltip.textContent = 'x=' + formatNumber(x) + ', y=' + formatNumber(y);
    }

    function onHover(e) {
        var rect = svg.getBoundingClientRect();
        var px = e.clientX - rect.left, py = e.clientY - rect.top;
        if (px < 0 || py < 0 || px > rect.width || py > rect.height) { hideCrosshair(); return; }

        var interpolated = findBracket(toDataX(px));
        if (!interpolated) { hideCrosshair(); return; }

        showPointInfo(interpolated.x, interpolated.y);
    }

    function hideCrosshair() {
        var existing = document.getElementById('crosshair-line');
        if (existing) existing.remove();
        tooltip.classList.add('hidden');
    }

    // Zoom: wheel, centered on cursor.
    svg.addEventListener('wheel', function (e) {
        e.preventDefault();
        var rect = svg.getBoundingClientRect();
        var px = e.clientX - rect.left, py = e.clientY - rect.top;
        var anchorX = toDataX(px), anchorY = toDataY(py);
        var factor = e.deltaY > 0 ? 1.15 : 1 / 1.15;

        view.xMin = anchorX - (anchorX - view.xMin) * factor;
        view.xMax = anchorX + (view.xMax - anchorX) * factor;
        view.yMin = anchorY - (anchorY - view.yMin) * factor;
        view.yMax = anchorY + (view.yMax - anchorY) * factor;
        redraw();
    }, { passive: false });

    // Pan (drag) and box-zoom (shift-drag).
    var isDragging = false, dragMode = null, dragStart = null, selectionRect = null;

    svg.addEventListener('mousedown', function (e) {
        isDragging = true;
        dragMode = e.shiftKey ? 'zoom' : 'pan';
        var rect = svg.getBoundingClientRect();
        dragStart = { px: e.clientX - rect.left, py: e.clientY - rect.top, view: Object.assign({}, view) };
        if (dragMode === 'zoom') {
            selectionRect = document.createElementNS(svgNS, 'rect');
            selectionRect.setAttribute('class', 'selection-rect');
            svg.appendChild(selectionRect);
        }
    });

    window.addEventListener('mousemove', function (e) {
        if (!isDragging) { onHover(e); return; }

        var rect = svg.getBoundingClientRect();
        var px = e.clientX - rect.left, py = e.clientY - rect.top;

        if (dragMode === 'pan') {
            var startView = dragStart.view;
            var dataDX = (px - dragStart.px) / svgWidth() * (startView.xMax - startView.xMin);
            var dataDY = (py - dragStart.py) / svgHeight() * (startView.yMax - startView.yMin);
            view.xMin = startView.xMin - dataDX;
            view.xMax = startView.xMax - dataDX;
            view.yMin = startView.yMin + dataDY;
            view.yMax = startView.yMax + dataDY;
            redraw();
        } else if (dragMode === 'zoom' && selectionRect) {
            var x = Math.min(px, dragStart.px), y = Math.min(py, dragStart.py);
            var w = Math.abs(px - dragStart.px), h = Math.abs(py - dragStart.py);
            selectionRect.setAttribute('x', x);
            selectionRect.setAttribute('y', y);
            selectionRect.setAttribute('width', w);
            selectionRect.setAttribute('height', h);
        }
    });

    window.addEventListener('mouseup', function () {
        if (!isDragging) return;
        isDragging = false;

        if (dragMode === 'zoom' && selectionRect) {
            var x = parseFloat(selectionRect.getAttribute('x'));
            var y = parseFloat(selectionRect.getAttribute('y'));
            var w = parseFloat(selectionRect.getAttribute('width'));
            var h = parseFloat(selectionRect.getAttribute('height'));

            if (w > 4 && h > 4) {
                var newXMin = toDataX(x), newXMax = toDataX(x + w);
                var newYMax = toDataY(y), newYMin = toDataY(y + h);
                view.xMin = newXMin; view.xMax = newXMax;
                view.yMin = newYMin; view.yMax = newYMax;
                redraw();
            }
            selectionRect.remove();
            selectionRect = null;
        }
        dragMode = null;
    });

    renderNotablePanel();
    window.addEventListener('resize', redraw);
    redraw();
})();
";
}
