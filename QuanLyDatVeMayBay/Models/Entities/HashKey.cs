using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class HashKey
{
    public int Id { get; set; }

    public string? PublicKey { get; set; }

    public string? PrivateKey { get; set; }
}
