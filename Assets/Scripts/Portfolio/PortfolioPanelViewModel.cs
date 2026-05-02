using System;
using System.Linq;

namespace Portfolio
{
    public readonly struct PortfolioPanelViewModel
    {
        public readonly string Id;
        public readonly string CategoryLabel;
        public readonly string Title;
        public readonly string Subtitle;
        public readonly string Body;
        public readonly string[] StackTags;
        public readonly PortfolioLink[] Links;
        public string StackSummary => StackTags.Length == 0 ? string.Empty : string.Join(" / ", StackTags);

        private PortfolioPanelViewModel(
            string id,
            string categoryLabel,
            string title,
            string subtitle,
            string body,
            string[] stackTags,
            PortfolioLink[] links)
        {
            Id = id;
            CategoryLabel = categoryLabel;
            Title = title;
            Subtitle = subtitle;
            Body = body;
            StackTags = stackTags;
            Links = links;
        }

        public static PortfolioPanelViewModel FromData(PortfolioExhibitData data)
        {
            if (data == null)
            {
                return new PortfolioPanelViewModel(
                    string.Empty,
                    "Unknown",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    Array.Empty<string>(),
                    Array.Empty<PortfolioLink>());
            }

            var stackTags = data.StackTags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).ToArray()
                ?? Array.Empty<string>();
            var links = data.Links?.Where(link => link != null && !string.IsNullOrWhiteSpace(link.Url)).ToArray()
                ?? Array.Empty<PortfolioLink>();

            return new PortfolioPanelViewModel(
                data.Id ?? string.Empty,
                data.Category.ToString(),
                data.Title ?? string.Empty,
                data.Subtitle ?? string.Empty,
                data.Body ?? string.Empty,
                stackTags,
                links);
        }
    }
}
