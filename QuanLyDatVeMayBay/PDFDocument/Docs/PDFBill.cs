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
                page.Margin(10);
                page.Size(PageSizes.A6);
                page.PageColor(Colors.White);
                page.Header()
                    .Text("HÓA ĐƠN THANH TOÁN")
                    .SemiBold().FontSize(14).FontColor(Colors.Black);
                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        column.Item().Text(item =>
                        {
                            item.Span("Mã hóa đơn: ").SemiBold().FontSize(10);
                            item.Span("HD001").FontSize(10);
                        });
                        column.Item().Text(item =>
                        {
                            item.Span("Ngày: ").SemiBold().FontSize(10);
                            item.Span(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(10);
                        });
                        column.Item().Text(item =>
                        {
                            item.Span("Khách hàng: ").SemiBold().FontSize(10);
                            item.Span("Nguyễn Văn A").FontSize(10);
                        });
                        column.Item().Text(item =>
                        {
                            item.Span("Dịch vụ: ").SemiBold().FontSize(10);
                            item.Span("Vé máy bay").FontSize(10);
                        });
                        column.Item().Text(item =>
                        {
                            item.Span("Tổng tiền: ").SemiBold().FontSize(10);
                            item.Span("1,200,000 VND").FontSize(10);
                        });
                        string base64String = "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";
                        base64String = base64String.Trim();
                        base64String = base64String.Trim('"', '\'', ';');
                        base64String = base64String.Replace("\r", "").Replace("\n", "").Replace(" ", "");
                        byte[] image = Convert.FromBase64String(base64String);
                        column.Item().AlignCenter().Height(150).Image(image);
                        column.Spacing(10);
                        column.Item().Text("Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!").FontSize(10);
                        column.Item().Text("Chi tiết hóa đơn sẽ được gửi qua email của bạn.").FontSize(10);
                    });
                    
                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
            });
        }
    }
}
