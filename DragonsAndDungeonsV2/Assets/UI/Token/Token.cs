using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class Token : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IRightClickable
{
    public Character LinkedCharacter = new Character();
    [SerializeField]
    Behaviour HaloEffect;

    private Vector3 offset;
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private float MaxSpeed = 10f;
    private bool dragging = false;
    [SerializeField]
    Text Label;

    void Start() {
        this.gameObject.name = LinkedCharacter.Name;
        if(Label != null) Label.text = LinkedCharacter.Name;
        if(rb == null) rb = GetComponent<Rigidbody2D>();
        if(HaloEffect != null) HaloEffect.enabled = false; 
    }

    void FixedUpdate() {
        if(!Camera.main.pixelRect.Contains(Input.mousePosition)) {
            dragging = false;
        }
        if(dragging) {
            Vector2 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
            Vector2 difference = newPos - rb.position;
            rb.velocity = difference / Time.deltaTime;
            if(rb.velocity.sqrMagnitude > MaxSpeed * MaxSpeed) {
                rb.velocity = rb.velocity.normalized * MaxSpeed;
            }
            rb.MovePosition(rb.position + difference);
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(eventData.position);
        if(eventData.button == PointerEventData.InputButton.Left) {
            offset = (Vector3)mousePos - transform.position;
            dragging = true;
        } 
    }

    public void OnPointerUp(PointerEventData eventData) {
        dragging = false;
    }

    public GameObject[] CreateMenuItems() {
        List<GameObject> menuItems = new List<GameObject>();
        ContextMenu menu = GameManager.windowManager.ContextMenu;
        Terminal terminal = GameManager.windowManager.Terminal;
        if(GameManager.SelectedToken == this) {
            menuItems.Add(menu.CreateItem("Deselect",() => GameManager.SelectToken(null)));
            GameObject[] buttons = new GameObject[2];
            buttons[0] = menu.CreateItem("Dodge", () => terminal.Log($"{LinkedCharacter.Name} Dodges."));
            buttons[1] = menu.CreateItem("Use Item", () => terminal.Log($"{LinkedCharacter.Name} Used an Item."));
            menuItems.Add(menu.CreateItemGroup("Actions", buttons));
            menuItems.Add(menu.CreateItem("Remove Token", () => {
                GameObject.Destroy(this.gameObject);
                GameManager.SelectToken(null);
                }));
        } else {
            menuItems.Add(menu.CreateItem("Select",() => GameManager.SelectToken(this)));
            if(GameManager.SelectedToken != null){
                GameObject[] buttons = new GameObject[2];
                buttons[0] = menu.CreateItem($"Attack {LinkedCharacter.Name}", () => terminal.Log($"{GameManager.SelectedToken.LinkedCharacter.Name} Attacks {LinkedCharacter.Name}"));
                buttons[1] = menu.CreateItem($"Target {LinkedCharacter.Name} with a spell", () => terminal.Log($"{LinkedCharacter.Name} has been Abrakadaberad by {GameManager.SelectedToken.LinkedCharacter.Name}."));
                menuItems.Add(menu.CreateItemGroup("Actions", buttons));
            }
            menuItems.Add(menu.CreateItem("Remove Token", () => GameObject.Destroy(this.gameObject)));
        }
        menuItems.Add(menu.CreateItem("Edit Character",() => GameManager.windowManager.CharacterEditor.OpenEditor(this)));
        menuItems.Add(menu.CreateItem("View Character",() => GameManager.windowManager.CharacterEditor.OpenEditor(this,false)));
        if(menuItems.Count <= 0) return null;
        return menuItems.ToArray();
    }

    public void Highlight() {
        if(HaloEffect != null) HaloEffect.enabled = true;
    }

    public void Unhighlight() {
        if(HaloEffect != null) HaloEffect.enabled = false;
    }

    public void RefreshInfo() {
        Label.text = LinkedCharacter.Name;
    }

}
