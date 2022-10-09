using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace SwaggerSchemaToXSD
{
    public class SwaggerSchemaToXSD
    {




        public JObject? LoadJson()
        {
            using (StreamReader r = new StreamReader("swagger.json"))
            {
                var json = r.ReadToEnd();
                var jsonData = (JObject)JsonConvert.DeserializeObject(json);
                return jsonData;
            }
        }


        public string ConvertSchemaTosxd(JToken schema)
        {
            var elementList = new List<Element>();

            foreach (var token in schema)
            {
                Element element = new Element() { Name = ((JProperty)token).Name, Properties = new List<Propertie>() };
                foreach (var prop in token.Children()["properties"].Children())
                {

                    string refType = prop.First.Value<string>("$ref");
                    if (!string.IsNullOrEmpty(refType))
                        refType = refType.Replace("#/components/schemas/", "").ToString();

                    Propertie propertie = new Propertie()
                    {
                        Name = ((JProperty)prop).Name,
                        Type = prop.First.Value<string>("type"),
                        Format = prop.First.Value<string>("format"),
                        Ref = refType
                    };

                    element.Properties.Add(propertie);
                }
                elementList.Add(element);
            }
            GenerateXSD(elementList);
            return "result.xsd";
        }


        private void GenerateXSD(List<Element> elementList)
        {
            StringBuilder fileXsd = new StringBuilder("<?xml version=\"1.0\"?>");
            fileXsd.AppendLine($"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">");
            foreach (var element in elementList)
            {
                //  fileXsd.AppendLine($"<xs:element name=\"{element.Name}\">");
                fileXsd.AppendLine($"<xs:complexType name=\"{element.Name}\">");
                fileXsd.AppendLine($"<xs:sequence>");

                foreach (var pro in element.Properties)
                {
                    if (string.IsNullOrEmpty(pro.Ref))
                        if (pro.Type == "array")
                            fileXsd.AppendLine($"<xs:element name=\"{pro.Name}\" type=\"Array\"/>");
                        else
                            fileXsd.AppendLine($"<xs:element name=\"{pro.Name}\" type=\"xs:{pro.Type}\"/>");
                    else
                        fileXsd.AppendLine($"<xs:element name=\"{pro.Name}\" type=\"{pro.Ref}\"/>");

                }
                fileXsd.AppendLine($"</xs:sequence>");
                fileXsd.AppendLine($"</xs:complexType>");
                // fileXsd.AppendLine($"</xs:element>");

            }


            fileXsd.AppendLine($"<xs:complexType name=\"Array\">");
            fileXsd.AppendLine($"<xs:sequence>");
            fileXsd.AppendLine($"<xs:element maxOccurs=\"unbounded\" name=\"date\" type=\"xs:string\"/>");
            fileXsd.AppendLine($"</xs:sequence>");
            fileXsd.AppendLine($"</xs:complexType>");



            fileXsd.AppendLine($"</xs:schema>");

            System.IO.File.WriteAllText(@"./result.xsd", fileXsd.ToString());
        }
    }
}
