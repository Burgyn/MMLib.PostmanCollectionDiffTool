using DiffPlex.DiffBuilder.Model;
using MMLib.PostmanCollectionDiff.Comparer;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Reflection;
using System.Text;

namespace MMLib.PostmanCollectionDiff.ConsoleReporter;

public class DiffReporter : IDiffReporter
{
    private const string Original = "Original";
    private const string Updated = "Updated";
    private static Color ModifieldColor => Color.LightYellow3;
    private static Color InsertedColor => Color.Green;
    private static Color DeletedColor => Color.Red;

    public bool Verbose { get; set; }

    public bool CanPrintLogo { get; set; } = true;

    public Task GenerateReportAsync(CollectionDiffResult result)
    {
        PrintLogo();
        PrintInfo(result);

        if (Verbose)
        {
            PrintDetailedReport(result);
        }

        PrintSummaryReport(result);

        return Task.CompletedTask;
    }

    private static void PrintDetailedReport(CollectionDiffResult result)
    {
        AnsiConsole.Write(new Rule(""));
        foreach (ItemDiff item in result.Items.OrderBy(i => i.ChangeType))
        {
            PrintRequest(item);
        }
        AnsiConsole.Write(new Rule(""));
    }

    private static void PrintSummaryReport(CollectionDiffResult result)
    {
        PrintGraph(result);
        PrintItems(result.Items.Where(i => i.ChangeType == ChangeType.Inserted), InsertedColor, "+ New created");
        PrintItems(result.Items.Where(i => i.ChangeType == ChangeType.Deleted), DeletedColor, "- Removed");
        PrintItems(result.Items.Where(i => i.ChangeType == ChangeType.Modified), ModifieldColor, "~ Modified");
    }

    private void PrintLogo()
    {
        if (!CanPrintLogo)
        {
            return;
        }
        Version version = Assembly.GetAssembly(typeof(DiffReporter))!.GetName().Version!;
        AnsiConsole.Write(
            new FigletText($"MMLib.PMDiff {version.Major}.{version.Minor}.{version.Revision}")
                .Centered()
                .Color(Color.Blue));
    }

    private void PrintInfo(CollectionDiffResult result)
    {
        if (result.OriginalName.Equals(result.UpdatedName))
        {
            AnsiConsole.Write(Panel(result.OriginalName));
        }
        else
        {
            AnsiConsole.Write(CreateGrid().AddRow(
                Panel(result.OriginalName, Original, ModifieldColor),
                Panel(result.UpdatedName, Updated, ModifieldColor)));
        }
        if (Verbose)
        {
            PrintVariables(result);
            PrintEvents(result);
        }
    }

    private static void PrintEvents(CollectionDiffResult result)
    {
        var grid = CreateGrid();
        PrintEvents(grid, result.Events);

        AnsiConsole.Write(grid);
    }

    private static void PrintEvents(Grid grid, IEnumerable<EventDiff> events)
    {
        foreach (var @event in events.Where(e => e.HasDifferences))
        {
            if (@event.HasDifferences && @event.Script.HasDifferences)
            {
                grid.AddRow(
                    RenderEvent(@event, DiffValueType.Original, $"Event - {@event.Listen} - original"),
                    RenderEvent(@event, DiffValueType.Updated, $"Event - {@event.Listen} - updated"));
            }
        }
    }

    private static Panel RenderEvent(EventDiff @event, DiffValueType valueType, string title)
    {
        var lines = @event.Script.Exec.GetDiff(valueType).Lines;

        return RenderLines(title, lines);
    }

    private static Panel RenderLines(string title, List<DiffPiece> lines)
    {
        var sb = new StringBuilder();
        var isNotFirstCall = false;

        foreach (var line in lines.Where(l => l.Text is not null))
        {
            if (isNotFirstCall)
            {
                sb.AppendLine();
            }
            isNotFirstCall = true;
            sb.Append('[').Append(GetColor(line.Type).ToString()).Append(']');
            sb.Append(line.Text.Replace("[", "[[").Replace("]", "]]")).Append("[/]");
        }

        return Panel(new Markup(sb.ToString()), title);
    }

