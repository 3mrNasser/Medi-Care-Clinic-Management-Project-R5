using ClosedXML.Excel;
using MediCare.Services.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MediCare.Web.Infrastructure;

// Exports the admin analytics to Excel (ClosedXML, MIT) and PDF (QuestPDF, Community license: free for
// non-commercial / small organizations; set in Program.cs).
public static class ReportExporter
{
    public static byte[] ToExcel(AnalyticsDto a)
    {
        using var book = new XLWorkbook();

        var summary = book.Worksheets.Add("Summary");
        summary.Cell(1, 1).Value = "MediCare report";
        summary.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(16);
        summary.Cell(2, 1).Value = $"{a.From:d MMM yyyy} – {a.To:d MMM yyyy}";
        var rows = new (string, object)[]
        {
            ("Patients", a.TotalPatients), ("Doctors", a.TotalDoctors), ("Approved doctors", a.ApprovedDoctors),
            ("Appointments", a.TotalAppointments), ("Completed", a.CompletedAppointments), ("Revenue (EGP)", a.TotalRevenue)
        };
        for (var i = 0; i < rows.Length; i++)
        {
            summary.Cell(4 + i, 1).Value = rows[i].Item1;
            summary.Cell(4 + i, 2).Value = XLCellValue.FromObject(rows[i].Item2);
        }
        summary.Cell(9, 2).Style.NumberFormat.Format = "#,##0";
        summary.Columns().AdjustToContents();

        var months = book.Worksheets.Add("Monthly");
        string[] head = { "Month", "Appointments", "Completed", "Cancelled / rejected", "Revenue (EGP)", "New patients" };
        for (var c = 0; c < head.Length; c++) months.Cell(1, c + 1).Value = head[c];
        for (var r = 0; r < a.Months.Count; r++)
        {
            var m = a.Months[r];
            months.Cell(r + 2, 1).Value = m.Label;
            months.Cell(r + 2, 2).Value = m.Appointments;
            months.Cell(r + 2, 3).Value = m.Completed;
            months.Cell(r + 2, 4).Value = m.Cancelled;
            months.Cell(r + 2, 5).Value = m.Revenue;
            months.Cell(r + 2, 6).Value = m.NewPatients;
        }
        if (a.Months.Count > 0)
            months.Range(1, 1, a.Months.Count + 1, head.Length).CreateTable("MonthlyTable");
        months.Columns().AdjustToContents();

        AddList(book, "Top doctors", "Doctor", "Appointments", a.TopDoctors);
        AddList(book, "Top specialties", "Specialty", "Appointments", a.TopSpecialties);
        AddList(book, "Status", "Status", "Appointments", a.StatusBreakdown);
        AddList(book, "Gender", "Gender", "Patients", a.Genders);
        AddList(book, "Age groups", "Age group", "Patients", a.AgeGroups);

        using var stream = new MemoryStream();
        book.SaveAs(stream);
        return stream.ToArray();
    }

    private static void AddList(XLWorkbook book, string sheet, string nameHeader, string valueHeader, IReadOnlyList<NameValue> items)
    {
        var ws = book.Worksheets.Add(sheet);
        ws.Cell(1, 1).Value = nameHeader;
        ws.Cell(1, 2).Value = valueHeader;
        ws.Row(1).Style.Font.SetBold();
        for (var i = 0; i < items.Count; i++)
        {
            ws.Cell(i + 2, 1).Value = items[i].Name;
            ws.Cell(i + 2, 2).Value = items[i].Value;
        }
        ws.Columns().AdjustToContents();
    }

    public static byte[] ToPdf(AnalyticsDto a)
    {
        const string navy = "#14248A";
        return Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.DefaultTextStyle(t => t.FontSize(10).FontColor("#0F172A"));

            page.Header().Column(col =>
            {
                col.Item().Text("MediCare — Clinic report").FontSize(20).Bold().FontColor(navy);
                col.Item().Text($"{a.From:d MMM yyyy} – {a.To:d MMM yyyy}   ·   generated {DateTime.Now:d MMM yyyy HH:mm}").FontColor("#64748B");
            });

            page.Content().PaddingVertical(14).Column(col =>
            {
                col.Spacing(14);

                col.Item().Row(row =>
                {
                    void Tile(string label, string value) => row.RelativeItem().Border(1).BorderColor("#E2E8F0").Padding(8).Column(c =>
                    {
                        c.Item().Text(value).FontSize(16).Bold().FontColor(navy);
                        c.Item().Text(label).FontColor("#64748B");
                    });
                    Tile("Patients", a.TotalPatients.ToString());
                    Tile("Doctors", $"{a.ApprovedDoctors}/{a.TotalDoctors}");
                    Tile("Appointments", a.TotalAppointments.ToString());
                    Tile("Completed", a.CompletedAppointments.ToString());
                    Tile("Revenue (EGP)", a.TotalRevenue.ToString("#,##0"));
                });

                col.Item().Text("Monthly appointments and revenue").FontSize(13).Bold();
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                    void H(string s) => t.Cell().Background(navy).Padding(4).Text(s).FontColor("#FFFFFF").Bold();
                    H("Month"); H("Appointments"); H("Completed"); H("Cancelled"); H("Revenue"); H("New patients");
                    foreach (var m in a.Months)
                    {
                        void C(string s) => t.Cell().BorderBottom(1).BorderColor("#E2E8F0").Padding(4).Text(s);
                        C(m.Label); C(m.Appointments.ToString()); C(m.Completed.ToString()); C(m.Cancelled.ToString());
                        C(m.Revenue.ToString("#,##0")); C(m.NewPatients.ToString());
                    }
                });

                col.Item().Row(row =>
                {
                    row.Spacing(14);
                    row.RelativeItem().Element(e => List(e, "Most popular doctors", a.TopDoctors, navy));
                    row.RelativeItem().Element(e => List(e, "Most popular specialties", a.TopSpecialties, navy));
                });
                col.Item().Row(row =>
                {
                    row.Spacing(14);
                    row.RelativeItem().Element(e => List(e, "Appointments by status", a.StatusBreakdown, navy));
                    row.RelativeItem().Element(e => List(e, "Patients by gender", a.Genders, navy));
                    row.RelativeItem().Element(e => List(e, "Patients by age", a.AgeGroups, navy));
                });
            });

            page.Footer().AlignCenter().Text(t =>
            {
                t.Span("Page ").FontColor("#64748B");
                t.CurrentPageNumber();
                t.Span(" / ").FontColor("#64748B");
                t.TotalPages();
            });
        })).GeneratePdf();
    }

    private static void List(IContainer container, string title, IReadOnlyList<NameValue> items, string navy) =>
        container.Column(c =>
        {
            c.Item().Text(title).Bold().FontColor(navy);
            if (items.Count == 0) c.Item().Text("No data").FontColor("#64748B");
            foreach (var x in items)
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text(x.Name);
                    r.ConstantItem(50).AlignRight().Text(x.Value.ToString("#,##0"));
                });
        });
}
