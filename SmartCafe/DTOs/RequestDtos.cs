using SmartCafe.Entities;
using SmartCafe.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartCafe.DTOs
{
    public class RequestDtos
    {
        public class RequestCategory()
        {
           // [Required(ErrorMessage = "category name is required")]
            [StringLength(100)]
            public string CategoryName { get; set; } = null!;
            public string? CategoryImage {  get; set; }
        }

        public class RequestMenu()
        {
            public string MenuName {  get; set; } = null!;
            public string MenuImage { get; set; }= null!;
            public string Description {  get; set; } = null!;
            public bool? Is_available { get; set; }
            public decimal Price {  get; set; } = 0;
            public int CategoryId { get; set; }
        
            
        }
        public class RequestOptionGroup()
        {
            [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Numbers and special characters are not allowed.")]
            public string GroupName {  get; set; } = null!;
        }

        public class RequestOptionItem()
        {
            public string ItemName {  get; set; } = null!;
            public decimal ExtraPrice {  get; set; } = 0;
            public int OptionGroupId { get; set; }
        }

        public class RequestMenuOptionGroupDto()
        {
            public int MenuId {  get; set; } = 0;
            public int OptionGroupId {  set; get; }= 0;
        }
        
        public class OrderRequest()
        {
            public string TableNo {  get; set; } =null!;
            public string? Note {  get; set; } = null!;
           
            public List<OrderItemRequest> Items { get; set; } = new();
        }

        public class OrderItemRequest()
        {
            public int MenuId {  set; get; } = 0;
            public int Quantity {  get; set; } = 0;
            public List<int> OptionItemSelectedIds { get; set; } = new();
        }

        public class ConfirmPaymentRequest()
        {
            public int OrderId {  set; get; } = 0;
            public string TransitionId { set; get; } = null!;//store in note 
        }
    }
}
