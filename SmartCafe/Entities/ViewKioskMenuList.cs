using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCafe.Entities;

[Keyless]
public partial class ViewKioskMenuList
{
    public int Id { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string? MenuName { get; set; }

    public string? MenuImage { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Price { get; set; }

    public int? CategoryId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? CategoryName { get; set; }

    [Column("isAvailable")]
    public bool? IsAvailable { get; set; }
}
