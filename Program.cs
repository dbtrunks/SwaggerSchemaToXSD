using System.Web.Helpers;

namespace SwaggerSchemaToXSD
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("#######################");
            Console.WriteLine("#Swagger schema to XSD#");
            Console.WriteLine("#######################");

            var sagggerToXsd = new SwaggerSchemaToXSD();

            var json =   sagggerToXsd.LoadJson();
            var schemas = json["components"]["schemas"];

           var resultPath =   sagggerToXsd.ConvertSchemaTosxd(schemas);

            Console.WriteLine("");
            Console.WriteLine($"Wynik zapisano do pliku {resultPath}");
        }
    }
}