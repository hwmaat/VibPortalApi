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

        [HttpGet("{supplNr}/{revDate}/{dimset}")]
        public async Task<ActionResult<EuravibImport>> GetById(string supplNr, DateTime revDate, string dimset)
        {
            var record = await _euravibService.GetByIdAsync(supplNr, revDate, dimset);
            if (record == null) return NotFound();
            return Ok(record);
        }

        [HttpPut("{supplNr}/{revDate}/{dimset}")]
        public async Task<IActionResult> Update(string supplNr, DateTime revDate, string dimset, EuravibImport record)
        {
            if (record.Suppl_Nr != supplNr || record.Rev_Date != revDate || record.Dimset != dimset)
                return BadRequest("Composite key mismatch.");

            var updated = await _euravibService.UpdateAsync(record);
            if (!updated) return NotFound();

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
