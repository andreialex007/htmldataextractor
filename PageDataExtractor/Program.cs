using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace PageDataExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var sourceFolder = @"C:\Downloaded Web Sites\www.interlab.ru\katalog-oborudovaniya\";
            var htmlFiles = Directory.EnumerateFiles(sourceFolder, "*.html", SearchOption.AllDirectories).ToList();
            var parsedFiles = new List<XElement>();

            foreach (var htmlFile in htmlFiles)
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(File.ReadAllText(htmlFile, Encoding.UTF8));
                var dataItem = new DataItem
                {
                    Tag = "Page",
                    Selector = ".ContentAreaPage",
                    Node = htmlDocument.DocumentNode,
                    DataItems = new List<DataItem>
                    {
                        new DataItem
                        {
                            Tag = "Header",
                            Selector = ".TitleH"
                        },
                        new DataItem
                        {
                            Tag = "Description",
                            Selector = ".ItemInfo"
                        },
                        new DataItem
                        {
                            Tag = "Images",
                            Selector = ".carousel-inner",
                            DataItems = new List<DataItem>
                            {
                                new DataItem
                                {
                                    Selector = ".img-responsive",
                                    Attribute = "src",
                                    Tag = "Image"
                                }
                            }
                        }

                    }
                };
                var process = dataItem.Process();
                if (process.Any())
                {
                    parsedFiles.AddRange(process);
                }

            }


            var document =
                new XDocument(
                    new XElement("root",
                        parsedFiles
                    )
                );

            document.Save(@"C:\output\Sample.xml");
        }
    }



    public class DataItem
    {
        public string Tag { get; set; }
        public string Selector { get; set; }
        public List<DataItem> DataItems { get; set; } = new List<DataItem>();
        public HtmlNode Node { get; set; }
        public string Attribute { get; set; }
    }

    public static class ProcessExtensions
    {
        public static List<XElement> Process(this DataItem dataItem)
        {
            var elements = new List<XElement>();
            var htmlNodes = dataItem.Node.QuerySelectorAll(dataItem.Selector);
            if (dataItem.DataItems.Any())
            {
                var parentElement = new XElement(dataItem.Tag);



                foreach (var htmlNode in htmlNodes)
                {
                    dataItem.DataItems.ForEach(x => x.Node = htmlNode);
                    var childProperties = dataItem.DataItems.Select(x => x.Process()).ToList();
                    if (childProperties.All(x => x != null))
                    {
                        parentElement.Add(childProperties);
                    }
                }

                if (parentElement.Elements().Any())
                    elements.Add(parentElement);
            }
            else
            {
                var content = htmlNodes
                    .Select(x => new XElement(dataItem.Tag, x.ExtractContent(dataItem.Attribute)))
                    .ToList();
                if (!content.Any())
                    return null;
                elements.AddRange(content);
            }


            return elements;
        }

        static string RemoveInvalidXmlChars(this string text)
        {
            var validXmlChars = text.Where(XmlConvert.IsXmlChar).ToArray();
            return new string(validXmlChars);
        }
        static object ExtractContent(this HtmlNode node, string attribute)
        {
            if (attribute != null)
                return node.Attributes.First(x => x.Name == attribute).Value;
            var escaped = System.Security.SecurityElement.Escape(node.InnerHtml);
            if (escaped != node.InnerHtml)
                return new XCData(node.InnerHtml.RemoveInvalidXmlChars().Trim());
            return node.InnerHtml.Trim();
        }
    }
}
