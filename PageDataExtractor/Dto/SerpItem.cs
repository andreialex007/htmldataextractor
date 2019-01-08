using System.Collections.Generic;

namespace JavelinView2.Models.AdministrativeModels.OverridableEditorModels.HotelEditor.ImagesSearcher.Dto
{
    public class SerpItem
    {
        public string reqid { get; set; }
        public string freshness { get; set; }
        public List<Preview> preview { get; set; }
        public List<Dup> dups { get; set; }
        public Thumb thumb { get; set; }
        public Snippet snippet { get; set; }
        public string detail_url { get; set; }
        public string img_href { get; set; }
        public bool useProxy { get; set; }
        public int pos { get; set; }
        public string id { get; set; }
        public string timeQuery { get; set; }
        public string counterPath { get; set; }
    }
}