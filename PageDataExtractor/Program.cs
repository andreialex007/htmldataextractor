using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            //            var images = YandexParser.GetImages("Железная дорога");
            //
            //            return;

            var sourceFolder = @"C:\Downloaded Web Sites\www.hccomposite.com\press\news\archive";
            var htmlFiles = Directory.EnumerateFiles(sourceFolder, "*.html", SearchOption.AllDirectories).ToList();
            var parsedFiles = new List<XElement>();

            for (var index = 0; index < htmlFiles.Count; index++)
            {
                var htmlFile = htmlFiles[index];
                var htmlDocument = new HtmlDocument();
                var html = File.ReadAllText(htmlFile, Encoding.Default);

                htmlDocument.LoadHtml(html);

                var dataItem = new DataItem
                {
                    Tag = "Page",
                    Selector = ".body_top",
                    Node = htmlDocument.DocumentNode,

                    DataItems = new List<DataItem>
                    {
                        new DataItem
                        {
                            Tag = "Header",
                            Selector = "h1"
                        },
                        new DataItem
                        {
                            Tag = "Description",
                            Selector = ".inner-left p",
                            JoinContent = true,
                            OuterHtml = true
                        }
                    }

                    //                    DataItems = new List<DataItem>
                    //                    {
                    //                        new DataItem
                    //                        {
                    //                            Tag = "Header",
                    //                            Selector = ".TitleH"
                    //                        },
                    //                        new DataItem
                    //                        {
                    //                            Tag = "Description",
                    //                            Selector = ".ItemInfo"
                    //                        },
                    //                        new DataItem
                    //                        {
                    //                            Tag = "Images",
                    //                            Selector = ".carousel-inner",
                    //                            DataItems = new List<DataItem>
                    //                            {
                    //                                new DataItem
                    //                                {
                    //                                    Selector = ".img-responsive",
                    //                                    Attribute = "src",
                    //                                    Tag = "Image"
                    //                                }
                    //                            }
                    //                        }
                    //
                    //                    }
                };
                var process = dataItem.Process();
                var totalItems = process.Count;
                for (var i = 0; i < process.Count; i++)
                {
                    var element = process[i];
                    var originalImage = YandexParser.GetImages(element.Element("Header").Value).First().original;
                    element.Add(new XElement("Image", originalImage));
                }

                if (process.Any())
                    parsedFiles.AddRange(process);

                Debug.WriteLine("Item #" + index + " Of " + htmlFiles.Count);
            }


            var document =
                new XDocument(
                    new XElement("root",
                        parsedFiles
                    )
                );

            document.Save(@"C:\output\_Result.xml");
        }

    }



    public class DataItem
    {
        public string Tag { get; set; }
        public string Selector { get; set; }
        public List<DataItem> DataItems { get; set; } = new List<DataItem>();
        public HtmlNode Node { get; set; }
        public string Attribute { get; set; }
        public bool JoinContent { get; set; } = false;
        public bool OuterHtml { get; set; }
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

                var contentItems = htmlNodes
                    .Select(x => new XElement(dataItem.Tag,
                        x.ExtractContent(dataItem.Attribute)))
                    .ToList();
                if (!contentItems.Any())
                    return null;

                if (dataItem.JoinContent)
                {
                    var singleContent = contentItems.Aggregate("", (item, el) => item + el.Value);
                    elements.Add(new XElement(dataItem.Tag, singleContent.CleanUp()));
                }
                else
                {
                    elements.AddRange(contentItems);
                }
            }


            return elements;
        }

        static string RemoveInvalidXmlChars(this string text)
        {
            var validXmlChars = text.Where(XmlConvert.IsXmlChar).ToArray();
            return new string(validXmlChars);
        }
        static object ExtractContent(this HtmlNode node, string attribute, bool outerHtml = false)
        {
            if (attribute != null)
                return node.Attributes.First(x => x.Name == attribute).Value;

            var html = outerHtml ? node.OuterHtml : node.InnerHtml;
            html = html.StripHtml();
            return CleanUp(html);
        }

        private static object CleanUp(this string html)
        {
            var escaped = System.Security.SecurityElement.Escape(html);
            if (escaped != html)
                return new XCData(html.RemoveInvalidXmlChars().Trim());
            return html.Trim();
        }

        public static string RemoveNode(this string content, string node) => Regex.Replace(content, $"<{node}(.*)</{node}>", "");
        public static string StripHtml(this string input) => Regex.Replace(input, "<.*?>", String.Empty);
    }
}
