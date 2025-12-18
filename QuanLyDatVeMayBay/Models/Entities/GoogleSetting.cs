using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class GoogleSetting
{
    public int Id { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? RedirectUrl { get; set; }
}
