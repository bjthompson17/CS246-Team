using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class Character : MonoBehaviour
{
    private Dictionary<Item,int> inventory = new Dictionary<Item,int>();
    
    public int HP, maxHP;
    private bool dead = false;

    public bool isCollector = true;
    public bool safe = false;

    public bool IsDead() {
        return dead;
    }
    public void GiveItem(Item item, int count) {
        if(dead) return;

        if(inventory.ContainsKey(item))
            inventory[item] += count;
        else 
            inventory.Add(item,count);
    }

    public int UseItem(Item item) {
        if(dead) return 0;
        if(item == null) return 0;
        if(inventory.ContainsKey(item)) {
            inventory[item] -= 1;
            if(inventory[item] <= 0) {
                inventory.Remove(item);
            }
            return 1;
        } else {
            return 0;
        }
    }

    public int QueryInventory(Item item) {
        if(!inventory.ContainsKey(item))
            return 0;
        else
            return inventory[item];
    }

    public Dictionary<Item,int> QueryInventory() {
        return inventory;
    }

    public void ChangeHP(int amount) {
        if(amount < 0) {
            StartCoroutine(Flash(Color.red));
        } else if (amount > 0) {
            StartCoroutine(Flash(Color.green));
        } else {
            StartCoroutine(Flash(new Color(0.5f,0.5f,0.5f,1f)));
        }
        HP += amount;
        if(HP > maxHP) HP = maxHP;
        if(HP <= 0) {
            HP = 0;
            if(!dead)
                BroadcastMessage("Kill");
        }
    }


    //TODO: figure out how to cleanly get this into sprite object
    IEnumerator Flash(Color flashColor) {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if(renderer == null) {
            renderer = GetComponentInChildren<SpriteRenderer>();
        }
        renderer.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        renderer.color = Color.white;
    }

    int maxIter = 1000;
    int iter = 0;
    void Kill() {
        dead = true;
        SetCollidersEnabled(false);
        Vector3 newPos;
        foreach(KeyValuePair<Item, int> entry in inventory) {
            for(int i = 0;i < entry.Value;i++) {
                iter = 0;
                Collider2D hit;
                do {
                    //Make sure to disable trigger collider queries
                    //otherwise items will show up behind walls
                    iter++;
                    newPos = transform.position + new Vector3(Random.Range(-2f,2f),Random.Range(-2f,2f),0);
                    hit = Physics2D.Linecast(transform.position,newPos).collider;

                    //Debug.Log("Iteration: " + iter + ", Collider: " + (hit != null ? hit.gameObject.name : null));
                }while((hit != null && !hit.isTrigger) && iter < maxIter);

                entry.Key.Instantiate(newPos);
            }
        }
        inventory.Clear();
    }

    void Revive () {
        dead = false;
        SetCollidersEnabled(true);
    }

    void SetCollidersEnabled(bool value) {
        Collider2D[] colliders = gameObject.GetComponents<Collider2D>();
        foreach(Collider2D col in colliders) {
            col.enabled = value;
        }
    }
}
