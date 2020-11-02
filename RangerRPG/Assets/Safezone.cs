using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Safezone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider) {
        Character character = collider.GetComponent<Character>();
        if(character == null) return;
        character.safe = true;
    }

    void OnTriggerExit2D(Collider2D collider) {
        Character character = collider.GetComponent<Character>();
        if(character == null) return;
        character.safe = false;
    }

}
