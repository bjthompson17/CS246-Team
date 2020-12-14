using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool dragging = false;
    private Vector3 offset = new Vector3();

    void Update() {
        if(dragging) {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
        }
    }
    public void OnPointerDown(PointerEventData eventData) {
        if(eventData.button == PointerEventData.InputButton.Left) {
            dragging = true;
            offset = Camera.main.ScreenToWorldPoint(eventData.position) - transform.position;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        dragging = false;
    }
}
