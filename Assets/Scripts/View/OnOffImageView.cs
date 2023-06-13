using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class OnOffImageView : MonoBehaviour
    {
        public Image onImage;
        public Image offImage;

        public void ToggleTo(bool isOn)
        {
            if (isOn)
            {
                onImage.gameObject.SetActive(true);
                offImage.gameObject.SetActive(false);
            }
            else
            {
                onImage.gameObject.SetActive(false);
                offImage.gameObject.SetActive(true);
            }
        }

        public void Toggle()
        {
            onImage.gameObject.SetActive(!onImage.gameObject.activeSelf);
            offImage.gameObject.SetActive(!offImage.gameObject.activeSelf);
        }
    }
}