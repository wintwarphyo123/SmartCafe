using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Entities;

namespace SmartCafe.Data;

public partial class SmartCafeDbContext : DbContext
{
    public SmartCafeDbContext(DbContextOptions<SmartCafeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<OptionGroup> OptionGroups { get; set; }

    public virtual DbSet<OptionItem> OptionItems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<ProductOptionGroup> ProductOptionGroups { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B897F4596");

            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Categories_is_active");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menu__C99ED23080F57FA2");

            entity.Property(e => e.IsAvailable).HasDefaultValue(true, "DF__Menu__Is_availab__49C3F6B7");

            entity.HasOne(d => d.Category).WithMany(p => p.Menus).HasConstraintName("fk_Menus_Categories");
        });

        modelBuilder.Entity<OptionGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Option_G__3214EC07A11740E4");
        });

        modelBuilder.Entity<OptionItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Option_I__3214EC07D056B0CC");

            entity.HasOne(d => d.OptionGroup).WithMany(p => p.OptionItems).HasConstraintName("FK__Option_It__Optio__6383C8BA");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCFF0574BA3");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.OrderStatus).HasDefaultValue("Pending");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED0681E8A8FD42");

            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Menu).WithMany(p => p.OrderItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Menu");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasConstraintName("FK_OrderItems_Orders");
        });

        modelBuilder.Entity<ProductOptionGroup>(entity =>
        {
            entity.HasKey(e => new { e.MenuId, e.OptionGroupId }).HasName("PK__Product___68CBA0AC942A08EE");

            entity.HasOne(d => d.Menu).WithMany(p => p.ProductOptionGroups).HasConstraintName("FK__Product_O__MenuI__6754599E");

            entity.HasOne(d => d.OptionGroup).WithMany(p => p.ProductOptionGroups).HasConstraintName("FK__Product_O__Optio__68487DD7");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
