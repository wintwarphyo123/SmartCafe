using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCafe.Entities;

[Index("OrderNumber", Name = "UQ__Orders__CAC5E7437C397EBD", IsUnique = true)]
public partial class Order
{
    [Key]
    public int OrderId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string OrderNumber { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string OrderStatus { get; set; } = null!;

    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
