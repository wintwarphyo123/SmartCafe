using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCafe.Entities;

public partial class MenuRecommendation
{
    [Key]
    public int Id { get; set; }

    public int MainMenuId { get; set; }

    public int RecommendedMenuId { get; set; }

    public int PairingCount { get; set; }

    public double SupportScore { get; set; }

    public DateTime LastUpdated { get; set; }

    [ForeignKey("MainMenuId")]
    [InverseProperty("MenuRecommendationMainMenus")]
    public virtual Menu MainMenu { get; set; } = null!;

    [ForeignKey("RecommendedMenuId")]
    [InverseProperty("MenuRecommendationRecommendedMenus")]
    public virtual Menu RecommendedMenu { get; set; } = null!;
}
