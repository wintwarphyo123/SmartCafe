using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCafe.Entities;

[PrimaryKey("MenuId", "OptionGroupId")]
[Table("Product_Option_Groups")]
public partial class ProductOptionGroup
{
    [Key]
    public int MenuId { get; set; }

    [Key]
    public int OptionGroupId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("MenuId")]
    [InverseProperty("ProductOptionGroups")]
    public virtual Menu Menu { get; set; } = null!;

    [ForeignKey("OptionGroupId")]
    [InverseProperty("ProductOptionGroups")]
    public virtual OptionGroup OptionGroup { get; set; } = null!;
}
