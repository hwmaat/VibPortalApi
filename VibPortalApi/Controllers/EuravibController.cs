using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VibPortalApi.Data;
using VibPortalApi.Models;

namespace VibPortalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EuravibController : ControllerBase
    {
        private readonly DB2Context _db2Context;

        public EuravibController(DB2Context db2Context)
        {
            _db2Context = db2Context;
        }

        // GET: /Euravib/GetEuravibImport
        [HttpGet("GetEuravibImport")]
        public async Task<IActionResult> GetEuravibImport()
        {
            // Retrieve data from the DB2 database using EF Core.
            var result = await _db2Context.EuravibImport.Take(10).ToListAsync();
            return Ok(result);
        }
    }
}