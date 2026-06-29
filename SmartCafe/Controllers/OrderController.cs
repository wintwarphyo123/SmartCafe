using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Entities;
using SmartCafe.Models;
using System.Text.Json;

namespace SmartCafe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(SmartCafeDbContext context):ControllerBase
    {

        [HttpGet]
        [EndpointSummary("Get All Orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await context.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Orders retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred: " + ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        [EndpointSummary("Get Order Detail By Id")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(item=>item.Menu)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    return NotFound(new DefaultResponseModel()
                    {
                        Success = false,
                        Statuscode = StatusCodes.Status404NotFound,
                        Message = "Order not found",
                        Data = null
                    });
                }
                var responseDto = new ResponseDtos.OrderResponseDto
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    PhoneNumber=order.PhoneNumber,
                    OrderStatus = order.OrderStatus,
                    Note = order.Note,
                    CreatedAt = order.CreatedAt,

                    OrderItems = order.OrderItems.Select(item => {

                        var optionIds = string.IsNullOrWhiteSpace(item.SelectedOptionsJson)
                            ? new List<int>()
                            : item.SelectedOptionsJson.Contains("[")
                                ? new List<int>() // If it's a JSON array layout, handle or skip
                                : item.SelectedOptionsJson.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(int.Parse).ToList();

                        return new ResponseDtos.OrderItemResponseDto
                        {
                            OrderItemId = item.OrderItemId,
                            MenuId = item.MenuId,

                            
                            MenuName = context.Menus.FirstOrDefault(m => m.MenuId == item.MenuId)?.MenuName ?? "Unknown Item",
                            Quantity = item.Quantity,
                            PriceAtOrder = item.PriceAtOrder,

                            
                            SelectedOptions = context.OptionItems
                                .Include(oi => oi.OptionGroup)
                                .Where(oi => optionIds.Contains(oi.Id))
                                .Select(oi => new ResponseDtos.SelectedOptionDto
                                {
                                    OptionGroupName = oi.OptionGroup!.GroupName,
                                    OptionItemName = oi.ItemName,
                                    ExtraPrice = oi.ExtraPrice
                                }).ToList()
                        };
                    }).ToList()
                };

                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Order detail retrieved successfully",
                    Data = responseDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred: " + ex.Message
                });
            }
        }

        //get paid order by kitchen
        [HttpGet("Kitchen_Queue")]
        [EndpointSummary("Get paid orders by kitchen")]
        public async Task<IActionResult> GetPaidOrders()
        {
            var orderList = await context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderStatus ==OrderStatus.Paid.ToString() || o.OrderStatus == Models.OrderStatus.Preparing.ToString())
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
            if (orderList.Count == 0)
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Kitchen query is currently empty",
                    Data = orderList
                });
            }
            var orderData = orderList.Select(order => new ResponseDtos.OrderResponseDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus.ToString(),
                Note = order.Note,
                PhoneNumber=order.PhoneNumber,
                CreatedAt = order.CreatedAt,
                OrderItems = order.OrderItems.Select(item => new ResponseDtos.OrderItemResponseDto
                {
                    OrderItemId = item.OrderItemId,
                    MenuId = item.MenuId,
                    Quantity = item.Quantity,
                    PriceAtOrder = item.PriceAtOrder,
                    SelectedOptions = new List<ResponseDtos.SelectedOptionDto>()
                }).ToList()

            }).ToList();

            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "Kitchen active queue retrieved",
                Data = orderData
            });
        }
        
        [HttpGet("filter")]
        [EndpointSummary("to filter orders by Date, Status")]
        public async Task<IActionResult> FilterOrders(
                                     [FromQuery] DateTime? startDate,
                                     [FromQuery] DateTime? endDate,
                                     [FromQuery] string? status,
                                     [FromQuery] string? tableNumber)
        {
            try
            {
                var query = context.Orders.Include(o => o.OrderItems).AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= endDate.Value);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    if(Enum.TryParse<OrderStatus>(status,true,out var parsedStatus))//convert string to Enu
                    {
                        query = query.Where(o => o.OrderStatus.ToString() == status);
                    }
                    else
                    {
                        return BadRequest(new DefaultResponseModel()
                        {
                            Success = false,
                            Statuscode = StatusCodes.Status400BadRequest,
                            Message = $"Invalid status value provided: {status}"
                        });
                    }
                }

                var filteredOrders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

                
                var orderDtos = filteredOrders.Select(order => new ResponseDtos.OrderResponseDto
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    OrderStatus = order.OrderStatus.ToString(),
                    Note = order.Note,
                    PhoneNumber=order.PhoneNumber,
                    CreatedAt = order.CreatedAt,
                    OrderItems = order.OrderItems.Select(item => new ResponseDtos.OrderItemResponseDto
                    {
                        OrderItemId = item.OrderItemId,
                        MenuId = item.MenuId,
                        Quantity = item.Quantity,
                        PriceAtOrder = item.PriceAtOrder,
                        SelectedOptions = new List<ResponseDtos.SelectedOptionDto>()
                    }).ToList()
                }).ToList();

                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = $"Found {orderDtos.Count} orders matching the filter criteria.",
                    Data = orderDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred: " + ex.Message
                });
            }
        }

        [HttpPost]
        [EndpointSummary("Create Order")]
        public async Task<IActionResult> PlaceOrder(RequestDtos.OrderRequest orderDto)
        {
            try 
            { 
            await context.Database.BeginTransactionAsync();
            if(string.IsNullOrWhiteSpace(orderDto.PhoneNumber))
                {
                    return BadRequest(new DefaultResponseModel()
                    {
                        Success=false,
                        Statuscode=StatusCodes.Status400BadRequest,
                        Message="Phone Number is required",
                        Data=null
                    });
                }
            List<OrderItem> orderList = new List<OrderItem>();//for store order Items that ordered
            decimal totalAmout = 0;
            
            //to check that the oredered menu is exist or not
          
            foreach (var item in orderDto.Items)
            {
                var itemList = await context.Menus
                    .FirstOrDefaultAsync(p => p.MenuId == item.MenuId && p.DeletedAt == null);
                if (itemList == null)
                {
                    return NotFound(new DefaultResponseModel()
                    {
                        Success = false,
                        Statuscode = StatusCodes.Status404NotFound,
                        Message = "Menu doesn't exist",
                        Data = null
                    });
                }
                decimal singleItemPrice = (decimal)itemList.Price;
                    if(item.OptionItemSelectedIds != null)
                    {
                        foreach (var optionItem in item.OptionItemSelectedIds)//option items
                        {
                            var optionData = await context.OptionItems
                                .FirstOrDefaultAsync(o => o.Id == optionItem);
                            if (optionData != null)
                            {
                                singleItemPrice += optionData.ExtraPrice;
                            }
                        }
                    }
                totalAmout += (singleItemPrice * item.Quantity);
                var orderItem = new OrderItem()
                {
                    MenuId = item.MenuId,
                    Quantity = item.Quantity,
                    PriceAtOrder = singleItemPrice,
                    SelectedOptionsJson = string.Join(",", item.OptionItemSelectedIds)
                };
                orderList.Add(orderItem);
            }
                var orderData = new Order()
                {

                    OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    PhoneNumber = orderDto.PhoneNumber,
                    TotalAmount = totalAmout,
                    CreatedAt = DateTime.UtcNow,
                    OrderStatus = OrderStatus.Pending.ToString()
                };
            await context.Orders.AddAsync(orderData);
            await context.SaveChangesAsync();

            foreach (var itemData in orderList)
            {
                itemData.OrderId = orderData.OrderId;
                
            }
            await context.OrderItems.AddRangeAsync(orderList);
            await context.SaveChangesAsync();
                
                await context.Database.CurrentTransaction.CommitAsync();
                var responseDto = new ResponseDtos.OrderResponseDto
                {
                    OrderId = orderData.OrderId,
                    OrderNumber = orderData.OrderNumber,
                    TotalAmount = orderData.TotalAmount,
                    OrderStatus = orderData.OrderStatus,
                    PhoneNumber=orderData.PhoneNumber,
                    CreatedAt = orderData.CreatedAt,
                    OrderItems = orderList.Select(item => new ResponseDtos.OrderItemResponseDto
                    {
                        OrderItemId = item.OrderItemId,
                        MenuId = item.MenuId,
                        Quantity = item.Quantity,
                        PriceAtOrder = item.PriceAtOrder,
                        SelectedOptions=new List<ResponseDtos.SelectedOptionDto>()
                }).ToList()
                };
                return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "Placed order complete",
                Data = responseDto
            });
        }
            catch (Exception ex)
        {
                await context.Database.CurrentTransaction.RollbackAsync();
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                //await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred: " +innerMessage
                });
            }

        }
        //payment
        [HttpPut("confirmPayment")]
        [EndpointSummary("Confirm payment")]
        public async Task<IActionResult> ConfirmPayment(RequestDtos.ConfirmPaymentRequest request)
        {
            try
            {
                var order = await context.Orders.FirstOrDefaultAsync(o => o.OrderId == request.OrderId);
                if (order == null)
                {
                    return NotFound(new DefaultResponseModel()
                    {
                        Success = false,
                        Statuscode = StatusCodes.Status404NotFound,
                        Message = "OrderId doesn't exist",
                        Data = null
                    });
                }
                if (order.OrderStatus != OrderStatus.Pending.ToString())
                {
                    return BadRequest(new DefaultResponseModel()
                    {
                        Success = false,
                        Statuscode = StatusCodes.Status400BadRequest,
                        Message = "Order can't be confirm",
                        Data = null
                    });
                }
                order.OrderStatus = OrderStatus.Paid.ToString();
                order.Note = $"Paid via KPay/WavePay (Txn ID: {request.TransitionId})";

                await context.SaveChangesAsync();
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Payment confirmed successfully. Order sent to Kitchen!",
                    Data = order
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred: " + ex.Message
                });
            }
        }

        [HttpPut("{orderId}/prepare")]
        [EndpointSummary("Prepare Order")]
        public async Task<IActionResult> StartPreparingOrder(int orderId)
        {
            var orderData=await context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if(orderData == null)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Order can't be confirm",
                    Data = null
                });
            }

            orderData.OrderStatus = OrderStatus.Preparing.ToString();
            await context.SaveChangesAsync();
            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "order is preparing",
                Data = orderData
            });
        }

        [HttpPut("{orderId}/complete")]
        [EndpointSummary("Complete Order")]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var orderData = await context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (orderData == null)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Order can't be confirm",
                    Data = null
                });
            }

            orderData.OrderStatus = OrderStatus.Ready.ToString();
            await context.SaveChangesAsync();
            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "order is ready",
                Data = orderData
            });
        }
    }
}
