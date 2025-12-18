using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class AiChatBot
{
    public int Id { get; set; }

    public string CauHoi { get; set; } = null!;

    public string CauTraLoi { get; set; } = null!;
}
