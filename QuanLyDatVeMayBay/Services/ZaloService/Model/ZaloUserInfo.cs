namespace QuanLyDatVeMayBay.Services.ZaloService.Model
{
    public class ZaloUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ZaloPicture Picture { get; set; }
    }

    public class ZaloPicture
    {
        public ZaloPictureData Data { get; set; }
    }

    public class ZaloPictureData
    {
        public string Url { get; set; }
    }
}
