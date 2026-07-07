
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Entities;
using SmartCafe.Hubs;
using SmartCafe.Models;
using SmartCafe.Services;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace SmartCafe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(SmartCafeDbContext context,
        ExportService exportService,
        IHubContext<NotificationHubs> hubContext) :ControllerBase
    {

        [HttpGet]
        [EndpointSummary("Get All Orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] string status = "All")
        {
            try
            {
                var query = context.Orders.AsQueryable();
                if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(o => o.OrderStatus == status);
                }
                var today = DateTime.Today;
                var orders = await query
                    .Where(o=>o.CreatedAt.Date==today)
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

        //[HttpGet("{id}")]
        //[EndpointSummary("Get Order Detail By Id")]
        //public async Task<IActionResult> GetOrderById(int id)
        //{
        //    try
        //    {
        //        var order = await context.Orders
        //            .Include(o => o.OrderItems)
        //            .ThenInclude(item=>item.Menu)
        //            .FirstOrDefaultAsync(o => o.OrderId == id);

        //        if (order == null)
        //        {
        //            return NotFound(new DefaultResponseModel()
        //            {
        //                Success = false,
        //                Statuscode = StatusCodes.Status404NotFound,
        //                Message = "Order not found",
        //                Data = null
        //            });
        //        }
        //        var responseDto = new ResponseDtos.OrderResponseDto
        //        {
        //            OrderId = order.OrderId,
        //            OrderNumber = order.OrderNumber,
        //            TotalAmount = order.TotalAmount,
        //            PhoneNumber=order.PhoneNumber,
        //            OrderStatus = order.OrderStatus,
        //            Note = order.Note,
        //            CreatedAt = order.CreatedAt,

        //            OrderItems = order.OrderItems.Select(item => {

        //                var optionIds = new List<int>();

        //                if (!string.IsNullOrWhiteSpace(item.SelectedOptionsJson))
        //                {
        //                    var jsonStr = item.SelectedOptionsJson.Trim();
        //                    if (jsonStr.StartsWith("[") && jsonStr.EndsWith("]"))
        //                    {
        //                        try
        //                        {
        //                            optionIds = JsonSerializer.Deserialize<List<int>>(jsonStr) ?? new List<int>();
        //                        }
        //                        catch
        //                        {
        //                            optionIds = jsonStr.Trim('[', ']')
        //                                               .Split(',', StringSplitOptions.RemoveEmptyEntries)
        //                                               .Select(int.Parse).ToList();
        //                        }
        //                    }
        //                    else
        //                    {
        //                        optionIds = jsonStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
        //                                           .Select(int.Parse).ToList();
        //                    }
        //                }

        //                return new ResponseDtos.OrderItemResponseDto
        //                {
        //                    OrderItemId = item.OrderItemId,
        //                    MenuId = item.MenuId,
        //                    MenuName = item.Menu?.MenuName ?? "Unknown Item",
        //                    Quantity = item.Quantity,
        //                    PriceAtOrder = item.PriceAtOrder,
        //                    SelectedOptions = context.OptionItems
        //                        .Include(oi => oi.OptionGroup)
        //                        .Where(oi => optionIds.Contains(oi.Id))
        //                        .Select(oi => new ResponseDtos.SelectedOptionDto
        //                        {
        //                            OptionGroupName = oi.OptionGroup!.GroupName,
        //                            OptionItemName = oi.ItemName,
        //                            ExtraPrice = oi.ExtraPrice
        //                        }).ToList()
        //                };
        //            }).ToList()
        //        };

        //        return Ok(new DefaultResponseModel()
        //        {
        //            Success = true,
        //            Statuscode = StatusCodes.Status200OK,
        //            Message = "Order detail retrieved successfully",
        //            Data = responseDto
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponseModel()
        //        {
        //            Success = false,
        //            Statuscode = StatusCodes.Status500InternalServerError,
        //            Message = "An error occurred: " + ex.Message
        //        });
        //    }
        //}

        // [HttpGet("{id}")]
        //[EndpointSummary("Get Order Detail By Id")]
        //public async Task<IActionResult> GetOrderById(int id)
        //{
        //    var order = await context.Orders
        //        .Include(o => o.OrderItems)
        //            .ThenInclude(item => item.Menu)
        //        .Where(o => o.OrderId == id)
        //        .FirstOrDefaultAsync();

        //    if (order == null)
        //    {
        //        return NotFound(new DefaultResponseModel
        //        {
        //            Success = false,
        //            Statuscode = StatusCodes.Status404NotFound,
        //            Message = "Order not found"
        //        });
        //    }

        //    var responseDto = new ResponseDtos.OrderResponseDto
        //    {
        //        OrderId = order.OrderId,
        //        OrderNumber = order.OrderNumber,
        //        TotalAmount = order.TotalAmount,
        //        PhoneNumber = order.PhoneNumber,
        //        OrderStatus = order.OrderStatus,
        //        Note = order.Note,
        //        CreatedAt = order.CreatedAt,

        //        OrderItems = order.OrderItems
        //            .Select(item => new ResponseDtos.OrderItemResponseDto
        //            {
        //                OrderItemId = item.OrderItemId,
        //                MenuId = item.MenuId,
        //                MenuName = item.Menu?.MenuName ?? "Unknown Item",
        //                Quantity = item.Quantity,
        //                PriceAtOrder = item.PriceAtOrder,

        //                SelectedOptions = context.OptionItems
        //                    .Where(oi => (!string.IsNullOrEmpty(item.SelectedOptionsJson) &&
        //                                  item.SelectedOptionsJson.Contains(oi.Id.ToString())))
        //                    .Select(oi => new ResponseDtos.SelectedOptionDto
        //                    {
        //                        OptionGroupName = oi.OptionGroup!.GroupName,
        //                        OptionItemName = oi.ItemName,
        //                        ExtraPrice = oi.ExtraPrice
        //                    }).ToList()
        //            }).ToList()
        //    };

        //    return Ok(new DefaultResponseModel
        //    {
        //        Success = true,
        //        Statuscode = StatusCodes.Status200OK,
        //        Message = "Order detail retrieved successfully",
        //        Data = responseDto
        //    });
        //}


        [HttpGet("{id}")]
        [EndpointSummary("Get Order Detail By Id")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            // 1. Fetch the main order details and items first
            var order = await context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(item => item.Menu)
                .Where(o => o.OrderId == id)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(new DefaultResponseModel
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Order not found"
                });
            }

            // 2. Parse out the IDs and keep track of which option goes to which order item
            var allSelectedIds = new HashSet<int>();
            var itemOptionsMap = new Dictionary<int, List<int>>();

            foreach (var item in order.OrderItems)
            {
                if (!string.IsNullOrEmpty(item.SelectedOptionsJson))
                {
                    try
                    {
                        // Try parsing standard integer arrays: [1, 2]
                        var ids = JsonSerializer.Deserialize<List<int>>(item.SelectedOptionsJson);
                        if (ids != null)
                        {
                            itemOptionsMap[item.OrderItemId] = ids;
                            foreach (var optionId in ids) allSelectedIds.Add(optionId);
                        }
                    }
                    catch (JsonException)
                    {
                        // Fallback: Try parsing string arrays if stored as ["1", "2"]
                        try
                        {
                            var stringIds = JsonSerializer.Deserialize<List<string>>(item.SelectedOptionsJson);
                            if (stringIds != null)
                            {
                                var parsedIds = stringIds
                                    .Select(s => int.TryParse(s, out int parsed) ? parsed : 0)
                                    .Where(i => i != 0)
                                    .ToList();

                                itemOptionsMap[item.OrderItemId] = parsedIds;
                                foreach (var optionId in parsedIds) allSelectedIds.Add(optionId);
                            }
                        }
                        catch
                        {
                            // Fallback for empty or corrupt format
                            itemOptionsMap[item.OrderItemId] = new List<int>();
                        }
                    }
                }
            }

            // 3. Batch query ONLY the option items that were actually used in this order
            var optionsLookup = await context.OptionItems
                .Include(oi => oi.OptionGroup)
                .Where(oi => allSelectedIds.Contains(oi.Id))
                .ToDictionaryAsync(oi => oi.Id);

            // 4. Map perfectly to your response DTO structure
            var responseDto = new ResponseDtos.OrderResponseDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                PhoneNumber = order.PhoneNumber,
                OrderStatus = order.OrderStatus,
                Note = order.Note,
                CreatedAt = order.CreatedAt,

                OrderItems = order.OrderItems.Select(item => new ResponseDtos.OrderItemResponseDto
                {
                    OrderItemId = item.OrderItemId,
                    MenuId = item.MenuId,
                    MenuName = item.Menu?.MenuName ?? "Unknown Item",
                    Quantity = item.Quantity,
                    PriceAtOrder = item.PriceAtOrder,

                    // Pull matching options from the batch lookup dictionary
                    SelectedOptions = itemOptionsMap.TryGetValue(item.OrderItemId, out var optionIds)
                        ? optionIds
                            .Where(id => optionsLookup.ContainsKey(id))
                            .Select(id => optionsLookup[id])
                            .Select(oi => new ResponseDtos.SelectedOptionDto
                            {
                                OptionGroupName = oi.OptionGroup?.GroupName ?? "Unknown Group",
                                OptionItemName = oi.ItemName,
                                ExtraPrice = oi.ExtraPrice
                            }).ToList()
                        : new List<ResponseDtos.SelectedOptionDto>()
                }).ToList()
            };

            return Ok(new DefaultResponseModel
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "Order detail retrieved successfully",
                Data = responseDto
            });
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
                                     [FromQuery] DateTime? endDate
                                     )
        {
            try
            {
                var query = context.Orders.Include(o => o.OrderItems).AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(o => o.CreatedAt <= endDate);
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
        //remain
        [HttpPost("excel")]
        [EndpointSummary("export excel")]
        [EndpointDescription("order report")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> PostExcelAsync([FromQuery] DateTime? startDate,
                                         [FromQuery] DateTime? endDate,
                                         [FromQuery] string? q,
                                         [FromQuery] string? sortfield,
                                         [FromQuery] int order,
                                          [FromBody] KeyValuePair<string, string>[] columns)
        {
            IQueryable<Order> orders = OrderQuery(q, sortfield, order);
            DateTime start = startDate?.Date ?? DateTime.Today;
            DateTime end = endDate?.Date ?? DateTime.Today;
            DateTime endOfPeriod = end.AddDays(1).AddTicks(-1);

            orders = orders.Where(p => p.CreatedAt >= start && p.CreatedAt <= endOfPeriod);
            List<Order> records = await orders.ToListAsync();

            if (records.Count == 0 || records==null)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success=false,
                    Statuscode=StatusCodes.Status400BadRequest,
                    Message = "Failed to report excel"
                });
            }
            Stream? stream = exportService.ExportToExcelStreamSpecificColumns(records, columns, "Order List");
            if (stream == null)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Message = "Failed to report excel"
                });
            }
            string fileName = $"Orders_Report_{start:yyyyMMdd}_to_{end:yyyyMMdd}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        //....

        //[HttpPost]
        //[EndpointSummary("Create Order")]
        //public async Task<IActionResult> PlaceOrder(RequestDtos.OrderRequest orderDto)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(orderDto.PhoneNumber))
        //        {
        //            return BadRequest(new DefaultResponseModel()
        //            {
        //                Success = false,
        //                Statuscode = StatusCodes.Status400BadRequest,
        //                Message = "Phone Number is required",
        //                Data = null
        //            });
        //        }

        //        List<OrderItem> orderList = new List<OrderItem>();
        //        decimal totalAmout = 0;

        //        foreach (var item in orderDto.Items)
        //        {
        //            var itemList = await context.Menus
        //                .FirstOrDefaultAsync(p => p.MenuId == item.MenuId && p.DeletedAt == null);

        //            if (itemList == null)
        //            {
        //                return NotFound(new DefaultResponseModel()
        //                {
        //                    Success = false,
        //                    Statuscode = StatusCodes.Status404NotFound,
        //                    Message = $"Menu ID {item.MenuId} doesn't exist",
        //                    Data = null
        //                });
        //            }

        //            decimal singleItemPrice = (decimal)itemList.Price;

        //            if (item.OptionItemSelectedIds != null && item.OptionItemSelectedIds.Any())
        //            {
        //                foreach (var optionItem in item.OptionItemSelectedIds)
        //                {
        //                    var optionData = await context.OptionItems
        //                        .FirstOrDefaultAsync(o => o.Id == optionItem);
        //                    if (optionData != null)
        //                    {
        //                        singleItemPrice += optionData.ExtraPrice;
        //                    }
        //                }
        //            }

        //            totalAmout += (singleItemPrice * item.Quantity);

        //            var selectedOptionsStr = item.OptionItemSelectedIds != null
        //                ? string.Join(",", item.OptionItemSelectedIds)
        //                : string.Empty;

        //            var orderItem = new OrderItem()
        //            {
        //                MenuId = item.MenuId,
        //                Quantity = item.Quantity,
        //                PriceAtOrder = singleItemPrice,
        //                SelectedOptionsJson = selectedOptionsStr
        //            };
        //            orderList.Add(orderItem);
        //        }

        //        var orderData = new Order()
        //        {
        //            OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
        //            PhoneNumber = orderDto.PhoneNumber,
        //            TotalAmount = totalAmout,
        //            CreatedAt = DateTime.UtcNow,
        //            OrderStatus = OrderStatus.Paid.ToString()
        //        };

        //        await context.Orders.AddAsync(orderData);
        //        await context.SaveChangesAsync();

        //        foreach (var itemData in orderList)
        //        {
        //            itemData.OrderId = orderData.OrderId;
        //        }
        //        await context.OrderItems.AddRangeAsync(orderList);
        //        await context.SaveChangesAsync();

        //        var responseDto = new ResponseDtos.OrderResponseDto
        //        {
        //            OrderId = orderData.OrderId,
        //            OrderNumber = orderData.OrderNumber,
        //            TotalAmount = orderData.TotalAmount,
        //            OrderStatus = orderData.OrderStatus,
        //            PhoneNumber = orderData.PhoneNumber,
        //            CreatedAt = orderData.CreatedAt,
        //            OrderItems = orderList.Select(item => new ResponseDtos.OrderItemResponseDto
        //            {
        //                OrderItemId = item.OrderItemId,
        //                MenuId = item.MenuId,
        //                Quantity = item.Quantity,
        //                PriceAtOrder = item.PriceAtOrder,
        //                SelectedOptions = new List<ResponseDtos.SelectedOptionDto>()
        //            }).ToList()
        //        };

        //        return Ok(new DefaultResponseModel()
        //        {
        //            Success = true,
        //            Statuscode = StatusCodes.Status200OK,
        //            Message = "Placed order complete",
        //            Data = responseDto
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        //        return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponseModel()
        //        {
        //            Success = false,
        //            Statuscode = StatusCodes.Status500InternalServerError,
        //            Message = "An error occurred in PlaceOrder: " + innerMessage,
        //            Data = null
        //        });
        //    }
        //}
        [HttpPost]
        [EndpointSummary("Create Order")]
        public async Task<IActionResult> PlaceOrder(RequestDtos.OrderRequest orderDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderDto.PhoneNumber))
                {
                    return BadRequest(new DefaultResponseModel()
                    {
                        Success = false,
                        Statuscode = StatusCodes.Status400BadRequest,
                        Message = "Phone Number is required",
                        Data = null
                    });
                }

                List<OrderItem> orderList = new List<OrderItem>();
                decimal totalAmount = 0;

                // Pre-fetch all selected option IDs across all items to reduce DB roundtrips
                var allIncomingOptionIds = orderDto.Items
                    .Where(i => i.OptionItemSelectedIds != null)
                    .SelectMany(i => i.OptionItemSelectedIds!)
                    .Distinct()
                    .ToList();

                var optionsLookup = await context.OptionItems
                    .Include(oi => oi.OptionGroup)
                    .Where(oi => allIncomingOptionIds.Contains(oi.Id))
                    .ToDictionaryAsync(oi => oi.Id);

                foreach (var item in orderDto.Items)
                {
                    var menuItem = await context.Menus
                        .FirstOrDefaultAsync(p => p.MenuId == item.MenuId && p.DeletedAt == null);

                    if (menuItem == null)
                    {
                        return NotFound(new DefaultResponseModel()
                        {
                            Success = false,
                            Statuscode = StatusCodes.Status404NotFound,
                            Message = $"Menu ID {item.MenuId} doesn't exist",
                            Data = null
                        });
                    }

                    decimal singleItemPrice = (decimal)menuItem.Price;

                    if (item.OptionItemSelectedIds != null && item.OptionItemSelectedIds.Any())
                    {
                        foreach (var optionId in item.OptionItemSelectedIds)
                        {
                            if (optionsLookup.TryGetValue(optionId, out var optionData))
                            {
                                singleItemPrice += optionData.ExtraPrice;
                            }
                        }
                    }

                    totalAmount += (singleItemPrice * item.Quantity);

                    // ✔️ FIX 1: Serialize as a valid JSON Array instead of raw comma-separated string
                    var jsonString = item.OptionItemSelectedIds != null && item.OptionItemSelectedIds.Any()
                        ? JsonSerializer.Serialize(item.OptionItemSelectedIds)
                        : "[]";

                    var orderItem = new OrderItem()
                    {
                        MenuId = item.MenuId,
                        Quantity = item.Quantity,
                        PriceAtOrder = singleItemPrice,
                        SelectedOptionsJson = jsonString
                    };
                    orderList.Add(orderItem);
                }

                var orderData = new Order()
                {
                    OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    PhoneNumber = orderDto.PhoneNumber,
                    TotalAmount = totalAmount,
                    CreatedAt = DateTime.UtcNow,
                    OrderStatus = OrderStatus.Paid.ToString()
                };

                await context.Orders.AddAsync(orderData);
                await context.SaveChangesAsync();

                foreach (var itemData in orderList)
                {
                    itemData.OrderId = orderData.OrderId;
                }
                await context.OrderItems.AddRangeAsync(orderList);
                await context.SaveChangesAsync();

                // ✔️ FIX 2: Populate real DTO Data in the response instead of an empty list
                var responseDto = new ResponseDtos.OrderResponseDto
                {
                    OrderId = orderData.OrderId,
                    OrderNumber = orderData.OrderNumber,
                    TotalAmount = orderData.TotalAmount,
                    OrderStatus = orderData.OrderStatus,
                    PhoneNumber = orderData.PhoneNumber,
                    CreatedAt = orderData.CreatedAt,
                    OrderItems = orderList.Select(item => {
                        // Deserialize the stored IDs to match against pre-loaded master records
                        var optionIds = JsonSerializer.Deserialize<List<int>>(item.SelectedOptionsJson) ?? new List<int>();

                        return new ResponseDtos.OrderItemResponseDto
                        {
                            OrderItemId = item.OrderItemId,
                            MenuId = item.MenuId,
                            MenuName = context.Menus.FirstOrDefault(m => m.MenuId == item.MenuId)?.MenuName ?? "Unknown Item",
                            Quantity = item.Quantity,
                            PriceAtOrder = item.PriceAtOrder,
                            SelectedOptions = optionIds
                                .Where(id => optionsLookup.ContainsKey(id))
                                .Select(id => optionsLookup[id])
                                .Select(oi => new ResponseDtos.SelectedOptionDto
                                {
                                    OptionGroupName = oi.OptionGroup?.GroupName ?? "Unknown Group",
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
                    Message = "Placed order complete",
                    Data = responseDto
                });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred in PlaceOrder: " + innerMessage,
                    Data = null
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
                if (order.OrderStatus == "Preparing" || order.OrderStatus == "Ready")
                {
                    return BadRequest(new DefaultResponseModel()
                    {
                        Success = false,
                        Statuscode = StatusCodes.Status400BadRequest,
                        Message = "Order has already been processed or completed.",
                        Data = null
                    });
                }
                var today = DateTime.UtcNow.Date;

                bool hasOrdersInQueue = await context.Orders
        .AnyAsync(o => o.OrderStatus == "Paid" && o.CreatedAt < order.CreatedAt && o.OrderId != order.OrderId);
                bool isKitchenBusy = await context.Orders
                    .AnyAsync(o => o.OrderStatus == "Preparing" && o.CreatedAt.Date == today);
                string finalStatus = (!hasOrdersInQueue && !isKitchenBusy) ? "Preparing" : "Paid" ;
                order.OrderStatus = finalStatus;
                order.Note = $"Paid via KPay/WavePay (Txn ID: {request.TransitionId})";

                var hubPayload = new
                {
                    orderId = order.OrderId,
                    orderNumber = order.OrderNumber,
                    totalAmount = order.TotalAmount,
                    orderStatus = order.OrderStatus,
                    phoneNumber = order.PhoneNumber,
                    note = order.Note,
                    createdAt = order.CreatedAt,
                    hasOrdersInQueue=hasOrdersInQueue
                };

                await context.SaveChangesAsync();
                await hubContext.Clients.All.SendAsync("newOrderCreated",hubPayload );

                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Payment confirmed successfully. Order sent to Kitchen!",
                    Data = new
                    {
                        order = order,
                        hasOrdersInQueue = hasOrdersInQueue
                    }
                });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
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
            var statusPayload = new
            {
                orderId = orderData.OrderId,
                orderNumber = orderData.OrderNumber,
                orderStatus = orderData.OrderStatus,
                phoneNumber = orderData.PhoneNumber,
                message = "Kitchen has started preparing your order."
            };
            await hubContext.Clients.All.SendAsync("orderStatusUpdated", statusPayload);

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
            var today = DateTime.Today;
            var nextWaitingOrder = await context.Orders
            .Where(o => o.OrderStatus == "Pending" && o.CreatedAt.Date == today)
            .OrderBy(o => o.CreatedAt) 
            .FirstOrDefaultAsync();

            if (nextWaitingOrder != null)
            {
                nextWaitingOrder.OrderStatus = "Preparing";
            }
            await context.SaveChangesAsync();
            var statusPayload = new
            {
                orderId = orderData.OrderId,
                orderNumber = orderData.OrderNumber,
                orderStatus = orderData.OrderStatus,
                phoneNumber = orderData.PhoneNumber,
                message = "Order is completd and ready for pickup/delivery."
            };
            await hubContext.Clients.All.SendAsync("orderStatusUpdated", statusPayload);
            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "order is ready",
                Data = orderData
            });
        }

        [NonAction]
        private IQueryable<Order> OrderQuery(string? q, string? sortField, int order)
        {
            IQueryable<Order> query = context.Orders.AsQueryable();


            // Sorting
            if (!string.IsNullOrEmpty(sortField))
            {
                query = query.OrderBy($"{sortField} {(order > 0 ? "ascending" : "descending")}");
            }

            // Filtering

            if (!string.IsNullOrEmpty(q))
            {
                q = q.ToLower();

                query = query.Where(
                    x => x.OrderId.ToString()!.Contains(q) ||
                    (x.OrderNumber ?? string.Empty).Contains(q));

            }

            return query;
        }
    }
}
