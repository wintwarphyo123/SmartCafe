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

        public class ResponseOptionGroup()
        {
            public int? Id { get; set; }
            public string? GroupName { get; set; } 
        }

        public class ResponseOptionItem()
        {
            public int? Id { get; set; }
            public string? ItemName {  get; set; }
            public decimal? ExtraPrice {  get; set; }
            public int? OptionGroupId {  get; set; }
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
            public string TableNumber { get; set; }=null!;
            public decimal TotalAmount { get; set; }
            public string OrderStatus { get; set; } = null!;
            public string? Note { get; set; }
            public DateTime CreatedAt { get; set; }

            public List<OrderItemResponseDto> OrderItems { get; set; }=new();
        }

        public class OrderItemResponseDto
        {
            public int OrderItemId { get; set; }
            public int MenuId { get; set; }
            public int Quantity { get; set; }
            public decimal PriceAtOrder { get; set; }
            public string SelectedOptionsJson { get; set; } = null!;
        }
    }
}
