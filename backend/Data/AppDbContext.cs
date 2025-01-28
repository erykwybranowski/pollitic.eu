using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Group = backend.Models.Group;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Party> Parties { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Poll> Polls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Country Configuration
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.CountryCode);

                entity.Property(e => e.CountryCode)
                      .IsRequired()
                      .HasColumnType("varchar(10)");

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasColumnType("varchar(255)");

                entity.ToTable("Countries");
            });

            // Group Configuration
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd()
                      .HasColumnType("int");

                entity.Property(e => e.Acronym)
                      .IsRequired()
                      .HasColumnType("varchar(10)");

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasColumnType("varchar(255)");

                entity.Property(e => e.R)
                      .IsRequired()
                      .HasColumnType("int");

                entity.Property(e => e.G)
                      .IsRequired()
                      .HasColumnType("int");

                entity.Property(e => e.B)
                      .IsRequired()
                      .HasColumnType("int");

                entity.ToTable("Groups");
            });

            // Party Configuration
            modelBuilder.Entity<Party>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd()
                      .HasColumnType("char(36)");

                entity.Property(e => e.StringId)
                      .IsRequired()
                      .HasColumnType("varchar(255)");

                entity.Property(e => e.Acronym)
                      .HasColumnType("varchar(50)");

                entity.Property(e => e.EnglishName)
                      .IsRequired()
                      .HasColumnType("varchar(255)");

                entity.Property(e => e.CountryCode)
                      .IsRequired()
                      .HasColumnType("varchar(10)");

                entity.Property(e => e.Mp)
                      .HasColumnType("int");
                
                // Serialize Role to a JSON string
                entity.Property(e => e.Role)
                    .HasConversion(
                        v => string.Join(",", v), // Convert ICollection<string> to CSV
                        v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()) // Convert CSV to ICollection<string>
                    .HasColumnType("longtext");

                // Serialize LocalName to a JSON string
                entity.Property(e => e.LocalName)
                    .HasConversion(
                        v => string.Join(",", v), // Convert List<string> to CSV
                        v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()) // Convert CSV to List<string>
                    .HasColumnType("longtext");

                // Serialize CHES attributes to JSON strings
                entity.Property(e => e.CHES_EU)
                    .HasConversion(
                        v => string.Join(",", v.Select(x => x.ToString())), // Convert List<double> to CSV
                        v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToList()) // Convert CSV to List<double>
                    .HasColumnType("longtext");

                entity.Property(e => e.CHES_Economy)
                    .HasConversion(
                        v => string.Join(",", v.Select(x => x.ToString())),
                        v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToList())
                    .HasColumnType("longtext");

                entity.Property(e => e.CHES_Progress)
                    .HasConversion(
                        v => string.Join(",", v.Select(x => x.ToString())),
                        v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToList())
                    .HasColumnType("longtext");

                entity.Property(e => e.CHES_Liberal)
                    .HasConversion(
                        v => string.Join(",", v.Select(x => x.ToString())),
                        v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToList())
                    .HasColumnType("longtext");
                
                entity.HasOne(p => p.Country)
                    .WithMany(c => c.Parties)
                    .HasForeignKey(p => p.CountryCode)
                    .HasPrincipalKey(c => c.CountryCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable("Parties");
            });

            // Poll Configuration
            modelBuilder.Entity<Poll>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd()
                      .HasColumnType("char(36)");

                entity.Property(e => e.Pollster)
                      .IsRequired()
                      .HasColumnType("varchar(255)");

                entity.Property(e => e.StartDate)
                      .IsRequired()
                      .HasColumnType("datetime");

                entity.Property(e => e.FinishDate)
                      .IsRequired()
                      .HasColumnType("datetime");

                entity.Property(e => e.Type)
                      .IsRequired()
                      .HasColumnType("varchar(50)");

                entity.Property(e => e.Sample)
                      .HasColumnType("int");

                entity.Property(e => e.Others)
                      .HasColumnType("double");

                entity.Property(e => e.Area)
                      .HasColumnType("varchar(255)");
                
                // Serialize Media to a JSON string
                entity.Property(e => e.Media)
                    .HasConversion(
                        v => string.Join(",", v), // Convert List<string> to CSV
                        v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()) // Convert CSV to List<string>
                    .HasColumnType("longtext");

                entity.ToTable("Polls");
            });

            // PollResult Configuration
            modelBuilder.Entity<PollResult>(entity =>
            {
                // Define composite key
                entity.HasKey(pr => new { pr.PollId, pr.PartyId });

                // Configure the relationship to Poll
                entity.HasOne(pr => pr.Poll)
                    .WithMany(p => p.Results) // Matches Poll.Results
                    .HasForeignKey(pr => pr.PollId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure the relationship to Party
                entity.HasOne(pr => pr.Party)
                    .WithMany() // No navigation property on Party
                    .HasForeignKey(pr => pr.PartyId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(pr => pr.Value)
                    .IsRequired()
                    .HasColumnType("double");

                entity.ToTable("PollResults");
            });

            // Relationships
            modelBuilder.Entity<Party>()
                .HasMany(p => p.Groups)
                .WithMany(g => g.Parties)
                .UsingEntity<Dictionary<string, object>>(
                    "PartyGroups",
                    join => join.HasOne<Group>().WithMany().HasForeignKey("GroupId").OnDelete(DeleteBehavior.Cascade),
                    join => join.HasOne<Party>().WithMany().HasForeignKey("PartyId").OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("PartyGroups");
                        join.HasKey("PartyId", "GroupId");
                    });

            // Party-SubParties Relationship (Many-to-Many Self-Referencing)
            modelBuilder.Entity<Party>()
                .HasMany(p => p.SubParties)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "PartySubParties",
                    join => join.HasOne<Party>() // SubParty
                        .WithMany()
                        .HasForeignKey("SubPartyId")
                        .OnDelete(DeleteBehavior.Restrict),
                    join => join.HasOne<Party>() // ParentParty
                        .WithMany()
                        .HasForeignKey("ParentPartyId")
                        .OnDelete(DeleteBehavior.Restrict),
                    join =>
                    {
                        join.ToTable("PartySubParties");
                        join.HasKey("ParentPartyId", "SubPartyId");
                    });
            
            modelBuilder.Entity<Country>()
                .HasMany<Party>()
                .WithOne()
                .HasForeignKey(p => p.CountryCode)
                .HasPrincipalKey(c => c.CountryCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Initialize Countries
            modelBuilder.Entity<Country>().HasData(new[]
            {
                new Country {CountryCode = "at", Name = "Austria" },
                new Country {CountryCode = "be", Name = "Belgia" },
                new Country {CountryCode = "bg", Name = "Bułgaria"},
                new Country {CountryCode = "hr", Name = "Chorwacja"},
                new Country {CountryCode = "cy", Name = "Cypr"},
                new Country {CountryCode = "cz", Name = "Czechy"},
                new Country {CountryCode = "dk", Name = "Dania"},
                new Country {CountryCode = "ee", Name = "Estonia"},
                new Country {CountryCode = "fi", Name = "Finlandia"},
                new Country {CountryCode = "fr", Name = "Francja"},
                new Country {CountryCode = "gr", Name = "Grecja"},
                new Country {CountryCode = "es", Name = "Hiszpania"},
                new Country {CountryCode = "nl", Name = "Holandia"},
                new Country {CountryCode = "ie", Name = "Irlandia"},
                new Country {CountryCode = "lt", Name = "Litwa"},
                new Country {CountryCode = "lu", Name = "Luksemburg"},
                new Country {CountryCode = "lv", Name = "Łotwa"},
                new Country {CountryCode = "mt", Name = "Malta"},
                new Country {CountryCode = "de", Name = "Niemcy"},
                new Country {CountryCode = "pl", Name = "Polska"},
                new Country {CountryCode = "pt", Name = "Portugalia"},
                new Country {CountryCode = "ro", Name = "Rumunia"},
                new Country {CountryCode = "sk", Name = "Słowacja"},
                new Country {CountryCode = "si", Name = "Słowenia"},
                new Country {CountryCode = "se", Name = "Szwecja"},
                new Country {CountryCode = "hu", Name = "Węgry"},
                new Country {CountryCode = "it", Name = "Włochy"}
            });

            // Initialize Groups
            List<Group> groups = new List<Group>()
            {
                new Group
                {
                    Id = -8, Acronym = "LEFT", Name = "Progressive Alliance of Socialists and Democrats", R = 138,
                    G = 21, B = 28
                },
                new Group
                {
                    Id = -7, Acronym = "S&D", Name = "Progressive Alliance of Socialists and Democrats", R = 219,
                    G = 58, B = 46
                },
                new Group
                {
                    Id = -6, Acronym = "GREENS", Name = "Greens/European Free Alliance", R = 27, G = 209, B = 36
                },
                new Group { Id = -5, Acronym = "RE", Name = "Renew Europe", R = 238, G = 230, B = 1 },
                new Group { Id = -4, Acronym = "EPP", Name = "European People's Party", R = 52, G = 143, B = 235 },
                new Group
                {
                    Id = -3, Acronym = "ECR", Name = "European Conservatives and Reformists", R = 39, G = 44, B = 186
                },
                new Group { Id = -2, Acronym = "PfE", Name = "Patriots for Europe", R = 76, G = 48, B = 122 },
                new Group { Id = -1, Acronym = "ESN", Name = "Europe of Sovereign Nations", R = 9, G = 52, B = 92 }
            };
            modelBuilder.Entity<Group>().HasData(groups);

            // Parse .ropf files for Parties and Polls
            var parties = new List<Party>();
            var polls = new List<Poll>();
            
            var assetsFolder = Path.Combine(Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName, "frontend", "public", "assets");

            foreach (var country in modelBuilder.Entity<Country>().Metadata.GetSeedData())
            {
                var countryCode = country["CountryCode"]?.ToString();
                if (string.IsNullOrEmpty(countryCode)) continue;

                var ropfFilePath = Path.Combine(assetsFolder, $"{countryCode}.ropf");
                if (File.Exists(ropfFilePath))
                {
                    ParseRopfFile(ropfFilePath, countryCode, parties, polls, groups);
                }
            }
            
            Console.WriteLine("Before ParseChesData");
            
            // Parse CHES data
            var chesFilePath = Path.Combine(assetsFolder, "CHES.txt");
            if (File.Exists(chesFilePath))
            {
                ParseChesData(chesFilePath, parties);
            }
            
            Console.WriteLine("Before Seed Parties");

            // Seed Parties
            modelBuilder.Entity<Party>().HasData(parties.Select(p => new
            {
                p.Id,
                p.StringId,
                p.Acronym,
                p.EnglishName,
                LocalName = p.LocalName ?? new List<string>([""]),
                p.CountryCode,
                CountryCode1 = p.CountryCode,
                p.CHES_EU,
                p.CHES_Economy,
                p.CHES_Progress,
                p.CHES_Liberal,
                p.Mp,
                p.Role
            }).ToArray<object>());
            
            Console.WriteLine("Before Seed Polls");

            // Seed Polls
            modelBuilder.Entity<Poll>().HasData(polls.Select(p => new
            {
                p.Id,
                p.Pollster,
                Media = p.Media ?? new List<string>([""]),
                p.StartDate,
                p.FinishDate,
                p.Type,
                p.Sample,
                p.Others,
                p.Area
            }).ToArray<object>());
            
            Console.WriteLine("Before Seed PollResults");

            // Seed PollResults
            var pollResults = polls
                .SelectMany(p => p.Results, (poll, result) => new
                {
                    PollId = poll.Id,
                    PartyId = result.Party.Id, // Assuming Result.Party is an actual Party object
                    Value = result.Value
                })
                .ToArray();
            modelBuilder.Entity<PollResult>().HasData(pollResults);
            
            Console.WriteLine("Before Seed PartyGroups");

            // Seed PartyGroups
            var partyGroups = parties
                .SelectMany(p => p.Groups, (party, group) => new
                {
                    PartyId = party.Id,
                    GroupId = group.Id
                })
                .ToArray();
            modelBuilder.Entity("PartyGroups").HasData(partyGroups);
            
            Console.WriteLine("Before Seed PartySubParties");

            // Seed PartySubParties
            var partySubParties = parties
                .Where(p => p.SubParties != null) // Ensure SubParties is not null
                .SelectMany(p => p.SubParties, (parentParty, subParty) => new
                {
                    ParentPartyId = parentParty.Id,
                    SubPartyId = subParty.Id
                })
                .ToArray();
            modelBuilder.Entity("PartySubParties").HasData(partySubParties);
            
            Console.WriteLine("FINISHED");
        }
        
        
        
        
        

        private void ParseRopfFile(string ropfFilePath, string countryCode, List<Party> parties, List<Poll> polls, List<Group> groups)
        {
            var lines = File.ReadAllLines(ropfFilePath);

            bool partiesStarted = false;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (partiesStarted)
                    {
                        break; // End extraction after the second empty line
                    }
                    partiesStarted = true; // Start collecting after the first empty line
                }
                else if (partiesStarted)
                {
                    parties.Add(ParsePartyLine(line, countryCode, parties, groups)); // Collect party lines between two empty lines
                }
            }
            
            Poll currentPoll = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("•PF:"))
                {
                    // Create a new poll
                    if (currentPoll != null) polls.Add(currentPoll);
                    currentPoll = CreatePollFromLine(line, parties);
                }
                else if (line.StartsWith("&") && currentPoll != null)
                {
                    // Create a new poll based on the previous one
                    var newPoll = CreatePollFromLine(line, parties, currentPoll);
                    polls.Add(newPoll);
                }
            }

            // Add the last parsed poll
            if (currentPoll != null) polls.Add(currentPoll);
        }
        
        private Party ParsePartyLine(string line, string country, List<Party> existingParties, List<Group> groups)
        {
            var stringId = line.Split(':')[0].Trim();
            var acronym = ExtractField(line, "•R:") ?? ExtractField(line, "•A:") ?? "Error";
            var englishName = ExtractField(line, "•EN:") ?? "Error";

            var localNames = ExtractLocalNames(line); // Updated to handle multiple local names
            var mp = int.TryParse(ExtractField(line, "•MP:"), out var parsedMp) ? (int?)parsedMp : null;
            var subParties = ExtractSubParties(line, existingParties);

            var role = new HashSet<string>();
            if (line.Contains("•GOV")) role.Add("Rząd");
            if (line.Contains("•SUP")) role.Add("Wsparcie");

            if (subParties != null && role.Any())
            {
                ApplyRoleToSubParties(subParties, role);
            }
            else if (subParties != null)
            {
                var uniqueRoles = new HashSet<string>();
                foreach (var subParty in subParties)
                {
                    if (subParty.Role != null)
                    {
                        foreach (var subRole in subParty.Role)
                        {
                            if (!uniqueRoles.Contains(subRole))
                            {
                                uniqueRoles.Add(subRole);
                                role.Add(subRole);
                            }
                        }
                    }
                }
            }

            var groupField = ExtractField(line, "•GROUP:");
            var groupObjects = !string.IsNullOrEmpty(groupField)
                ? groupField.Split('/').Select(g => groups.SingleOrDefault(gr => gr.Acronym == g.Trim())).ToList()
                : null;

            var uniqueGroups = new HashSet<Group>();
            if (groupObjects == null && subParties != null)
            {
                var uniqueGroupIdentifiers = new HashSet<string>();
                foreach (var subParty in subParties)
                {
                    if (subParty.Groups != null)
                    {
                        foreach (var group in subParty.Groups)
                        {
                            if (!uniqueGroupIdentifiers.Contains(group.Acronym))
                            {
                                uniqueGroupIdentifiers.Add(group.Acronym);
                                uniqueGroups.Add(group);
                            }
                        }
                    }
                }
            }

            // Create the Party object
            var party = new Party
            {
                Id = Guid.NewGuid(),
                StringId = stringId,
                Acronym = acronym,
                EnglishName = englishName,
                LocalName = localNames,
                CountryCode = country,
                CHES_EU = null,
                CHES_Economy = null,
                CHES_Progress = null,
                CHES_Liberal = null,
                SubParties = subParties,
                Groups = groupObjects != null ? new HashSet<Group>(groupObjects) : uniqueGroups,
                Mp = mp,
                Role = role
            };
            
            return party;
        }
        
        private string? ExtractField(string line, string marker)
        {
            var regex = new Regex($@"{marker}(.*?)(?=•|$|\s+\S+?:)");
            var match = regex.Match(line);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }
        
        private List<string>? ExtractLocalNames(string line)
        {
            var regex = new Regex(@"•([A-Z]{2,4}):\s*(.*?)(?=•|$)");
            var matches = regex.Matches(line);
            var localNames = new List<string>();

            foreach (Match match in matches)
            {
                var marker = match.Groups[1].Value;
                var value = match.Groups[2].Value.Trim();

                if (!new[] { "EN", "SUB", "GROUP", "MP", "GOV", "SUP" }.Contains(marker))
                {
                    localNames.Add(value);
                }
            }

            return localNames.Count > 0 ? localNames : null;
        }
        
        private List<Party>? ExtractSubParties(string line, List<Party> existingParties)
        {
            var subPartiesString = ExtractField(line, "•SUB:");
            if (subPartiesString == null) return null;

            var subPartyIds = subPartiesString.Split(',').Select(id => id.Trim()).ToList();
            return existingParties.Where(party => subPartyIds.Contains(party.StringId)).ToList();
        }
        
        private void ApplyRoleToSubParties(IEnumerable<Party> subParties, HashSet<string> role)
        {
            foreach (var subParty in subParties)
            {
                subParty.Role = role;

                if (subParty.SubParties != null)
                {
                    ApplyRoleToSubParties(subParty.SubParties, role);
                }
            }
        }
        
        private void ParseChesData(string chesFilePath, List<Party> parties)
        {
            var lines = File.ReadAllLines(chesFilePath);

            foreach (var party in parties)
            {
                var matchingCHESData = lines.FirstOrDefault(data =>
                {
                    if (data.Split(';')[2] != party.CountryCode)
                    {
                        return false;
                    }

                    // Split the acronym field and check against party properties
                    var acronyms = data.Split(';')[3].Split('/').Select(acr => acr.Trim());
                    return acronyms.Any(acronym =>
                        acronym == party.Acronym ||
                        acronym == party.EnglishName ||
                        acronym == party.StringId);
                });

                if (matchingCHESData != null)
                {
                    party.CHES_EU = [double.Parse(matchingCHESData.Split(';')[7])];
                    party.CHES_Economy = [double.Parse(matchingCHESData.Split(';')[11])];
                    party.CHES_Progress = [double.Parse(matchingCHESData.Split(';')[14])];
                    party.CHES_Liberal = [double.Parse(matchingCHESData.Split(';')[17])];
                }
                else if (party.SubParties != null && party.SubParties.Any())
                {
                    // Aggregate CHES data from sub-parties
                    var subPartiesData = GetSubPartiesCHESData(party.SubParties);
                    party.CHES_EU = subPartiesData[0];
                    party.CHES_Economy = subPartiesData[1];
                    party.CHES_Progress = subPartiesData[2];
                    party.CHES_Liberal = subPartiesData[3];
                }
            }
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
        
        private Poll CreatePollFromLine(string line, List<Party> parties, Poll basePoll = null)
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
                Results = ExtractResults(line, parties),
                Others = ExtractDoubleField(line, "•O:") ?? 0,
                Area = ExtractField(line, "•A:") ?? string.Empty
            };

            // Add additional media if found in the current line
            AddMultipleFields(line, "•C:", poll.Media);
            
            return poll;
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

        List<PollResult> ExtractResults(string line, List<Party> parties)
        {
            var results = new List<PollResult>();
            var regex = new Regex(@"\s([\p{L}\w\-+]+):\s(\d+(\.\d+)?)", RegexOptions.Compiled);
            var matches = regex.Matches(line);

            double totalSupport = 0;
            var tempResults = new List<(Party Party, double Value)>();

            foreach (Match match in matches)
            {
                var acronym = match.Groups[1].Value.Trim();
                var value = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                totalSupport += value;

                var party = parties.FirstOrDefault(p => p.StringId == acronym);
                if (party != null)
                {
                    tempResults.Add((party, value));
                }
                else if (acronym.Contains("+"))
                {
                    var subAcronyms = acronym.Split("+").Select(sub => sub.Trim());
                    var allParties = subAcronyms
                        .Select(sub => parties.FirstOrDefault(p => p.StringId == sub))
                        .Where(p => p != null)
                        .ToList();

                    if (allParties.Any())
                    {
                        var newParty = CreateCompositeParty(allParties);
                        tempResults.Add((newParty, value));
                    }
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

        Party CreateCompositeParty(List<Party> subParties)
        {
            var groups = subParties.SelectMany(p => p.Groups).ToHashSet();
            var roles = subParties.SelectMany(p => p.Role).ToHashSet();

            var chesData = GetSubPartiesCHESData(subParties);

            return new Party
            {
                Id = Guid.NewGuid(),
                Acronym = string.Join("/", subParties.Select(p => p.Acronym)),
                StringId = string.Join("/", subParties.Select(p => p.Acronym)),
                EnglishName = string.Join("/", subParties.Select(p => p.Acronym)),
                LocalName = null,
                Groups = groups,
                Role = roles,
                SubParties = subParties,
                CountryCode = subParties.First().CountryCode,
                CHES_EU = chesData[0],
                CHES_Economy = chesData[1],
                CHES_Progress = chesData[2],
                CHES_Liberal = chesData[3]
            };
        }
    }
}