using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using SoftITOFlix.Data;
using SoftITOFlix.Models;

namespace SoftITOFlix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediasController : ControllerBase
    {
        private readonly SoftITOFlixContext _context;

        public MediasController(SoftITOFlixContext context)
        {
            _context = context;
        }

        // GET: api/Medias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Media>>> GetMedias()
        {
          if (_context.Medias == null)
          {
              return NotFound();
          }
            return await _context.Medias.ToListAsync();
        }

        // GET: api/Medias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Media>> GetMedia(int id, bool withLinks)
        {
          if (_context.Medias == null)
          {
              return NotFound();
          }
            var media = await _context.Medias.FindAsync(id);

            if (media == null)
            {
                return NotFound();
            }

            return media;
        }

        // PUT: api/Medias/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedia(int id, Media media)
        {
            if (id != media.Id)
            {
                return BadRequest();
            }

            _context.Entry(media).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MediaExists(id))
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

        // POST: api/Medias
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Media>> PostMedia(Media media)
        {
          if (_context.Medias == null)
          {
              return Problem("Entity set 'SoftITOFlixContext.Medias'  is null.");
          }
            _context.Medias.Add(media);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMedia", new { id = media.Id }, media);
        }

        // DELETE: api/Medias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedia(int id)
        {
            if (_context.Medias == null)
            {
                return NotFound();
            }
            var media = await _context.Medias.FindAsync(id);
            if (media == null)
            {
                return NotFound();
            }

            _context.Medias.Remove(media);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MediaExists(int id)
        {
            return (_context.Medias?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
