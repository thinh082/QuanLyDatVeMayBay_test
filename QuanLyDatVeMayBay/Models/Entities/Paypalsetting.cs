using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class Paypalsetting
{
    public int Id { get; set; }

    public string? ClientId { get; set; }

    public string? SecretId { get; set; }

    public string? Enviroment { get; set; }

    public string? BaseUrl { get; set; }

    public DateTime? CreateAt { get; set; }
}
