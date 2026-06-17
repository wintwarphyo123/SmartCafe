using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCafe.Entities;

[Table("Menu")]
public partial class Menu
{
    [Key]
    public int MenuId { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string? MenuName { get; set; }

    public string? MenuImage { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Price { get; set; }

    [Column("Is_available")]
    public bool? IsAvailable { get; set; }

    public int? CategoryId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("UpdatedAT", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column("DeletedAT", TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Menus")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Menu")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Menu")]
    public virtual ICollection<ProductOptionGroup> ProductOptionGroups { get; set; } = new List<ProductOptionGroup>();
}
