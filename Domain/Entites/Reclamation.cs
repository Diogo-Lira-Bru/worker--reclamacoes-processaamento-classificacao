using System.Text.Json.Serialization;

namespace ReclamacoesBank.Domain.Entites
{
    public class Reclamation
    {
        public string Id { get; set; }
        public string ReclamationUserText { get; set; }
        public string CustomerIdentifier { get; set; }
        public string AttachmentUrl { get; set; }
        public List<string> ClassifiedCategories { get; set; } = new List<string>();
        public DateTime ReceivedDate { get; set; }
        public string SourceChannel { get; set; }
        public string CustomerHistory { get; set; }
    }
}
