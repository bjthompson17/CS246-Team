using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ProjectileController : MonoBehaviour
{
    public Item[] destructionDrops;
    public int damage;
    public Rigidbody2D rb;
    private GameObject source = null;
    private int life = 100;
    public AudioClip HitSound;

    void OnBecameInvisible() {
        Kill(false);
    }

    public void Fire(Vector3 velocity, GameObject src) {
        rb.velocity = velocity;
        transform.rotation = Quaternion.LookRotation(velocity,Vector3.forward);
        source = src;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Character character = collider.GetComponent<Character>();
        if(character != null) {
            if(character.gameObject == source) return;
            character.ChangeHP(-damage);
            Kill(true);
        } else if(!collider.isTrigger) {
            Kill(true);
        }
    }

    bool HitCharacter(Character character, bool dies) {
        
        return true;
    }

    void Kill(bool play_sound) {
        foreach(Item i in destructionDrops) {
            i.Instantiate(rb.position);
        }
        gameObject.SetActive(false);
        if(play_sound)
            AudioSource.PlayClipAtPoint(HitSound,transform.position);
        Destroy(gameObject);
    }

    void FixedUpdate() {
        life--;
        if(life <= 0) {
            Kill(false);
        }
    }
}
