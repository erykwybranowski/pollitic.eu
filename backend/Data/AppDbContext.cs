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
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd()
                      .HasColumnType("char(36)");

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
                      .HasColumnType("varchar(10)");

                entity.Property(e => e.Mp)
                      .HasColumnType("int");

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
                      .HasColumnType("int");

                entity.Property(e => e.Area)
                      .HasColumnType("varchar(255)");

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
                .WithMany()
                .UsingEntity(j => j.ToTable("PartyGroups"));

            modelBuilder.Entity<Party>()
                .HasMany(p => p.SubParties)
                .WithOne()
                .HasForeignKey("ParentPartyId");

            modelBuilder.Entity<Poll>()
                .HasMany(p => p.Results)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            // Initialize Countries
            modelBuilder.Entity<Country>().HasData(new[]
            {
                new Country {Id = Guid.NewGuid(), CountryCode = "at", Name = "Austria" },
                new Country {Id = Guid.NewGuid(), CountryCode = "be", Name = "Belgia" },
                new Country {Id = Guid.NewGuid(), CountryCode = "bg", Name = "Bułgaria"},
                new Country {Id = Guid.NewGuid(), CountryCode = "hr", Name = "Chorwacja"},
                new Country {Id = Guid.NewGuid(), CountryCode = "cy", Name = "Cypr"},
                new Country {Id = Guid.NewGuid(), CountryCode = "cz", Name = "Czechy"},
                new Country {Id = Guid.NewGuid(), CountryCode = "dk", Name = "Dania"},
                new Country {Id = Guid.NewGuid(), CountryCode = "ee", Name = "Estonia"},
                new Country {Id = Guid.NewGuid(), CountryCode = "fi", Name = "Finlandia"},
                new Country {Id = Guid.NewGuid(), CountryCode = "fr", Name = "Francja"},
                new Country {Id = Guid.NewGuid(), CountryCode = "gr", Name = "Grecja"},
                new Country {Id = Guid.NewGuid(), CountryCode = "es", Name = "Hiszpania"},
                new Country {Id = Guid.NewGuid(), CountryCode = "nl", Name = "Holandia"},
                new Country {Id = Guid.NewGuid(), CountryCode = "ie", Name = "Irlandia"},
                new Country {Id = Guid.NewGuid(), CountryCode = "lt", Name = "Litwa"},
                new Country {Id = Guid.NewGuid(), CountryCode = "lu", Name = "Luksemburg"},
                new Country {Id = Guid.NewGuid(), CountryCode = "lv", Name = "Łotwa"},
                new Country {Id = Guid.NewGuid(), CountryCode = "mt", Name = "Malta"},
                new Country {Id = Guid.NewGuid(), CountryCode = "de", Name = "Niemcy"},
                new Country {Id = Guid.NewGuid(), CountryCode = "pl", Name = "Polska"},
                new Country {Id = Guid.NewGuid(), CountryCode = "pt", Name = "Portugalia"},
                new Country {Id = Guid.NewGuid(), CountryCode = "ro", Name = "Rumunia"},
                new Country {Id = Guid.NewGuid(), CountryCode = "sk", Name = "Słowacja"},
                new Country {Id = Guid.NewGuid(), CountryCode = "si", Name = "Słowenia"},
                new Country {Id = Guid.NewGuid(), CountryCode = "se", Name = "Szwecja"},
                new Country {Id = Guid.NewGuid(), CountryCode = "hu", Name = "Węgry"},
                new Country {Id = Guid.NewGuid(), CountryCode = "it", Name = "Włochy"}
            });

            // Initialize Groups
            modelBuilder.Entity<Group>().HasData(new[]
            {
                new Group { Id = -8, Acronym = "LEFT", Name = "Progressive Alliance of Socialists and Democrats", R = 138, G = 21, B = 28 },
                new Group { Id = -7, Acronym = "S&D", Name = "Progressive Alliance of Socialists and Democrats", R = 219, G = 58, B = 46 },
                new Group { Id = -6, Acronym = "GREENS", Name = "Greens/European Free Alliance", R = 27, G = 209, B = 36 },
                new Group { Id = -5, Acronym = "RE", Name = "Renew Europe", R = 238, G = 230, B = 1 },
                new Group { Id = -4, Acronym = "EPP", Name = "European People's Party", R = 52, G = 143, B = 235 },
                new Group { Id = -3, Acronym = "ECR", Name = "European Conservatives and Reformists", R = 39, G = 44, B = 186 },
                new Group { Id = -2, Acronym = "PfE", Name = "Patriots for Europe", R = 76, G = 48, B = 122 },
                new Group { Id = -1, Acronym = "ESN", Name = "Europe of Sovereign Nations", R = 9, G = 52, B = 92 }
            });

            // Parse .ropf files for Parties and Polls
            var parties = new List<Party>();
            var polls = new List<Poll>();

            var assetsFolder = Path.Combine(AppContext.BaseDirectory, "..", "frontend", "public", "assets");
            foreach (var country in modelBuilder.Entity<Country>().Metadata.GetSeedData())
            {
                var countryCode = country["CountryCode"]?.ToString();
                if (string.IsNullOrEmpty(countryCode)) continue;

                var ropfFilePath = Path.Combine(assetsFolder, $"{countryCode}.ropf");
                if (File.Exists(ropfFilePath))
                {
                    ParseRopfFile(ropfFilePath, countryCode, parties, polls);
                }
            }
            
            // Parse CHES data
            var chesFilePath = Path.Combine(assetsFolder, "CHES.txt");
            if (File.Exists(chesFilePath))
            {
                ParseChesData(chesFilePath, parties);
            }

            // Seed Parties
            modelBuilder.Entity<Party>().HasData(parties);

            // Seed Polls
            modelBuilder.Entity<Poll>().HasData(polls);
        }

        private void ParseRopfFile(string ropfFilePath, string countryCode, List<Party> parties, List<Poll> polls)
        {
            var lines = File.ReadAllLines(ropfFilePath);
            var partyDictionary = new Dictionary<string, Party>();

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
                    parties.Add(ParsePartyLine(line, countryCode, parties)); // Collect party lines between two empty lines
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
        
        private Party ParsePartyLine(string line, string country, List<Party> existingParties)
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
                ? groupField.Split('/').Select(g => GetGroup(g.Trim())).ToList()
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
        
        public Group GetGroup(string acronym)
        {
            return Groups.SingleOrDefault(g => g.Acronym == acronym);
        }
        
        private string? ExtractField(string line, string marker)
        {
            var regex = new Regex($@"{marker}(.*?)(?=•|$)");
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

        private List<List<double>> GetSubPartiesCHESData(List<Party> subParties)
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
                StartDate = basePoll?.StartDate ?? DateTime.Parse(ExtractField(line, "•FS:")!),
                FinishDate = basePoll?.FinishDate ?? DateTime.Parse(ExtractField(line, "•FE:")!),
                Type = basePoll?.Type ?? ExtractField(line, "•SC:") ?? "N",
                Sample = basePoll?.Sample ?? ExtractNumberField(line, "•SS:") ?? null,
                Results = ExtractResults(line, parties),
                Others = ExtractNumberField(line, "•O") ?? 0,
                Area = ExtractField(line, "•A:") ?? string.Empty
            };

            // Add additional media if found in the current line
            AddMultipleFields(line, "•C:", poll.Media);

            return poll;
        }

        private int? ExtractNumberField(string line, string marker)
        {
            var valueStr = ExtractField(line, marker);
            return valueStr != null ? int.Parse(valueStr) : null;
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
                var value = double.Parse(match.Groups[2].Value);
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