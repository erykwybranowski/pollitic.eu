using System;
using System.Linq;
using backend;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests
{
    // TStartup is your Program class. Make sure Program is declared as partial.
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Force the environment to "IntegrationTest" so that Program.cs registers InMemory.
            builder.UseEnvironment("IntegrationTest");

            builder.ConfigureServices(services =>
            {
                // Remove any existing registrations for AppDbContext options.
                var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)).ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Now register AppDbContext using the InMemory provider.
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDb");
                });

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope and seed the database.
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();

                    // Ensure the database is created.
                    db.Database.EnsureCreated();

                    // Seed the database, but only if not already seeded.
                    SeedData(db);
                }
            });
        }

        private void SeedData(AppDbContext context)
        {
            // Check if data is already seeded.
            if (context.Countries.Any(c => c.CountryCode == "pl"))
            {
                return; // Data already exists, so skip seeding.
            }

            // Create a sample country.
            var poland = new Country
            {
                CountryCode = "pl",
                Name = "Polska",
                Parties = new System.Collections.Generic.List<Party>()
            };
            context.Countries.Add(poland);

            // Create a sample group.
            var groupECR = new Group
            {
                Id = -3,
                Acronym = "ECR",
                Name = "European Conservatives and Reformists",
                R = 39,
                G = 44,
                B = 186,
                Parties = new System.Collections.Generic.List<Party>()
            };
            context.Set<Group>().Add(groupECR);

            // Create sample parties.
            var party1 = new Party
            {
                Id = Guid.NewGuid(),
                StringId = "ZP",
                Acronym = "ZP",
                EnglishName = "United Right",
                CountryCode = "pl",
                Country = poland,
                LocalName = new System.Collections.Generic.List<string> { "Zjednoczona Prawica" },
                CHES_EU = new System.Collections.Generic.List<double> { 1 },
                CHES_Economy = new System.Collections.Generic.List<double> { -1 },
                CHES_Progress = new System.Collections.Generic.List<double> { 3 },
                CHES_Liberal = new System.Collections.Generic.List<double> { 3 },
                Mp = 0,
                Role = new System.Collections.Generic.List<string>(),
                SubParties = new System.Collections.Generic.List<Party>(),
                Groups = new System.Collections.Generic.List<Group> { groupECR },
                Results = new System.Collections.Generic.List<PollResult>()
            };

            var party2 = new Party
            {
                Id = Guid.NewGuid(),
                StringId = "PIS",
                Acronym = "PIS",
                EnglishName = "Law and Justice",
                CountryCode = "pl",
                Country = poland,
                LocalName = new System.Collections.Generic.List<string> { "Prawo i Sprawiedliwość" },
                CHES_EU = new System.Collections.Generic.List<double> { 0 },
                CHES_Economy = new System.Collections.Generic.List<double> { 0 },
                CHES_Progress = new System.Collections.Generic.List<double> { 0 },
                CHES_Liberal = new System.Collections.Generic.List<double> { 0 },
                Mp = 0,
                Role = new System.Collections.Generic.List<string>(),
                SubParties = new System.Collections.Generic.List<Party>(),
                Groups = new System.Collections.Generic.List<Group>(),
                Results = new System.Collections.Generic.List<PollResult>()
            };

            // Add parties to the country.
            poland.Parties.Add(party1);
            poland.Parties.Add(party2);
            context.Set<Party>().AddRange(party1, party2);

            // Create a sample poll.
            var poll = new Poll
            {
                Id = Guid.NewGuid(),
                Pollster = "Ipsos",
                Media = new System.Collections.Generic.List<string> { "TVP" },
                StartDate = new DateTime(2025, 1, 14),
                FinishDate = new DateTime(2025, 1, 16),
                Type = "N",
                Sample = 1000,
                Others = 2,
                Area = "",
                Results = new System.Collections.Generic.List<PollResult>()
            };

            // Create poll results.
            var result1 = new PollResult
            {
                PollId = poll.Id,
                Poll = poll,
                PartyId = party1.Id,
                Party = party1,
                Value = 32.2
            };

            var result2 = new PollResult
            {
                PollId = poll.Id,
                Poll = poll,
                PartyId = party2.Id,
                Party = party2,
                Value = 11.1
            };

            poll.Results.Add(result1);
            poll.Results.Add(result2);
            context.Polls.Add(poll);

            context.SaveChanges();
        }
    }
}
