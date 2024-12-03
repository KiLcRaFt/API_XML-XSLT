namespace API_XML_XSLT.Models
{
    public class Igapaeva_andmed
    {
        public int Id { get; set; }
        public int TootajaId { get; set; }
        public Tootaja Tootaja { get; set; }
        public DateTime Kuupaev { get; set; }
        public TimeSpan? Too_algus { get; set; }
        public TimeSpan? Too_lypp { get; set; }
    }
}
