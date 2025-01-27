using System.Collections.Generic;

namespace backend.Data
{
    public class PartyDTO
    {
        public Guid Id { get; set; }
        public string StringId { get; set; }
        public string Acronym { get; set; }
        public string EnglishName { get; set; }
        public string CountryCode { get; set; }
        public List<string> LocalName { get; set; }
        public List<double?> CHES_EU { get; set; }
        public List<double?> CHES_Economy { get; set; }
        public List<double?> CHES_Progress { get; set; }
        public List<double?> CHES_Liberal { get; set; }
        public List<int> SubPartyIds { get; set; }
        public List<int> GroupIds { get; set; }
        public int? Mp { get; set; }
        public List<string> Role { get; set; }
    }

    public class GroupDTO
    {
        public Guid Id { get; set; }
        public string Acronym { get; set; }
        public string Name { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }

    public class PollDTO
    {
        public Guid Id { get; set; }
        public string Pollster { get; set; }
        public List<string> Media { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public string Type { get; set; }
        public int? Sample { get; set; }
        public List<PollResultDTO> Results { get; set; }
        public double Others { get; set; }
        public string Area { get; set; }
    }

    public class PollResultDTO
    {
        public Guid PartyId { get; set; }
        public double Value { get; set; }
    }
}