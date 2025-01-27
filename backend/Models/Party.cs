namespace backend.Models
{
    public class Party
    {
        public Guid Id { get; set; }
        public string StringId { get; set; }
        public string Acronym { get; set; }
        public string EnglishName { get; set; }
        public string CountryCode { get; set; }
        public List<string> LocalName { get; set; }
        public List<double> CHES_EU { get; set; }
        public List<double> CHES_Economy { get; set; }
        public List<double> CHES_Progress { get; set; }
        public List<double> CHES_Liberal { get; set; }
        public List<Party> SubParties { get; set; }
        public ICollection<Group> Groups { get; set; }
        public int? Mp { get; set; }
        public ICollection<string> Role { get; set; }
    }
}