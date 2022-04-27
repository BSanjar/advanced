using System.Collections.Generic;

namespace advanced.models
{
    public class ResponseApi
    {
        public string code { get; set; }
        public string comment { get; set; }
        public List<files> files { get; set; }
    }
    public class files
    {
        public string fileName { get; set; }
        public string resultParse { get; set; }
        public string resultTranslate { get; set; }
        public string translatedText { get; set; }
    }
}
