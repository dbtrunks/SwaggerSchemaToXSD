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

                    if (prop.First.Value<string>("type") == "array")
                    {
                        string refTypeArr = prop.Children()["items"].First().Value<string>("$ref");
                        if (!string.IsNullOrEmpty(refTypeArr))
                            refType = refTypeArr.Replace("#/components/schemas/", "").ToString();
                    }


                    List<string> enmList = null;
                    var enm = prop.First.Value<JArray>("enum");
                    if (enm?.Count > 0)
                    {
                        enmList = enm.Values<string>().ToList();
                    }

                    Propertie propertie = new Propertie()
                    {
                        Name = ((JProperty)prop).Name,
                        Type = prop.First.Value<string>("type"),
                        Format = prop.First.Value<string>("format"),
                        Ref = refType,
                        Enum = enmList
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

            var enumeration = new Dictionary<string, List<string>>();

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
                    {
                        string type = pro.Type;
                        switch (pro.Type)
                        {
                            case "number":
                                type = "decimal";
                                break;
                            case "string":
                                if (pro.Format == "date-time")
                                    type = "date";
                                break;
                            default:
                                break;
                        }

                        if (pro.Enum == null)
                            fileXsd.AppendLine($"<xs:element name=\"{pro.Name}\" type=\"xs:{type}\"/>");
                        else
                        {
                            fileXsd.AppendLine($"<xs:element name=\"{pro.Name}\" type=\"{pro.Name.ToLower()}\"/>");
                            if (!enumeration.ContainsKey(pro.Name.ToLower()))
                                enumeration.Add(pro.Name.ToLower(), pro.Enum);

                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(pro.Ref) && pro.Type == "array")
                            fileXsd.AppendLine($"<xs:element name=\"{pro.Name}\" type=\"{pro.Ref}\" maxOccurs=\"unbounded\" minOccurs=\"0\" />");
                        else
                            fileXsd.AppendLine($"<xs:element name=\"{pro.Name}\" type=\"{pro.Ref}\"/>");
                    }


                }
                fileXsd.AppendLine($"</xs:sequence>");
                fileXsd.AppendLine($"</xs:complexType>");
                // fileXsd.AppendLine($"</xs:element>");

            }

            foreach (var enu in enumeration.Distinct())
            {
                fileXsd.AppendLine($"<xs:simpleType name=\"{enu.Key}\" final=\"restriction\" >");
                fileXsd.AppendLine($"<xs:restriction base=\"xs:string\">");
                foreach (var item in enu.Value)
                {
                    fileXsd.AppendLine($"<xs:enumeration value=\"{item}\" />");
                }

                fileXsd.AppendLine($"</xs:restriction>");
                fileXsd.AppendLine($"</xs:simpleType>");

            }




            fileXsd.AppendLine($"</xs:schema>");

            System.IO.File.WriteAllText(@"./result.xsd", fileXsd.ToString());
        }
    }
}
