using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputList : MonoBehaviour
{
    public InputRow InputRowPrefab;
    public bool interactable = true;

    private Dictionary<string,InputRow> Rows;
    public int Count {
        get => transform.childCount;
    }

    void Awake() {
        Clear();
    }

    public void AddItem() {
        if(!interactable) return;
        AddRow();
    }

    public InputRow AddRow() {
        InputRow instance = GameObject.Instantiate<InputRow>(InputRowPrefab, transform);
        instance.transform.SetParent(transform);
        return instance;
    }

    public InputRow[] GetAllRows() {
        InputRow[] output = new InputRow[transform.childCount];
        for(int i = 0;i < transform.childCount;i++) {
            output[i] = transform.GetChild(i).GetComponent<InputRow>();
        }
        return output;
    }

    public void Clear() {
        for(int i = transform.childCount-1;i >= 0;i--) {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }
}
