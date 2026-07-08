using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCafe.Entities;

[Table("Option_Items")]
public partial class OptionItem
{
    [Key]
    public int Id { get; set; }

    public int OptionGroupId { get; set; }

    [StringLength(150)]
    public string ItemName { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ExtraPrice { get; set; }

    public bool? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("OptionGroupId")]
    [InverseProperty("OptionItems")]
    public virtual OptionGroup OptionGroup { get; set; } = null!;
}
