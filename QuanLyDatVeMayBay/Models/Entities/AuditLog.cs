using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class AuditLog
{
    public long Id { get; set; }

    public long? IdTaiKhoan { get; set; }

    public string HanhDong { get; set; } = null!;

    public string TenBang { get; set; } = null!;

    public long? RecordId { get; set; }

    public string? LogAction { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? CreatedAt { get; set; }
}
