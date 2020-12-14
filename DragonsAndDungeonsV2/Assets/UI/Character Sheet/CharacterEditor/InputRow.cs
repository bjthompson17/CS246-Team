using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputRow : MonoBehaviour
{
    public GameObject[] RowElements;
    public bool interactable = true;
    public void RemoveItem() {
        if(!interactable) return;
        GameObject.Destroy(this.gameObject);
    }
    
}
