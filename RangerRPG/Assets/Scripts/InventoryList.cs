using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InventoryList : MonoBehaviour
{
    public Character InventoryTarget;
    [SerializeField] public InventoryListItem ListPrefab;

    Dictionary<Item,InventoryListItem> currentlyDisplayed = new Dictionary<Item, InventoryListItem>();

    public void UpdateList() {
        Dictionary<Item,int> items = InventoryTarget.QueryInventory();

        foreach(KeyValuePair<Item,int> entry in items.ToArray()) {
            if(!currentlyDisplayed.ContainsKey(entry.Key)) {
                
                InventoryListItem instance = GameObject.Instantiate(
                    ListPrefab,
                    new Vector3(0,0,0),
                    Quaternion.identity,
                    transform);

                instance.SetData(entry.Key.icon, entry.Key.id, entry.Value.ToString());

                currentlyDisplayed.Add(entry.Key,instance);
            } else {
                currentlyDisplayed[entry.Key].count.text = entry.Value.ToString();
            }
        }

        // clean up items that have disappeared
        foreach(KeyValuePair<Item,InventoryListItem> entry in currentlyDisplayed) {
            if(!items.ContainsKey(entry.Key)) {
                Destroy(entry.Value.gameObject);
                currentlyDisplayed.Remove(entry.Key);
            }
        }
    }

    bool NeedsUpdate() {
        Dictionary<Item,int> items = InventoryTarget.QueryInventory();
        if(items.Count != currentlyDisplayed.Count) return true;
        foreach(KeyValuePair<Item,int> entry in items.ToArray()) {
            if(!currentlyDisplayed.ContainsKey(entry.Key))
                return true;
            currentlyDisplayed[entry.Key].count.text = entry.Value.ToString();
        }
        return false;
    }

    void LateUpdate() {
        if(NeedsUpdate())
            UpdateList();
    }
}
