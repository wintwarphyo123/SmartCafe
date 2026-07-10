using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCafe.Entities;

public partial class MenuDisabledOption
{
    [Key]
    public int Id { get; set; }

    public int MenuId { get; set; }

    public int OptionItemId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("MenuId")]
    [InverseProperty("MenuDisabledOptions")]
    public virtual Menu Menu { get; set; } = null!;

    [ForeignKey("OptionItemId")]
    [InverseProperty("MenuDisabledOptions")]
    public virtual OptionItem OptionItem { get; set; } = null!;
}
