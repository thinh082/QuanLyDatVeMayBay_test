using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class Vnpaysetting
{
    public int Id { get; set; }

    public string? TmnCode { get; set; }

    public string? HashSecret { get; set; }

    public string? BaseUrl { get; set; }

    public string? Enviroment { get; set; }

    public string? CallBackUrl { get; set; }

    public DateTime? CreateAt { get; set; }
}
