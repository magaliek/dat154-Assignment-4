using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Task3;

internal static class DebriefPdfExporter
{
    public static void Write(Stream stream, string title, string body)
    {
        var normalized = body.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).LineHeight(1.35f));
                page.Header().Text(title).FontSize(16).SemiBold();
                page.Content().PaddingTop(14).Column(column =>
                {
                    column.Spacing(2);
                    foreach (var line in lines)
                    {
                        if (line.Length == 0)
                            column.Item().Height(8);
                        else
                            column.Item().Text(line);
                    }
                });
            });
        }).GeneratePdf(stream);
    }
}
