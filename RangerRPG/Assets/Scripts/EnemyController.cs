using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D),typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public float speed = 5f;
    public Character me;
    public Animator anim;
    public Rigidbody2D rb;
    public Character target;
    bool canAttack = true;
    bool canMove = false;
    public int damage = 10;
    public float AttackRadius = 1f;
    public AudioClip DeathSound;
    [Range(0f,1f)] public float DeathVolume = 1f; 
    public AudioClip AttackSound;
    [Range(0f,1f)] public float AttackVolume = 1f; 

    void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position,AttackRadius);
    }
    void Awake() {
        StartCoroutine(SpawnIn());
    }

    IEnumerator SpawnIn() {
        canMove = false;
        GetComponent<Collider2D>().enabled = false;
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Revive"));
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Revive"));
        GetComponent<Collider2D>().enabled = true;
        InitInventory();
        canMove = true;
    }

    void InitInventory() {
        me.GiveItem(ItemManager.Query("Arrow"),4);
        me.GiveItem(ItemManager.Query("Coin"),3);
    }

    void FixedUpdate() {
        if(me.IsDead()) {
            rb.velocity = new Vector2(0,0);
            anim.SetFloat("Speed",0);
            return;
        }
        if(canMove && !target.safe){
            rb.velocity = ((Vector2)target.transform.position - rb.position).normalized * speed;
            if(rb.velocity.x < 0)
                anim.SetFloat("Horizontal",-1);
            else if(rb.velocity.x > 0)
                anim.SetFloat("Horizontal", 1);
            anim.SetFloat("Speed",rb.velocity.SqrMagnitude());
        }else {
            rb.velocity = new Vector2(0,0);
            anim.SetFloat("Speed",0);
        }
    }

    void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject == target.gameObject && canAttack) {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack() {
        canAttack = false;
        canMove = false;
        anim.SetTrigger("Attack");
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Attacking"));
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Attacking"));
        anim.ResetTrigger("Attack");

        AudioSource.PlayClipAtPoint(AttackSound,transform.position,AttackVolume);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position,AttackRadius);
        foreach(Collider2D collider in colliders) {
            if(collider == GetComponent<Collider2D>()) continue;
            Character character = collider.GetComponent<Character>();
            if(character != null)
                character.ChangeHP(-damage);
        }

        canMove = true;
        yield return new WaitForSeconds(0.5f);
        canAttack = true;
    }

    void Kill() {
        StartCoroutine(Cleanup());
    }

    IEnumerator Cleanup() {
        anim.SetTrigger("Die");
        if(DeathSound != null)
            AudioSource.PlayClipAtPoint(DeathSound,transform.position,DeathVolume);
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Death"));
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Death"));
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
}