    private static void PrintVariables(CollectionDiffResult result)
    {
        if (result.Variables.HasDifferences)
        {
            var grid = CreateGrid();
            grid.AddRow(
                RenderVariables(result.Variables, DiffValueType.Original, "Variables - original"),
                RenderVariables(result.Variables, DiffValueType.Updated, "Variables - updated"));
            AnsiConsole.Write(grid);
        }
    }

    private static void PrintGraph(CollectionDiffResult result)
    {
        int newAdded = 0;
        int deleted = 0;
        int modified = 0;
        foreach (var item in result.Items)
        {
            switch (item.ChangeType)
            {
                case ChangeType.Inserted:
                    newAdded++;
                    break;
                case ChangeType.Deleted:
                    deleted++;
                    break;
                case ChangeType.Modified:
                    modified++;
                    break;
            }
        }

        var chart = new BreakdownChart()
            .FullSize()
            .ShowTagValues()
            .AddItem("+ New created", newAdded, InsertedColor)
            .AddItem("- Removed", deleted, DeletedColor)
            .AddItem("~ Modified", modified, ModifieldColor)
            .AddItem("Unchanged", result.UnchangedItemCount, Color.White);

        AnsiConsole.Write(Panel(chart));
    }

    private static Panel Panel(string text, string? type = null, Color? color = null)
        => Panel(new Markup(text), type, color);

    private static Panel Panel(IRenderable renderable, string? type = null, Color? color = null)
    {
        var panel = new Panel(renderable)
        {
            Border = BoxBorder.Rounded,
            Expand = true
        };

        if (type != null)
        {
            panel.Header = new PanelHeader(type);
        }

        if (color != null)
        {
            panel.BorderColor(color.Value);
        }

        return panel;
    }

    private static void PrintItems(IEnumerable<ItemDiff> diffs, Color color, string header)
    {
        var table = new Table
        {
            ShowHeaders = false,
            Title = new TableTitle(header, new Style(color))
        };

        table.AddColumn("Icon");
        table.AddColumn("Method");
        table.AddColumn("Name");

        table.BorderColor(color);

        var leafStyle = new Style(color);
        var pathStyle = new Style(Color.DarkSlateGray3);
        var style = new Style(color);

        foreach (ItemDiff diff in diffs)
        {
            var icon = diff.IsFolder ? new Markup("📂") : new Markup("🔥");
            var method = diff.IsFolder ? Text.Empty : new Text(diff.Request.Method.Updated ?? string.Empty, style);
            var path = new TextPath(diff.FullName!) { StemStyle = pathStyle, LeafStyle = leafStyle };
            table.AddRow(icon, method, path);
        }

        AnsiConsole.Write(table);
    }

    private static void PrintRequest(ItemDiff item)
    {
        Color color = GetColor(item);
        var grid = new Grid();
        grid.AddColumn();

        var style = new Style(color);
        var pathStyle = new Style(Color.DarkSlateGray3);

        AddRequestName(item, grid, style, pathStyle);
        if (!item.IsFolder)
        {
            grid.AddRow(new Rule());
            AddRequestUrl(item.Request, grid);
            AddHeaders(item.Request.Headers, grid);
            AddBody(item.Request.Body, grid);
        }

        var eventsGrid = CreateGrid();
        PrintEvents(eventsGrid, item.Events);
        grid.AddRow(eventsGrid);

        AnsiConsole.Write(Panel(grid, $"─ {ChangeTypeToString(item.ChangeType)} {Escape(item.Name)} ─", color: color));
    }

    private static string Escape(string text)
        => text.Replace("[", "[[").Replace("]", "]]");

    private static string ChangeTypeToString(ChangeType type)
        => type switch
        {
            ChangeType.Inserted => "+",
            ChangeType.Deleted => "-",
            ChangeType.Modified => "~",
            _ => string.Empty
        };

