using UnityEngine;

namespace Portfolio
{
    public class InteractableExhibit : MonoBehaviour
    {
        [SerializeField] private PortfolioExhibitData data;
        [SerializeField] private GameObject highlight;

        public PortfolioExhibitData Data => data;

        public void SetData(PortfolioExhibitData exhibitData)
        {
            data = exhibitData;
        }

        public void SetHighlight(GameObject highlightObject)
        {
            highlight = highlightObject;
            SetHighlighted(false);
        }

        public void SetHighlighted(bool highlighted)
        {
            if (highlight != null)
            {
                highlight.SetActive(highlighted);
            }
        }
    }
}
