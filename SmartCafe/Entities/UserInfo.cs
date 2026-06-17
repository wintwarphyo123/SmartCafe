using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCafe.Entities;

public partial class UserInfo
{
    [Key]
    [Column("userId")]
    [StringLength(50)]
    public string UserId { get; set; } = null!;

    [Column("userName")]
    [StringLength(50)]
    public string? UserName { get; set; }

    [Column("status")]
    public bool? Status { get; set; }

    [Column("joinDate")]
    public DateOnly? JoinDate { get; set; }

    [Column("role")]
    [StringLength(50)]
    public string? Role { get; set; }

    [Column("profileImage")]
    public string? ProfileImage { get; set; }
}
