using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class JwtSetting
{
    public int Id { get; set; }

    public string? KeyJwt { get; set; }

    public string? Audience { get; set; }

    public int? ExpireMinutes { get; set; }

    public string? Issuer { get; set; }
}
