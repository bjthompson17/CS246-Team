using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextItemGroup : MonoBehaviour
{
    [SerializeField]
    public ContextMenu Submenu;
    [SerializeField]
    Color NormalColor;
    [SerializeField]
    Color HighlightedColor;
    private Image img;
    void Start() {
        img = gameObject.GetComponent<Image>();
        if(img != null && NormalColor == null) {
            NormalColor = img.color;
        }
    }
    public void MouseEnter() {
        if(img != null && HighlightedColor != null) {
            img.color = HighlightedColor;
        }
        Submenu.Show();
    }
    public void MouseExit() {
        if(img != null && NormalColor != null) {
            img.color = NormalColor;
        }
        Submenu.Hide();
    }
}
