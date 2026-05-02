using UnityEngine;

namespace Portfolio
{
    [CreateAssetMenu(menuName = "Portfolio/Exhibit Data", fileName = "PortfolioExhibit")]
    public class PortfolioExhibitData : ScriptableObject
    {
        public string Id = string.Empty;
        public PortfolioExhibitCategory Category = PortfolioExhibitCategory.Unknown;
        public string Title = string.Empty;
        public string Subtitle = string.Empty;
        [TextArea(3, 8)] public string Body = string.Empty;
        public string[] StackTags = new string[0];
        public PortfolioLink[] Links = new PortfolioLink[0];
        public int DisplayOrder;
    }
}
