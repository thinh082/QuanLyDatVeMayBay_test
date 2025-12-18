using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class ZaloSetting
{
    public int Id { get; set; }

    public string? AppId { get; set; }

    public string? AppSceret { get; set; }

    public string? RedirectUrl { get; set; }
}
