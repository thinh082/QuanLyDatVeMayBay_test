using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class ApiLog
{
    public long Id { get; set; }

    public string? Path { get; set; }

    public string? Method { get; set; }

    public string? RequestBody { get; set; }

    public string? ResponseBody { get; set; }

    public int? StatusCode { get; set; }

    public DateTime? CreatedAt { get; set; }
}
