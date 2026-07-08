using SmartCafe.Entities;

namespace SmartCafe.DTOs
{
    public class ResponseDtos
    {
        public class AllCategories()
        {
            public int Id { get; set; }
            public string? CategoryName {  get; set; }
            public bool? isActive { get; set; }
            public string? CategoryImage {  get; set; }

        }

        public class AllMenu()
        {
            public int Id { get; set; }
            public string? MenuName {  get; set; }
            public string? MenuImage { get; set; }
            public decimal? Price { get; set; }
            public string? Description {  get; set; }
            public bool? Is_available {  get; set; }
            public int? CategoryId {  get; set; }
            public string? CategoryName {  get; set; }
        }
        public class AllCategoryForDropDown()
        {
            public int CategoriesId { get; set; }
            public string? CategoriesName { get; set; }
        }

        public class ResponseOptionGroup()
        {
            public int? Id { get; set; }
            public string? GroupName { get; set; } 
            public bool? Status { get; set; }
        }

        public class ResponseOptionItem()
        {
            public int? Id { get; set; }
            public string? ItemName {  get; set; }
            public decimal? ExtraPrice {  get; set; }
            public bool? Status { get; set; }
            public int? OptionGroupId {  get; set; }
            public string? GroupName { get; set; }
        }

        public class MenuDetailResponseDto
        {
            public int MenuId { get; set; }
            public string? MenuName { get; set; }
            public decimal? Price { get; set; }
            public string? Description { get; set; }
            public List<OptionGroupDto> OptionGroups { get; set; } = new();
        }

        public class OptionGroupDto
        {
            public int GroupId { get; set; }
            public string GroupName { get; set; } = null!;
            public List<OptionItemDto> OptionItems { get; set; } = new();
        }

        public class OptionItemDto
        {
            public int ItemId { get; set; }
            public string ItemName { get; set; } = null!;
            public decimal ExtraPrice { get; set; }
        }
        
        public class OrderResponseDto
        {
            public int OrderId { get; set; }
            public string OrderNumber { get; set; }= null!;
            public decimal TotalAmount { get; set; }
            public string OrderStatus { get; set; } = null!;
            public string PhoneNumber { get; set; } = null!;
            public string? Note { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }

            public List<OrderItemResponseDto> OrderItems { get; set; }=new();
        }

        public class OrderItemResponseDto
        {
            public int OrderItemId { get; set; }
            public int MenuId { get; set; }
            public string MenuName { get; set; }=null!;
            public int Quantity { get; set; }
            public decimal PriceAtOrder { get; set; }
            public List<SelectedOptionDto> SelectedOptions { get; set; } = new();
        }

        public class SelectedOptionDto
        {
            public string OptionGroupName { get; set; } = null!; 
            public string OptionItemName { get; set; } = null!;  
            public decimal ExtraPrice { get; set; }            
        }

        public class SummaryDashboardDto
        {
            public int TotalMenu {  get; set; }
            public int TotalCategory { get; set; }
            public int TotalStaff { get; set; }
            public decimal TotalRevenue { get; set; }
            public int TotalOrders { get; set; }

        }

        public class TrendingItemResponseModel
        {
            public int MenuId { get; set; }
            public string MenuName{ get; set; }=null!;
            public string CategoryName { get; set; } = null!;
            public int TotalSales { get; set; }
            public double Percentage { get; set; }
        }
    }
}
