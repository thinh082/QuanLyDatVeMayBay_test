using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class AwsSetting
{
    public int Id { get; set; }

    public string? Profile { get; set; }

    public string? Region { get; set; }

    public string? AccessKey { get; set; }

    public string? SecretKey { get; set; }

    public string? BucketName { get; set; }

    public string? ServiceUrl { get; set; }
}
