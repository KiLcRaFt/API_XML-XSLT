namespace API_XML_XSLT.Models
{
    public class Tootaja
    {
        public int Id { get; set; }

        public string Nimi { get; set; }

        public string Perenimi { get; set; }

        public string? Email { get; set; }
        public string? Telefoni_number { get; set; }
        public string Salasyna { get; set; }

        public bool Is_admin { get; set; }

        public string Amet { get; set; }

        public ICollection<Igapaeva_andmed> IgapaevaAndmed { get; set; }
    }
}
