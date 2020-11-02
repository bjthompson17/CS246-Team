using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryListItem : MonoBehaviour
{
    public Image icon;
    public Text id;
    public Text count;

    public void SetData(Sprite icon, string id, string count) {
        this.icon.sprite = icon;
        this.id.text = id;
        this.count.text = count;
    }
}
