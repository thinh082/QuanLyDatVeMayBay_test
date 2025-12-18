using QuanLyDatVeMayBay.Models.Entities;

namespace MyApp
{
    public static class AwsHelper
    {
        public static AwsSetting LoadAwsS3Settings(ThinhContext context)
        {
            var config = context.AwsSettings.FirstOrDefault();
            if (config == null)
            {
                throw new Exception("AWS S3 configuration not found in the database.");
            }
            return config;
        }
    }
}
