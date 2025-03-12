using DataLayerObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RCM.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly RetailChainContext _context;
        public WarehousesController(RetailChainContext context)
        {
            _context = context;
        }
        [HttpGet("GetWarehouses")]
        public IActionResult GetWarehouses()
        {
            var warehouses =  _context.Warehouses
                .Select(w => new
                {
                    w.Id,
                    w.Name,
                    w.Capacity
                })
                .ToList();

            return Ok(warehouses);
        }

    }
}
