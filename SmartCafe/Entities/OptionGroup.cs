using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCafe.Entities;

[Table("Option_Groups")]
public partial class OptionGroup
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string GroupName { get; set; } = null!;

    public bool? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("OptionGroup")]
    public virtual ICollection<OptionItem> OptionItems { get; set; } = new List<OptionItem>();

    [InverseProperty("OptionGroup")]
    public virtual ICollection<ProductOptionGroup> ProductOptionGroups { get; set; } = new List<ProductOptionGroup>();
}
