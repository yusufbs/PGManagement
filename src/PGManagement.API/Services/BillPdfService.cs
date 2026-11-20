using PGManagement.Contracts.Responses.Bills;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PGManagement.API.Services;

public interface IBillPdfService
{
    byte[] GenerateBillPdf(BillResponse bill);
}

public class BillPdfService : IBillPdfService
{
    public BillPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateBillPdf(BillResponse bill)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("PAYING GUEST MANAGEMENT").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("Invoice & Dues Summary").FontSize(12).FontColor(Colors.Grey.Medium);
                        });

                        row.ConstantItem(150).Column(col =>
                        {
                            col.Item().AlignRight().Text($"Invoice #: INV-{bill.BillYear}-{bill.BillMonth:D2}-{bill.BillId:D4}").Bold();
                            col.Item().AlignRight().Text($"Date: {bill.GeneratedAt:dd MMM yyyy}");
                            col.Item().AlignRight().Text($"Due Date: {bill.DueDate:dd MMM yyyy}").FontColor(Colors.Red.Medium);
                        });
                    });

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Spacing(15);

                        // Tenant and Room Info
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Billed To:").Bold();
                                c.Item().Text(bill.TenantName).FontSize(13).Bold();
                                c.Item().Text($"Room Number: {bill.RoomNumber}");
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Billing Period:").Bold();
                                c.Item().Text($"{new DateTime(bill.BillYear, bill.BillMonth, 1):MMMM yyyy}").FontSize(13);
                                c.Item().Text($"Status: {(bill.IsPaid ? "PAID" : "UNPAID")}").Bold()
                                    .FontColor(bill.IsPaid ? Colors.Green.Medium : Colors.Red.Medium);
                            });
                        });

                        // Line items table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                                columns.ConstantColumn(120);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("#");
                                header.Cell().Element(HeaderStyle).Text("Description");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Amount (INR)");

                                static IContainer HeaderStyle(IContainer container) =>
                                    container.Background(Colors.Grey.Lighten3).Padding(5).DefaultTextStyle(x => x.Bold());
                            });

                            table.Cell().Element(CellStyle).Text("1");
                            table.Cell().Element(CellStyle).Text("Monthly Room Rent");
                            table.Cell().Element(CellStyle).AlignRight().Text($"Rs. {bill.RentAmount:N2}");

                            table.Cell().Element(CellStyle).Text("2");
                            table.Cell().Element(CellStyle).Text("Electricity Charges");
                            table.Cell().Element(CellStyle).AlignRight().Text($"Rs. {bill.ElectricityCharges:N2}");

                            table.Cell().Element(CellStyle).Text("3");
                            table.Cell().Element(CellStyle).Text("Water Charges");
                            table.Cell().Element(CellStyle).AlignRight().Text($"Rs. {bill.WaterCharges:N2}");

                            if (bill.OtherCharges > 0)
                            {
                                table.Cell().Element(CellStyle).Text("4");
                                table.Cell().Element(CellStyle).Text("Other Operational Charges");
                                table.Cell().Element(CellStyle).AlignRight().Text($"Rs. {bill.OtherCharges:N2}");
                            }

                            if (bill.Discount > 0)
                            {
                                table.Cell().Element(CellStyle).Text("-");
                                table.Cell().Element(CellStyle).Text("Discount / Concession");
                                table.Cell().Element(CellStyle).AlignRight().Text($"-Rs. {bill.Discount:N2}");
                            }

                            static IContainer CellStyle(IContainer container) =>
                                container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                        });

                        // Summary
                        col.Item().AlignRight().Column(c =>
                        {
                            c.Spacing(5);
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(150).Text("Total Amount:").Bold();
                                r.ConstantItem(120).AlignRight().Text($"Rs. {bill.TotalAmount:N2}").Bold();
                            });
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(150).Text("Amount Paid:").FontColor(Colors.Green.Medium);
                                r.ConstantItem(120).AlignRight().Text($"Rs. {bill.PaidAmount:N2}").FontColor(Colors.Green.Medium);
                            });
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(150).Text("Balance Due:").Bold().FontColor(Colors.Red.Medium);
                                r.ConstantItem(120).AlignRight().Text($"Rs. {bill.BalanceDue:N2}").Bold().FontColor(Colors.Red.Medium);
                            });
                        });

                        if (!string.IsNullOrWhiteSpace(bill.Notes))
                        {
                            col.Item().Text($"Notes: {bill.Notes}").Italic().FontSize(10);
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("Thank you for staying with us. For payment queries, please reach out to the management.")
                    .FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });

        return document.GeneratePdf();
    }
}
