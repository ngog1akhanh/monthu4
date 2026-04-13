using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.API.Data;
using VinhKhanh.Shared.Models;

namespace VinhKhanh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class POIsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public POIsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/POIs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetPOIs()
        {
            return await _context.POIs.ToListAsync();
        }

        // GET: api/POIs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<POI>> GetPOI(int id)
        {
            var pOI = await _context.POIs.FindAsync(id);

            if (pOI == null)
            {
                return NotFound();
            }

            return pOI;
        }

        // PUT: api/POIs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPOI(int id, POI pOI)
        {
            if (id != pOI.Id)
            {
                return BadRequest();
            }

            _context.Entry(pOI).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!POIExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/POIs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<POI>> PostPOI(POI pOI)
        {
            _context.POIs.Add(pOI);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPOI", new { id = pOI.Id }, pOI);
        }

        // DELETE: api/POIs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePOI(int id)
        {
            var pOI = await _context.POIs.FindAsync(id);
            if (pOI == null)
            {
                return NotFound();
            }

            _context.POIs.Remove(pOI);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool POIExists(int id)
        {
            return _context.POIs.Any(e => e.Id == id);
        }
    }
}
