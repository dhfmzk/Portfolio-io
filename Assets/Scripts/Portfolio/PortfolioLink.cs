using System;

namespace Portfolio
{
    [Serializable]
    public class PortfolioLink
    {
        public string Label;
        public string Url;

        public PortfolioLink()
        {
            Label = string.Empty;
            Url = string.Empty;
        }

        public PortfolioLink(string label, string url)
        {
            Label = label ?? string.Empty;
            Url = url ?? string.Empty;
        }
    }
}