    private static void AddBody(BodyDiff body, Grid grid)
    {
        if (body.HasDifferences)
        {
            var layout = CreateGrid();
            layout.AddRow(
                RenderBody(body, DiffValueType.Original, "Body - original"),
                RenderBody(body, DiffValueType.Updated, "Body - updated"));
            grid.AddRow(layout);
        }
        else if (!string.IsNullOrEmpty(body.Raw.Raw))
        {
            grid.AddRow(RenderBody(body, DiffValueType.Updated, "Body"));
        }
    }

    private static Panel RenderBody(BodyDiff body, DiffValueType valueType, string title)
    {
        var lines = body.Raw.GetDiff(valueType).Lines;

        return RenderLines(title, lines);
    }

    private static void AddHeaders(HeadersDiff headers, Grid grid)
    {
        if (headers.Any())
        {
            if (headers.HasDifferences)
            {
                var layout = CreateGrid();
                layout.AddRow(
                    RenderHeader(headers, DiffValueType.Original, "Headers - original"),
                    RenderHeader(headers, DiffValueType.Updated, "Headers - updated"));
                grid.AddRow(layout);
            }
            else
            {
                grid.AddRow(RenderHeader(headers, DiffValueType.Updated, "Headers"));
            }
        }
    }

    private static Panel RenderHeader(HeadersDiff headers, DiffValueType valueType, string title)
    {
        var headersGrid = new Grid() { Expand = true };
        headersGrid.AddColumn().AddColumn();
        foreach (var header in headers)
        {
            var style = GetStyle(header.ChangeType);
            headersGrid.AddRow(new Text(header.Key, style), new Text(header.Value.GetValue(valueType), style));
        }

        return Panel(headersGrid, title);
    }

    private static Panel RenderVariables(VariablesDiff variables, DiffValueType valueType, string title)
    {
        var headersGrid = new Grid() { Expand = true };
        headersGrid.AddColumn().AddColumn();
        foreach (var variable in variables)
        {
            var style = GetStyle(variable.ChangeType);
            headersGrid.AddRow(new Text(variable.Key, style), new Text(variable.Value.GetValue(valueType), style));
        }

        return Panel(headersGrid, title);
    }

    private static void AddRequestUrl(RequestDiff request, Grid grid)
    {
        if (request.Url.HasDifferences)
        {
            var layout = CreateGrid();
            layout.AddRow(
                RenderUrl(request, DiffValueType.Original, "Url - original"),
                RenderUrl(request, DiffValueType.Updated, "Url - updated"));
            grid.AddRow(layout);
        }
        else
        {
            grid.AddRow(RenderUrl(request, DiffValueType.Updated, "Url"));
        }
    }

    private static void AddRequestName(ItemDiff item, Grid grid, Style style, Style pathStyle)
        => grid.AddRow(new TextPath(item.FullName) { StemStyle = pathStyle, LeafStyle = style });

    private static IRenderable RenderUrl(RequestDiff request, DiffValueType valueType, string title)
        => Panel(new Text($"{request.Method.GetValue(valueType)} {request.Url.GetValue(valueType)}"), title);

    private static Color GetColor(ItemDiff item)
        => GetColor(item.ChangeType);

    private static Color GetColor(ChangeType changeType)
        => changeType switch
        {
            ChangeType.Inserted => InsertedColor,
            ChangeType.Deleted => DeletedColor,
            ChangeType.Modified => ModifieldColor,
            _ => Color.White
        };

    private static Style GetStyle(ChangeType changeType)
        => new(GetColor(changeType));

    private static Grid CreateGrid()
    {
        int consoleWidth = Console.WindowWidth;

        var grid = new Grid()
            .AddColumns(new GridColumn().Width(consoleWidth / 2), new GridColumn().Width(consoleWidth / 2));
        return grid.Expand();
    }
}
