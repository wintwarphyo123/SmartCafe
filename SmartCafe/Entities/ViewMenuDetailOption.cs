using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCafe.Entities;

[Keyless]
public partial class ViewMenuDetailOption
{
    public int MenuId { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string? MenuName { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MenuPrice { get; set; }

    [StringLength(500)]
    public string? MenuDescription { get; set; }

    public int? GroupId { get; set; }

    [StringLength(150)]
    public string? GroupName { get; set; }

    public int? ItemId { get; set; }

    [StringLength(150)]
    public string? ItemName { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? ExtraPrice { get; set; }

    public bool? IsAvailable { get; set; }
}
