using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowManager : MonoBehaviour
{
    public ContextMenu ContextMenu;
    public Terminal Terminal;
    public CharacterManager CharacterManager;
    public CharacterEditor CharacterEditor;
    public GameObject controlMenu;

    private bool[] mouseButtonUpStates = new bool[3];
    private bool MiddleDrag = false;
    private Vector3 DragOrigin = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        GameManager.windowManager = this;
        ContextMenu.Close();
        CharacterManager.UpdateAll();
        MiddleDrag = false;
    }


    private List<RaycastResult> Raycast(Vector3 mousePos) {
        List<RaycastResult> Hits = new List<RaycastResult>();
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        EventSystem.current.RaycastAll(eventData,Hits);
        return Hits;
    }
    
    void Update() {
        bool AnyButtonUp = false;
        for(int i = 0;i < 3;i++) {
            if(Input.GetMouseButtonUp(i)) {
                mouseButtonUpStates[i] = true;
                AnyButtonUp = true;
            } else {
                mouseButtonUpStates[i] = false;
            }
        }

        if(Input.mouseScrollDelta.y != 0) {
            List<RaycastResult> Hits = Raycast(Input.mousePosition);
            if(Hits.Count <= 0 || (Hits.Count > 0 && Hits[0].gameObject.transform.root.tag != "Screen UI")) {
                if(Input.mouseScrollDelta.y > 0) {
                    DragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Camera.main.orthographicSize *= 0.9f;
                    Vector3 DragDelta = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
                    Camera.main.transform.position = DragOrigin - DragDelta;
                } else if (Input.mouseScrollDelta.y < 0) {
                    DragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Camera.main.orthographicSize *= 1.1f;
                    Vector3 DragDelta = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
                    Camera.main.transform.position = DragOrigin - DragDelta;
                }
            }
        }

        if(Input.GetMouseButtonDown(2)) {
            DragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            MiddleDrag = true;
        }

        if(MiddleDrag){
            Vector3 DragDelta = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
            Camera.main.transform.position = DragOrigin - DragDelta;
        }


        if(AnyButtonUp) {
            List<RaycastResult> Hits = Raycast(Input.mousePosition);
            ContextMenu.Close();
            if(Hits.Count > 0) {
                GameObject firstHit = Hits[0].gameObject;
                IRightClickable rightClickableObject = firstHit.GetComponent<IRightClickable>();
                if(rightClickableObject != null && mouseButtonUpStates[1]){
                    GameObject[] menuItems = rightClickableObject.CreateMenuItems();
                    if(menuItems != null) {
                        ContextMenu.AddItems(menuItems);
                        ContextMenu.OpenAtPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    }
                }
            }
            if(mouseButtonUpStates[2]){
                MiddleDrag = false;
            }
        }
    }

    
}
