using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Models;
using OrderService.Application.Services;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly Serilog.ILogger _logger;

        public OrdersController(IOrderService orderService, Serilog.ILogger logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // We keep it simple for now: get employee id from X-Employee-Id header.
        // Later you can swap this to JWT claims (User.Claims) without changing the service layer.
        private Guid GetEmployeeId()
        {
            if (Request.Headers.TryGetValue("X-Employee-Id", out var header) &&
                Guid.TryParse(header, out var employeeId))
            {
                return employeeId;
            }

            throw new InvalidOperationException("X-Employee-Id header is required and must be a valid GUID.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll(CancellationToken cancellationToken)
        {
            var employeeId = GetEmployeeId();
            var orders = await _orderService.GetForEmployeeAsync(employeeId, cancellationToken);
            return Ok(orders);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var employeeId = GetEmployeeId();
            var order = await _orderService.GetByIdAsync(id, employeeId, cancellationToken);

            if (order is null)
                return NotFound();

            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create(
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employeeId = GetEmployeeId();
            var created = await _orderService.CreateAsync(request, employeeId, cancellationToken);

            if (created is null)
            {
                // Most likely: product not found in ProductService
                return BadRequest("Invalid ProductId or unable to calculate price.");
            }

            _logger.Information("Order {OrderId} created by employee {EmployeeId}", created.Id, employeeId);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateOrderRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employeeId = GetEmployeeId();
            var success = await _orderService.UpdateAsync(id, request, employeeId, cancellationToken);

            if (!success)
                return NotFound();

            _logger.Information("Order {OrderId} updated by employee {EmployeeId}", id, employeeId);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var employeeId = GetEmployeeId();
            var success = await _orderService.DeleteAsync(id, employeeId, cancellationToken);

            if (!success)
                return NotFound();

            _logger.Information("Order {OrderId} deleted by employee {EmployeeId}", id, employeeId);

            return NoContent();
        }
    }
}
