namespace SwaggerSchemaToXSD
{
    public class Element
    {
        public string Name { get; set; }
        public List<Propertie> Properties { get; set; }
    }

    public class Propertie
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }

        public string Ref { get; set; }

        public List<string>? Enum { get; set; }
    }
}
