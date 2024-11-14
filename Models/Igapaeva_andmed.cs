namespace API_XML_XSLT.Models
{
    public class Igapaeva_andmed
    {
        public int Id { get; set; }
        public Tootaja Tootaja_Id { get; set; }
        public DateOnly Kuupaev { get; set; }
        public TimeOnly? Too_algus { get; set; }
        public TimeOnly? Too_lypp { get; set; }
    }
}
