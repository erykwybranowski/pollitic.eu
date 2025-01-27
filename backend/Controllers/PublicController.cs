using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/public")]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PublicController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/public/countries
        [HttpGet("countries")]
        public async Task<ActionResult<IEnumerable<Country>>> GetAllCountries()
        {
            return await _context.Countries.ToListAsync();
        }

        // GET: api/public/parties
        [HttpGet("parties")]
        public async Task<ActionResult<IEnumerable<Party>>> GetAllParties()
        {
            return await _context.Parties
                .Include(p => p.Groups)
                .Include(p => p.SubParties)
                .ToListAsync();
        }

        // GET: api/public/parties/{countryCode}
        [HttpGet("parties/{countryCode}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetPartiesByCountry(string countryCode)
        {
            var parties = await _context.Parties
                .Where(p => p.CountryCode == countryCode)
                .Include(p => p.Groups)
                .Include(p => p.SubParties)
                .ToListAsync();

            if (parties == null || !parties.Any())
            {
                return NotFound($"No parties found for country code: {countryCode}");
            }

            return parties;
        }

        // GET: api/public/polls
        [HttpGet("polls")]
        public async Task<ActionResult<IEnumerable<Poll>>> GetAllPolls()
        {
            return await _context.Polls
                .Include(p => p.Results)
                .ThenInclude(r => r.Party)
                .ToListAsync();
        }

        // GET: api/public/polls/{countryCode}
        [HttpGet("polls/{countryCode}")]
        public async Task<ActionResult<IEnumerable<Poll>>> GetPollsByCountry(string countryCode)
        {
            var polls = await _context.Polls
                .Where(p => p.Results.Any(r => r.Party.CountryCode == countryCode))
                .Include(p => p.Results)
                .ThenInclude(r => r.Party)
                .ToListAsync();

            if (polls == null || !polls.Any())
            {
                return NotFound($"No polls found for country code: {countryCode}");
            }

            return polls;
        }

        // GET: api/public/groups
        [HttpGet("groups")]
        public async Task<ActionResult<IEnumerable<Group>>> GetAllGroups()
        {
            return await _context.Groups.ToListAsync();
        }
    }
}
