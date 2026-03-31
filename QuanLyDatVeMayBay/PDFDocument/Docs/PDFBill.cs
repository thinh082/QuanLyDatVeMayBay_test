using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace QuanLyDatVeMayBay.PDFDocument.Docs
{
    public class PDFBill : IDocument
    {
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(20);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                // ===== HEADER =====
                page.Header().Column(header =>
                {
                    header.Item()
                        .Background(Colors.Blue.Darken3)
                        .Padding(10)
                        .Row(row =>
                        {
                            row.RelativeItem().AlignMiddle().Column(col =>
                            {
                                col.Item().Text("VÉ MÁY BAY ĐIỆN TỬ")
                                    .Bold().FontSize(13).FontColor(Colors.White);
                                col.Item().Text("AIRLINE ELECTRONIC TICKET")
                                    .FontSize(7).FontColor(Colors.Blue.Lighten3);
                            });
                            row.ConstantItem(80).AlignRight().AlignMiddle().Column(col =>
                            {
                                col.Item().AlignRight().Text("HÓA ĐƠN")
                                    .SemiBold().FontSize(8).FontColor(Colors.White);
                                col.Item().AlignRight().Text("HD001")
                                    .Bold().FontSize(11).FontColor(Colors.Yellow.Medium);
                            });
                        });

                    header.Item()
                        .Background(Colors.Blue.Darken4)
                        .PaddingHorizontal(10)
                        .PaddingVertical(4)
                        .Row(row =>
                        {
                            row.RelativeItem().Text($"Ngày: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(8).FontColor(Colors.Blue.Lighten3);
                            row.RelativeItem().AlignRight().Text("Trạng thái: Đã đặt")
                                .FontSize(8).FontColor(Colors.Green.Lighten2);
                        });
                });

                // ===== CONTENT =====
                page.Content().PaddingTop(8).Column(col =>
                {
                    col.Spacing(6);

                    // --- Thông tin hành khách ---
                    col.Item().Background(Colors.Grey.Lighten4).Padding(6).Column(section =>
                    {
                        section.Item().Text("THÔNG TIN HÀNH KHÁCH")
                            .Bold().FontSize(8).FontColor(Colors.Blue.Darken2);
                        section.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                            });
                            table.Cell().Text("Họ tên:").SemiBold();
                            table.Cell().Text("Nguyễn Văn A");
                            table.Cell().Text("Email:").SemiBold();
                            table.Cell().Text("example@gmail.com");

                            table.Cell().Text("SĐT:").SemiBold();
                            table.Cell().Text("0901234567");
                            table.Cell().Text("CCCD:").SemiBold();
                            table.Cell().Text("123456789");
                        });
                    });

                    // --- Thông tin chuyến bay ---
                    col.Item().Background(Colors.Blue.Lighten5).Padding(6).Column(section =>
                    {
                        section.Item().Text("THÔNG TIN CHUYẾN BAY")
                            .Bold().FontSize(8).FontColor(Colors.Blue.Darken2);
                        section.Item().PaddingTop(4).Row(row =>
                        {
                            // Điểm đi
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().AlignCenter().Text("HAN").Bold().FontSize(18).FontColor(Colors.Blue.Darken3);
                                c.Item().AlignCenter().Text("Nội Bài").FontSize(8);
                                c.Item().AlignCenter().Text("07:15").SemiBold().FontSize(9);
                                c.Item().AlignCenter().Text("30/03/2026").FontSize(8);
                            });

                            row.ConstantItem(50).AlignMiddle().AlignCenter().Column(c =>
                            {
                                c.Item().AlignCenter().Text(">>>").FontSize(14).FontColor(Colors.Blue.Medium);
                                c.Item().AlignCenter().Text("Vietnam\nAirlines").FontSize(6).FontColor(Colors.Grey.Darken1);
                            });

                            // Điểm đến
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().AlignCenter().Text("SGN").Bold().FontSize(18).FontColor(Colors.Blue.Darken3);
                                c.Item().AlignCenter().Text("Tân Sơn Nhất").FontSize(8);
                                c.Item().AlignCenter().Text("09:15").SemiBold().FontSize(9);
                                c.Item().AlignCenter().Text("30/03/2026").FontSize(8);
                            });
                        });
                    });

                    // --- Thông tin ghế & vé ---
                    col.Item().Background(Colors.Grey.Lighten4).Padding(6).Column(section =>
                    {
                        section.Item().Text("THÔNG TIN GHẾ & VÉ")
                            .Bold().FontSize(8).FontColor(Colors.Blue.Darken2);
                        section.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });
                            table.Cell().Text("Số ghế:").SemiBold();
                            table.Cell().Text("12A");
                            table.Cell().Text("Hạng vé:").SemiBold();
                            table.Cell().Text("Phổ thông");

                            table.Cell().Text("Hành lý:").SemiBold();
                            table.Cell().Text("20kg");
                            table.Cell().Text("Chuyến bay:").SemiBold();
                            table.Cell().Text("VN123");
                        });
                    });

                    // --- Tổng tiền ---
                    col.Item()
                        .Background(Colors.Blue.Darken3)
                        .Padding(8)
                        .Row(row =>
                        {
                            row.RelativeItem().AlignMiddle().Text("TỔNG TIỀN THANH TOÁN")
                                .Bold().FontSize(9).FontColor(Colors.White);
                            row.ConstantItem(100).AlignRight().AlignMiddle()
                                .Text("1,200,000 VNĐ")
                                .Bold().FontSize(11).FontColor(Colors.Yellow.Lighten2);
                        });

                    // --- Chữ ký & barcode ---
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Cảm ơn quý khách đã sử dụng dịch vụ!")
                                .Italic().FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text("Chi tiết sẽ được gửi qua email của bạn.")
                                .Italic().FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(110).AlignRight().Column(c =>
                        {
                            c.Item().AlignRight().Text("Chữ ký nhân viên")
                                .SemiBold().FontSize(8);
                            try
                            {
                                using var client = new System.Net.Http.HttpClient();
                                byte[] signBytes = client.GetByteArrayAsync(
                                    "https://res.cloudinary.com/dzffkairf/image/upload/v1774797195/sign_bk91ae.png")
                                    .GetAwaiter().GetResult();
                                c.Item().AlignRight().Width(90).Height(40).Image(signBytes, ImageScaling.FitArea);
                            }
                            catch { }
                        });
                    });
                });

                // ===== FOOTER =====
                page.Footer()
                    .BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                    .PaddingTop(4)
                    .Row(row =>
                    {
                        row.RelativeItem().Text($"In ngày: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(7).FontColor(Colors.Grey.Medium);
                        row.RelativeItem().AlignCenter().Text(x =>
                        {
                            x.Span("Trang ").FontSize(7).FontColor(Colors.Grey.Medium);
                            x.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                            x.Span(" / ").FontSize(7).FontColor(Colors.Grey.Medium);
                            x.TotalPages().FontSize(7).FontColor(Colors.Grey.Medium);
                        });
                        row.RelativeItem().AlignRight().Text("QuanLyDatVeMayBay ©")
                            .FontSize(7).FontColor(Colors.Grey.Medium);
                    });
            });
        }
    }
}
