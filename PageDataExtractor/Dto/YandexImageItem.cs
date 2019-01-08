using Newtonsoft.Json;

namespace JavelinView2.Models.AdministrativeModels.OverridableEditorModels.HotelEditor.ImagesSearcher.Dto
{
    public class YandexImageItem
    {
        [JsonProperty(PropertyName = "serp-item")]
        public SerpItem SerpItem { get; set; }
    }
}