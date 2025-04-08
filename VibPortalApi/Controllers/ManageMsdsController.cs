using Microsoft.AspNetCore.Mvc;
using VibPortalApi.Models;
using VibPortalApi.Services;

namespace VibPortalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManageMsdsController : ControllerBase
    {
        private readonly IManageMsdsService _manageMsdsService;

        public ManageMsdsController(IManageMsdsService manageMsdsService)
        {
            _manageMsdsService = manageMsdsService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _manageMsdsService.GetAllAsync();
            return Ok(items);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _manageMsdsService.GetByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            return Ok(record);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VibImport updated)
        {
            if (id != updated.Id)
                return BadRequest("ID mismatch");

            var result = await _manageMsdsService.UpdateAsync(updated);
            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<VibImport>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string sortColumn = "Entry_Date",
            [FromQuery] string sortDirection = "desc",
            [FromQuery] string? filter = null,
            [FromQuery] string? status = null)
        {
            var result = await _manageMsdsService.GetPagedAsync(page, pageSize, sortColumn, sortDirection, filter, status);
            return Ok(result);
        }

        [HttpGet("parse-filename")]
        public IActionResult ParseFileName([FromQuery] string fileName)
        {
            var (supplierCode, dimset, recipe) = _manageMsdsService.ParseFileName(fileName);
            return Ok(new
            {
                SupplierCode = supplierCode,
                Dimset = dimset,
                Recipe = recipe
            });
        }



    }
}
