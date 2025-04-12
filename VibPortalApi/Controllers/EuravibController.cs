using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VibPortalApi.Models;
using VibPortalApi.Models.DB2Models;
using VibPortalApi.Services;

namespace VibPortalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EuravibController : ControllerBase
    {
        private readonly IEuravibService _euravibService;

        public EuravibController(IEuravibService euravibService)
        {
            _euravibService = euravibService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EuravibImport>>> GetAll()
        {
            var records = await _euravibService.GetAllAsync();
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _euravibService.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EuravibImport model)
        {
            var updated = await _euravibService.UpdateAsync(id, model);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{supplNr}/{revDate}/{dimset}")]
        public async Task<IActionResult> Delete(string supplNr, DateTime revDate, string dimset)
        {
            var deleted = await _euravibService.DeleteAsync(supplNr, revDate, dimset);
            if (!deleted) return NotFound();
            return NoContent();
        }
        [HttpPost("paged")]
        public async Task<ActionResult<VibPagedResult<EuravibImport>>> GetPaged([FromBody] PagedRequest request)
        {
            var result = await _euravibService.GetPagedAsync(request);
            return Ok(result);
        }
    }
}
