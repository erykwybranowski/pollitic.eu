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
        public async Task<ActionResult<IEnumerable<CountryDTO>>> GetAllCountries()
        {
            return await _context.Countries
                .Select(c => new CountryDTO
                {
                    CountryCode = c.CountryCode,
                    Name = c.Name
                })
                .ToListAsync();
        }

        // GET: api/public/parties
        [HttpGet("parties")]
        public async Task<ActionResult<IEnumerable<PartyDTO>>> GetAllParties()
        {
            return await _context.Parties
                .Include(p => p.Groups)
                .Include(p => p.SubParties)
                .Select(p => new PartyDTO
                {
                    Id = p.Id,
                    StringId = p.StringId,
                    Acronym = p.Acronym,
                    EnglishName = p.EnglishName,
                    CountryCode = p.CountryCode,
                    LocalName = p.LocalName,
                    CHES_EU = p.CHES_EU,
                    CHES_Economy = p.CHES_Economy,
                    CHES_Progress = p.CHES_Progress,
                    CHES_Liberal = p.CHES_Liberal,
                    Mp = p.Mp,
                    Role = p.Role,
                    Groups = p.Groups.Select(g => new GroupDTO
                    {
                        Id = g.Id,
                        Acronym = g.Acronym,
                        Name = g.Name,
                        R = g.R,
                        G = g.G,
                        B = g.B
                    }).ToList(),
                    SubParties = p.SubParties.Select(sp => new PartyDTO
                    {
                        Id = sp.Id,
                        StringId = sp.StringId,
                        Acronym = sp.Acronym,
                        EnglishName = sp.EnglishName,
                        CountryCode = sp.CountryCode
                    }).ToList()
                })
                .ToListAsync();
        }
        
        // GET: api/public/parties/{countryCode}
        [HttpGet("parties/{countryCode}")]
        public async Task<ActionResult<IEnumerable<PartyDTO>>> GetPartiesByCountry(string countryCode)
        {
            return await _context.Parties
                .Where(p => p.CountryCode == countryCode)
                .Include(p => p.Groups)
                .Include(p => p.SubParties)
                .Select(p => new PartyDTO
                {
                    Id = p.Id,
                    StringId = p.StringId,
                    Acronym = p.Acronym,
                    EnglishName = p.EnglishName,
                    CountryCode = p.CountryCode,
                    LocalName = p.LocalName,
                    CHES_EU = p.CHES_EU,
                    CHES_Economy = p.CHES_Economy,
                    CHES_Progress = p.CHES_Progress,
                    CHES_Liberal = p.CHES_Liberal,
                    Mp = p.Mp,
                    Role = p.Role,
                    Groups = p.Groups.Select(g => new GroupDTO
                    {
                        Id = g.Id,
                        Acronym = g.Acronym,
                        Name = g.Name,
                        R = g.R,
                        G = g.G,
                        B = g.B
                    }).ToList(),
                    SubParties = p.SubParties.Select(sp => new PartyDTO
                    {
                        Id = sp.Id,
                        StringId = sp.StringId,
                        Acronym = sp.Acronym,
                        EnglishName = sp.EnglishName,
                        CountryCode = sp.CountryCode
                    }).ToList()
                })
                .ToListAsync();
        }

        // GET: api/public/polls
        [HttpGet("polls")]
        public async Task<ActionResult<IEnumerable<PollDTO>>> GetAllPolls()
        {
            return await _context.Polls
                .Include(p => p.Results)
                .Select(p => new PollDTO
                {
                    Id = p.Id,
                    Pollster = p.Pollster,
                    Media = p.Media,
                    StartDate = p.StartDate,
                    FinishDate = p.FinishDate,
                    Type = p.Type,
                    Sample = p.Sample,
                    Others = p.Others,
                    Area = p.Area,
                    Results = p.Results.Select(r => new PollResultDTO
                    {
                        PartyId = r.PartyId,
                        Value = r.Value
                    }).ToList()
                })
                .ToListAsync();
        }

        // GET: api/public/polls/{countryCode}
        [HttpGet("polls/{countryCode}")]
        public async Task<ActionResult<IEnumerable<PollDTO>>> GetPollsByCountry(string countryCode)
        {
            return await _context.Polls
                .Where(p => p.Results.Any(r => r.Party.CountryCode == countryCode))
                .Include(p => p.Results)
                .Select(p => new PollDTO
                {
                    Id = p.Id,
                    Pollster = p.Pollster,
                    Media = p.Media,
                    StartDate = p.StartDate,
                    FinishDate = p.FinishDate,
                    Type = p.Type,
                    Sample = p.Sample,
                    Others = p.Others,
                    Area = p.Area,
                    Results = p.Results.Select(r => new PollResultDTO
                    {
                        PartyId = r.PartyId,
                        Value = r.Value
                    }).ToList()
                })
                .ToListAsync();
        }

        // GET: api/public/groups
        [HttpGet("groups")]
        public async Task<ActionResult<IEnumerable<GroupDTO>>> GetAllGroups()
        {
            return await _context.Groups
                .Select(g => new GroupDTO
                {
                    Id = g.Id,
                    Acronym = g.Acronym,
                    Name = g.Name,
                    R = g.R,
                    G = g.G,
                    B = g.B
                })
                .ToListAsync();
        }
    }
}
