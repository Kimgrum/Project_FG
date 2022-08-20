using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomPanel : UIElement
{
    [SerializeField] Slider sizeSlider;
    [SerializeField] Slider opacitySlider;
    [SerializeField] KeyPanelArea keyPanelArea;

    private GameObject setBtn;
    private GameObject parent;
    private RectTransform setBtnRect;
    private RectTransform parentRect;
    private Image setBtnImage;

    private float halfWidth;
    private float halfHeight;
    private float pHalfWidth;
    private float pHalfHeight;

    private float x;
    private float y;
    private Vector2 TempRect;
    private Vector2 beforeSize;
    private float size;
    private Color color;

    public override void Open(UIParam param = null)
    {
        base.Open(param);
    }

    public override void Close()
    {
        base.Close();
    }

    // Call ClickedButton Slider Setting Value
    public void setSlider(GameObject go)
    {
        setBtn = go;
        parent = setBtn.transform.parent.gameObject;
        setBtnImage = setBtn.GetComponent<Image>();
        setBtnRect = setBtn.GetComponent<RectTransform>();
        parentRect = parent.GetComponent<RectTransform>();
        beforeSize = setBtnRect.sizeDelta;

        halfWidth = setBtnRect.sizeDelta.x / 2;
        halfHeight = setBtnRect.sizeDelta.y / 2;
        pHalfWidth = parentRect.sizeDelta.x / 2;
        pHalfHeight = parentRect.sizeDelta.y / 2;

        sizeSlider.value = PlayerPrefs.GetFloat($"{setBtn.name}Size");
        opacitySlider.value = PlayerPrefs.GetFloat($"{setBtn.name}Opacity");

        keyPanelArea.OnOffDrag(setBtn);
    }


    // When Size Slider Value Change
    public void SettingSizeSlider()
    {
        size = (sizeSlider.value + 50) / sizeSlider.maxValue;
        setBtnRect.sizeDelta = beforeSize * size;
    }

    // When Opacity Slider Value Change
    public void SettingOpacitySlider()
    {
        color = setBtnImage.color;
        color.a = opacitySlider.value / opacitySlider.maxValue;
        setBtnImage.color = color;
    }

    // Current Setting size, Opacity Save
    public void SaveSliderValue()
    {
        PlayerPrefs.SetFloat($"{setBtn.name}Size", sizeSlider.value);
        PlayerPrefs.SetFloat($"{setBtn.name}Opacity", opacitySlider.value);
        PlayerPrefs.SetFloat($"{setBtn.name}transX", setBtnRect.anchoredPosition.x);
        PlayerPrefs.SetFloat($"{setBtn.name}transY", setBtnRect.anchoredPosition.y);
        PlayerPrefs.Save();
    }

    // setBtn TransForm move
    public void moveUI(string direction)
    {
        switch (direction)
        {
            case "Right":
                TempRect = new Vector2(setBtnRect.anchoredPosition.x+1, setBtnRect.anchoredPosition.y);
                break;
            case "Left":
                TempRect = new Vector2(setBtnRect.anchoredPosition.x - 1, setBtnRect.anchoredPosition.y);
                break;
            case "Down":
                TempRect = new Vector2(setBtnRect.anchoredPosition.x, setBtnRect.anchoredPosition.y - 1);
                break;
            case "Up":
                TempRect = new Vector2(setBtnRect.anchoredPosition.x, setBtnRect.anchoredPosition.y + 1);
                break;
            default:
                Debug.Log("범위 벗어남");
                break;
        }

        x = Mathf.Clamp(TempRect.x, -pHalfWidth + halfWidth, pHalfWidth - halfWidth);
        y = Mathf.Clamp(TempRect.y, -pHalfHeight + halfHeight, pHalfHeight - halfHeight);
        TempRect = new Vector2(x, y);
        setBtnRect.anchoredPosition = TempRect;
    }
}
