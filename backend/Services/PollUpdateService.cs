using System.Globalization;
using System.Text.RegularExpressions;

namespace backend.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Data;
using Models;

public class PollUpdateService : BackgroundService
{
    private readonly ILogger<PollUpdateService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    public PollUpdateService(
        ILogger<PollUpdateService> logger, 
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run a continuous loop that checks once per day.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckForNewPollsAsync(stoppingToken);
                await UpdateLatestPollsCacheAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking for new polls.");
            }
            Console.WriteLine("Finished Poll Update");

            // Wait for 24 hours (adjust if needed)
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task CheckForNewPollsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Retrieve all countries from the database.
        var countries = await dbContext.Countries.ToListAsync(stoppingToken);
        var parties = await dbContext.Parties.ToListAsync(stoppingToken);
        
        foreach (var country in countries)
        {
            // Build the raw GitHub URL for the .ropf file using the country's CountryCode.
            // Example: for "de" the URL will be:
            // https://raw.githubusercontent.com/Europe-Elects/apopod/main/de.ropf
            var fileUrl = $"https://raw.githubusercontent.com/Europe-Elects/apopod/main/{country.CountryCode.ToLower()}.ropf";

            // Get the file content from GitHub.
            string fileContent = await GetRopfFileContentAsync(fileUrl, stoppingToken);
            if (string.IsNullOrEmpty(fileContent))
            {
                _logger.LogWarning($"No content retrieved from URL: {fileUrl}");
                continue;
            }

            using var reader = new StringReader(fileContent);
            string? line;
            Poll currentPoll = null;
            
            var candidatePolls = await dbContext.Polls
                .Include(p => p.Results)
                .Where(p => p.Results.First().Party.CountryCode == country.CountryCode)
                .ToListAsync(stoppingToken);

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    break;

                // Convert the line into a Poll object.
                Poll? poll = null;
                if (line.StartsWith("•PF:"))
                {
                    poll = CreatePollFromLine(line, parties, country.CountryCode, dbContext);
                    currentPoll = poll;
                }
                else if (line.StartsWith("&") && currentPoll != null)
                {
                    // Create a new poll based on the previous one
                    poll = CreatePollFromLine(line, parties, country.CountryCode, dbContext, currentPoll);
                }
                
                if (poll == null)
                {
                    _logger.LogWarning($"Failed to parse poll from line: {line}");
                    continue;
                }

                // Check whether this poll already exists in the database.
                bool exists = await PollExistsAsync(candidatePolls, poll, stoppingToken);
                if (exists)
                {
                    // Since new polls are added at the top, reaching an existing poll means
                    // the rest of the file contains already-processed polls.
                    break;
                }
                else
                {
                    // New poll found; add it to the DbContext.
                    dbContext.Polls.Add(poll);
                    // If needed, add associated PollResults or other related entities here.
                }
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    /// <summary>
    /// Uses HttpClient to fetch the content of a .ropf file from the given URL.
    /// </summary>
    private async Task<string> GetRopfFileContentAsync(string fileUrl, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(fileUrl, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        else
        {
            _logger.LogWarning($"Failed to fetch file from URL: {fileUrl}. Status Code: {response.StatusCode}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Parses a single line from a .ropf file into a Poll object.
    /// </summary>
    private Poll CreatePollFromLine(string line, List<Party> parties, string countryCode, AppDbContext dbContext, Poll basePoll = null)
    {
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Pollster = basePoll?.Pollster ?? ExtractField(line, "•PF:") ?? "Unknown",
            Media = basePoll?.Media.ToList() ?? new List<string>(),
            StartDate = basePoll?.StartDate ?? DateTime.Parse(ExtractField(line, "•FS:") ?? ExtractField(line, "•FE:") ?? ExtractField(line, "•PD:")),
            FinishDate = basePoll?.FinishDate ?? DateTime.Parse(ExtractField(line, "•FE:") ?? ExtractField(line, "•PD:")),
            Type = basePoll?.Type ?? ExtractField(line, "•SC:") ?? "N",
            Sample = basePoll?.Sample ?? ExtractNumberField(line, "•SS:") ?? null,
            Results = ExtractResults(line, parties, countryCode, dbContext),
            Others = ExtractDoubleField(line, "•O:") ?? 0,
            Area = ExtractField(line, "•A:") ?? string.Empty
        };

        // Add additional media if found in the current line
        AddMultipleFields(line, "•C:", poll.Media);
            
        return poll;
    }
    
    private string? ExtractField(string line, string marker)
    {
        var regex = new Regex($@"{marker}(.*?)(?=•|$|\s+\S+?:)");
        var match = regex.Match(line);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
    
    private int? ExtractNumberField(string line, string marker)
    {
        var valueStr = ExtractField(line, marker);
        return valueStr != null ? int.Parse(new string(valueStr.Where(char.IsDigit).ToArray())) : null;
    }
    private double? ExtractDoubleField(string line, string marker)
    {
        var valueStr = ExtractField(line, marker);
        return valueStr != null ? double.Parse(valueStr, CultureInfo.InvariantCulture) : null;
    }
    
    private void AddMultipleFields(string line, string marker, List<string> targetList)
    {
        var regex = new Regex($"{marker}(.*?)(?=•|$)", RegexOptions.Compiled);
        var matches = regex.Matches(line);

        foreach (Match match in matches)
        {
            targetList.Add(match.Groups[1].Value.Trim());
        }
    }
    
    List<PollResult> ExtractResults(string line, List<Party> parties, string countryCode, AppDbContext dbContext)
    {
        var results = new List<PollResult>();
        var regex = new Regex(@"\s([\p{L}\w\-+]+):\s+(\d+(\.\d+)?)", RegexOptions.Compiled);
        var matches = regex.Matches(line);

        double totalSupport = 0;
        var tempResults = new List<(Party Party, double Value)>();

        foreach (Match match in matches)
        {
            var acronym = match.Groups[1].Value.Trim();
            var value = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            totalSupport += value;

            var party = parties.FirstOrDefault(p => p.StringId == acronym && p.CountryCode == countryCode);
            if (party != null)
            {
                tempResults.Add((party, value));
            }
            else if (acronym.Contains("+"))
            {
                var subAcronyms = acronym.Split("+").Select(sub => sub.Trim());
                var allParties = subAcronyms
                    .Select(sub =>
                    {
                        var party = parties.FirstOrDefault(p => p.StringId == sub && p.CountryCode == countryCode);
                        if (party == null)
                        {
                            // Call your CreateParty method to create the missing party.
                            party = CreateParty(sub, countryCode);
                            parties.Add(party);
                            dbContext.Parties.Add(party);
                        }
                        return party;
                    })
                    .ToList();
                
                if (allParties.Any())
                {
                    var newParty = CreateCompositeParty(allParties, acronym);
                    tempResults.Add((newParty, value));
                    parties.Add(newParty);
                }
            }
            else
            {
                party = CreateParty(acronym, countryCode);
                parties.Add(party);
                dbContext.Parties.Add(party);
                tempResults.Add((party, value));
            }
        }

        foreach (var tempResult in tempResults)
        {
            var normalizedValue = (tempResult.Value / totalSupport) * 100;
            results.Add(new PollResult
            {
                Party = tempResult.Party,
                Value = Math.Round(normalizedValue, 1)
            });
        }

        return results;
    }

    Party CreateCompositeParty(List<Party> subParties, string acronym)
    {
        // Ensure that if any sub-party has a null Groups or Role, we use an empty sequence instead.
        var groups = subParties.SelectMany(p => p.Groups ?? Enumerable.Empty<Group>()).ToHashSet();
        var roles = subParties.SelectMany(p => p.Role ?? Enumerable.Empty<string>()).ToHashSet();

        var chesData = GetSubPartiesCHESData(subParties);

        return new Party
        {
            Id = Guid.NewGuid(),
            Acronym = acronym,
            StringId = acronym,
            EnglishName = "Composite-party",
            // Properly create a list of strings for LocalName.
            LocalName = new List<string> { acronym },
            Groups = groups,
            Role = roles,
            SubParties = subParties,
            CountryCode = subParties.First().CountryCode,
            CHES_EU = chesData[0],
            CHES_Economy = chesData[1],
            CHES_Progress = chesData[2],
            CHES_Liberal = chesData[3],
            Mp = subParties.Sum(p => p.Mp ?? 0)
        };
    }
    
    Party CreateParty(string acronym, string countryCode)
    {
        return new Party
        {
            Id = Guid.NewGuid(),
            Acronym = acronym,
            StringId = acronym,
            EnglishName = acronym,
            LocalName = [acronym],
            Groups = new HashSet<Group>(),
            Role = new HashSet<string>(),
            SubParties = new HashSet<Party>(),
            CountryCode = countryCode,
            CHES_EU = null,
            CHES_Economy = null,
            CHES_Progress = null,
            CHES_Liberal = null,
            Mp = null
        };
    }
    
    private List<List<double>> GetSubPartiesCHESData(ICollection<Party> subParties)
    {
        var chesEU = new List<double>();
        var chesEconomy = new List<double>();
        var chesProgress = new List<double>();
        var chesLiberal = new List<double>();

        void AddValueToRange(List<double> range, object? value)
        {
            if (value is List<double> list)
            {
                range.AddRange(list);
            }
            else if (value is double singleValue)
            {
                range.Add(singleValue);
            }
        }

        foreach (var subParty in subParties)
        {
            if (subParty.CHES_EU != null) AddValueToRange(chesEU, subParty.CHES_EU);
            if (subParty.CHES_Economy != null) AddValueToRange(chesEconomy, subParty.CHES_Economy);
            if (subParty.CHES_Progress != null) AddValueToRange(chesProgress, subParty.CHES_Progress);
            if (subParty.CHES_Liberal != null) AddValueToRange(chesLiberal, subParty.CHES_Liberal);
        }

        List<double> GetRange(List<double> values)
        {
            if (values.Count == 0) return null;
            var min = values.Min();
            var max = values.Max();
            return min == max ? [min] : [min, max];
        }

        return new List<List<double>> 
        { 
            GetRange(chesEU), 
            GetRange(chesEconomy), 
            GetRange(chesProgress), 
            GetRange(chesLiberal) 
        };
    }
    
    /// <summary>
    /// Checks if a poll already exists in the database by comparing its attributes (except the Id).
    /// Adjust the comparison logic as needed.
    /// </summary>
    private async Task<bool> PollExistsAsync(List<Poll> polls, Poll poll, CancellationToken cancellationToken)
    {
        // Filter candidate polls by comparing the key fields and using the Date property for date fields.
        var candidatePoll = polls.FirstOrDefault(p =>
            p.Pollster == poll.Pollster &&
            p.StartDate.Date == poll.StartDate.Date &&      // Compare dates rounded to day
            p.FinishDate.Date == poll.FinishDate.Date &&    // Compare dates rounded to day
            p.Type == poll.Type &&
            p.Sample == poll.Sample &&
            Math.Abs(p.Others - poll.Others) < 0.0001 &&     // Tolerance for numeric comparison
            p.Area == poll.Area
        );

        if (candidatePoll == null)
        {
            // No poll with matching attributes found.
            return false;
        }

        // Compare poll results: first check the count.
        if (candidatePoll.Results.Count != poll.Results.Count)
        {
            return false;
        }

        // Sort poll results by PartyId (adjust if needed)
        var candidateResults = candidatePoll.Results.OrderBy(r => r.Party.Acronym).ToList();
        var newPollResults = poll.Results.OrderBy(r => r.Party.Acronym).ToList();

        // Compare each poll result.
        for (int i = 0; i < candidateResults.Count; i++)
        {
            if (candidateResults[i].Party.Acronym != newPollResults[i].Party.Acronym ||
                Math.Abs(candidateResults[i].Value - newPollResults[i].Value) > 0.0001)
            {
                return false;
            }
        }

        // All checks passed: the poll already exists.
        return true;
    }
    
    private async Task UpdateLatestPollsCacheAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Get all countries
        var countries = await _context.Countries.ToListAsync(cancellationToken);

        foreach (var country in countries)
        {
            // Get the newest poll for this country
            var newestPoll = await _context.Polls
                .Where(p => p.Results.Any(r => r.Party.CountryCode == country.CountryCode))
                .OrderByDescending(p => p.FinishDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (newestPoll != null)
            {
                // Try to find an existing LatestPoll entry by country code
                var existing = await _context.LatestPolls.FindAsync(new object[] { country.CountryCode }, cancellationToken);
                if (existing != null)
                {
                    existing.PollId = newestPoll.Id;
                    // Optionally update other related properties if needed.
                    _context.Update(existing);
                }
                else
                {
                    var newEntry = new LatestPoll
                    {
                        CountryCode = country.CountryCode,
                        PollId = newestPoll.Id
                    };
                    await _context.LatestPolls.AddAsync(newEntry, cancellationToken);
                }
            }
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

}
