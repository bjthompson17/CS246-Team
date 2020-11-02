using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Item", fileName = "newItem.asset")]
public class Item : ScriptableObject
{
    public Sprite icon;
    public string id;
    public string type;
    public GameObject itemPrefab;

    void Reset() {
        Debug.Log(id + " Reset");
        if(icon == null) {
            icon = itemPrefab.GetComponent<SpriteRenderer>().sprite;
            if(icon != null) return;
            icon = itemPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
            if(icon != null) return;
            Debug.LogError("Could not find sprite object for" + id);
        }
    }
    public void Instantiate(Vector3 position) {
        if(itemPrefab == null) return;

        GameObject.Instantiate(itemPrefab, position, Quaternion.identity);
    }
}
