namespace RestfulTestful.SQLiteModels
{
    public class HATEOASLinkResponseModel
    {
        public string Method { get; private set; }
        public string Href { get; private set; }
        public string Rel { get; private set; }
        public HATEOASLinkResponseModel(string href, string rel, string method)
        {
            this.Method = method;
            this.Href = href;
            this.Rel = rel;
        }
    }
}