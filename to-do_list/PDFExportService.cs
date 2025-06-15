using System.Diagnostics;
using WPF_Projekt;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;

namespace to_do_list
{
    internal class PDFExportService
    {
        public static void ExportTasksToPdf(List<TaskItem> tasks, string filename = "ListaZadan.pdf")
        {
            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new CustomFontResolver();
            }


            PdfDocument document = new PdfDocument();
            document.Info.Title = Lang.L("pdf_title");

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Używamy Arial zamiast Times New Roman
            XFont headerFont = new XFont("Arial", 14, XFontStyle.Bold);
            XFont taskFont = new XFont("Arial", 10, XFontStyle.Regular);
            XFont subtaskFont = new XFont("Arial", 9, XFontStyle.Italic);


            int y = 40;
            gfx.DrawString(Lang.L("pdf_title"), headerFont, XBrushes.Black, 40, y);
            y += 30;

            foreach (var task in tasks)
            {
                if (y > page.Height - 80)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }

                string taskLine = $"{(task.Completed ? "[✔]" : "[✖]")} {task.Title} ({task.Priority}) - {task.Category.Name}";
                if (task.Deadline.HasValue)
                    taskLine += $" | {Lang.L("pdf_deadline")}: {task.Deadline.Value:dd.MM.yyyy}";

                gfx.DrawString(taskLine, taskFont, XBrushes.Black, new XRect(40, y, page.Width - 80, page.Height), XStringFormats.TopLeft);
                y += 20;

                if (!string.IsNullOrWhiteSpace(task.Description))
                {
                    gfx.DrawString($"    {Lang.L("pdf_description")}: {task.Description}", taskFont, XBrushes.Gray, new XRect(40, y, page.Width - 80, page.Height), XStringFormats.TopLeft);
                    y += 20;
                }

                foreach (var sub in task.Subtasks)
                {
                    string subLine = $"    └ {(sub.Completed ? "[✔]" : "[✖]")} {sub.Title}";
                    gfx.DrawString(subLine, subtaskFont, XBrushes.DarkGray, new XRect(40, y, page.Width - 80, page.Height), XStringFormats.TopLeft);
                    y += 18;
                }

                y += 10;
            }

            document.Save(filename);
            Process.Start("explorer", filename); // lub użyj `cmd /C start "" "filename"` dla niektórych systemów
        }
    }
}
