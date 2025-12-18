using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDatVeMayBay.Models.Model;

public class DangKyRequest
{
    [Required(ErrorMessage = " Email là bắt buộc")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    public string SoDienThoai { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    public string MatKhau { get; set; } = string.Empty;
    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
    [Compare("MatKhau", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không trùng khớp")]
    public string XacNhanMatKhau { get; set; } = string.Empty;
}

