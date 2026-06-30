using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackgroundScaler : MonoBehaviour
{
    private void Start()
    {
        Image image = GetComponent<Image>();
        if (image == null)
        {
            return;
        }
        float ws = Screen.width;
        float hs = Screen.height;

        if (ws >hs)//宽屏
        {

        }
        else if (ws < hs)//长屏
        {

        }
        else if (ws == hs)//正
        {

        }

        RectTransform rectTransform = image.rectTransform;
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (Screen.width >= Screen.height)
        {
            // Landscape mode (or square)
            float scale = Screen.width / (float)image.sprite.texture.width;

            rectTransform.sizeDelta = new Vector2(0, (image.sprite.texture.height * scale - Screen.height));
        }
        else
        {
            // Portrait mode
            float scale = Screen.height / (float)image.sprite.texture.height;

            rectTransform.sizeDelta = new Vector2((image.sprite.texture.width * scale - Screen.width) /2, 0);
        }

        //rectTransform.offsetMin = new Vector2(0, 0);
        //rectTransform.offsetMax = new Vector2(0, 0);
    }

}