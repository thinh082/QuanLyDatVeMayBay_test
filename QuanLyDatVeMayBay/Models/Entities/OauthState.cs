using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class OauthState
{
    public string State { get; set; } = null!;

    public string? CodeVerifier { get; set; }

    public DateTime? CreatedAt { get; set; }
}
