namespace API_XML_XSLT.Models
{
    public class Igapaeva_andmed
    {
        public int Id { get; set; }
        public int TootajaId { get; set; }
        public Tootaja? Tootaja { get; set; }
        public DateTime Kuupaev { get; set; }
        public TimeSpan? Too_algus { get; set; }
        public TimeSpan? Too_lypp { get; set; }
    }

    public class TooAeg_andmed
    {
        public DateTime Kuupaev { get; set; }
        public string TooAlgus { get; set; }
        public string TooLypp { get; set; }
    }

    public class TooAeg_andmed_id
    {
        public int TootajaId { get; set; }
        public DateTime Kuupaev { get; set; }
        public string TooAlgus { get; set; }
        public string TooLypp { get; set; }
    }

    public class WorkHourUpdateModel
    {
        public int TooAegaId { get; set; }
        public int TootajaId { get; set; }
        public DateTime Kuupaev { get; set; }
        public TimeSpan TooAlgus { get; set; }
        public TimeSpan TooLypp { get; set; }
    }
}
