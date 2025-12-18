using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class FacebookSetting
{
    public int Id { get; set; }

    public string? AppId { get; set; }

    public string? AppSecret { get; set; }

    public string? RedirectUri { get; set; }
}
