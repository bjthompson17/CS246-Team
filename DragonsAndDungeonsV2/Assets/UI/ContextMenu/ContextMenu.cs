using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

public class ContextMenu : MonoBehaviour
{

    [SerializeField]
    Button ItemPrefab;
    [SerializeField]
    ContextItemGroup ItemGroupPrefab;

    public void AddItem(GameObject item) {
        item.transform.SetParent(gameObject.transform);
        item.SetActive(true);
    }

    public void AddItems(GameObject[] items) {
        foreach(GameObject item in items) {
            AddItem(item);
        }
    }

    public GameObject CreateItem(string name, UnityAction callback) {
        Button instance = GameObject.Instantiate<Button>(ItemPrefab,transform);
        instance.onClick.AddListener(callback);
        Text ItemText = instance.GetComponentInChildren<Text>();
        if(ItemText != null) ItemText.text = name;
        return instance.gameObject;
    }

    public GameObject CreateItemGroup(string name, GameObject[] Items) {
        ContextItemGroup instance = GameObject.Instantiate<ContextItemGroup>(ItemGroupPrefab,transform);
        instance.Submenu.Close();
        Text ItemText = instance.GetComponentInChildren<Text>();
        if(ItemText != null) ItemText.text = name;
        foreach(GameObject item in Items) {
            instance.Submenu.AddItem(item.gameObject);
        }
        return instance.gameObject;
    }

    public void CreateAndAddItem(string name, UnityAction callback) {
        AddItem(CreateItem(name,callback).gameObject);
    }

    public void CreateAndAddItemGroup(string name, GameObject[] Items) {
        AddItem(CreateItemGroup(name,Items).gameObject);
    }

    public void Clear() {
        int childCount = transform.childCount;
        for(int i = childCount - 1;i >= 0;i--) {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void OpenAtPoint(Vector2 point) {
        transform.position = new Vector3(point.x,point.y,transform.position.z);
        gameObject.SetActive(true);
    }

    public void Close() {
        Hide();
        Clear();
    }
}
