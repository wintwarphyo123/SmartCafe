using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCafe.Entities;

public partial class Category
{
    [Key]
    public int CategoryId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? CategoryName { get; set; }

    [Column("Is_active")]
    public bool? IsActive { get; set; }

    [Unicode(false)]
    public string? CategoryImage { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
}
