using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{

    private static Dictionary<string,Item> gameItems = new Dictionary<string, Item>();
    // Start is called before the first frame update
    void Awake()
    {
        UpdateItems();
    }

    void UpdateItems() {
        Item[] items = Resources.FindObjectsOfTypeAll<Item>();
        gameItems.Clear();
        foreach(Item i in items) {
            if(gameItems.ContainsKey(i.id)) {
                i.id += "_";
            }
            gameItems.Add(i.id,i);
        }
    }

    public static Item Query(string id) {
        if(gameItems.ContainsKey(id))
            return gameItems[id];
        else
            return null;
    }

    public static Item[] QueryType(string type) {
        List<Item> output = new List<Item>();
        foreach(KeyValuePair<string,Item> i in gameItems) {
            if(i.Value.type == type)
                output.Add(i.Value);
        }
        return output.ToArray();
    }

}
