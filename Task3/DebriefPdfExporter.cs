using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Task3;

internal static class DebriefPdfExporter
{
    public static void Write(Stream stream, string title, string body)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().Text(title).FontSize(16).SemiBold();
                page.Content().PaddingTop(14).Text(body);
            });
        }).GeneratePdf(stream);
    }
}
