using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item[] rewards;
    public AudioClip PickupSound;
    void OnTriggerEnter2D(Collider2D collider) {
        Character character = collider.GetComponent<Character>();
        if(character == null) return;
        if(!character.isCollector) return;

        foreach(Item i in rewards)
            character.GiveItem(i,1);
        if(PickupSound != null) {
            AudioSource.PlayClipAtPoint(PickupSound,transform.position);
        }
        Destroy(gameObject);
    }
}
