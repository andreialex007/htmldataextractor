using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using JavelinView2.Models.AdministrativeModels.OverridableEditorModels.HotelEditor.ImagesSearcher.Dto;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;

namespace PageDataExtractor
{
    public static class YandexParser
    {

        private static ChromeDriver driver = new ChromeDriver();

        public class SearchImageItem
        {
            public string original { get; set; }
            public string preview { get; set; }
        }


        public static List<SearchImageItem> GetImages(string text)
        {
            var url = "https://yandex.ru/images/search?text=";
            var pageSource = string.Empty;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            var s = url + HttpUtility.UrlEncode(text);
            driver.Navigate().GoToUrl(s);
            pageSource = driver.PageSource;

            var docuemnt = new HtmlDocument();
            docuemnt.LoadHtml(pageSource);

            var nodeCollection = docuemnt.DocumentNode.SelectNodes("//*[contains(@class,'serp-item serp-item_type_search serp-item_group_search')]");

            var searchImageItems = nodeCollection
                .Select(x => x.Attributes["data-bem"].Value)
                .Select(TryDeserializeObject)
                .Where(x => x.SerpItem != null)
                .Select(x => new SearchImageItem { original = x.SerpItem.img_href, preview = x.SerpItem.thumb.url })
                .ToList();


            return searchImageItems;
        }

        private static YandexImageItem TryDeserializeObject(string input)
        {
            try
            {
                string decode = HttpUtility.HtmlDecode(input);
                return JsonConvert.DeserializeObject<YandexImageItem>(decode);
            }
            catch (Exception)
            {
                return new YandexImageItem();
            }
        }
    }
}
