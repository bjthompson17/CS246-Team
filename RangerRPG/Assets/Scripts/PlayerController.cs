using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    float speed = 5f;
    Vector2 input = new Vector2(0,0);
    public Rigidbody2D rb;
    public Animator anim;
    public AudioSource SoundSource;
    public AudioClip AttackSound;
    [Range(0f,1f)] public float AttackVolume = 1f;
    public AudioClip WalkSound;
    [Range(0f,1f)] public float WalkVolume = 1f;
    public AudioClip FistSound;
    [Range(0f,1f)] public float FistVolume = 1f;

    public Character me;
    public float AttackRate = 2f;
    public float UnarmedRange = 1f;
    public int UnarmedDamage = 5;
    bool canAttack = true;
    bool canMove = true;
    private float walkTimer = 0;

    void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position,UnarmedRange);
    }
    IEnumerator Attack(Vector3 direction) {
        if(!canAttack) yield break;
        if(me.safe) yield break;
        canAttack = false;

        float beforeAnim = Time.fixedTime;
        anim.SetFloat("Horizontal",direction.x);
        Item arrow = ItemManager.Query("Arrow");
        if(me.UseItem(arrow) > 0) {
            anim.SetTrigger("Attack");
            yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
            yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"));

            if(SoundSource != null && AttackSound != null) {
                SoundSource.PlayOneShot(AttackSound,AttackVolume);
            }
            arrow.itemPrefab.GetComponent<Ammunition>().Fire(transform.position, direction * 100, gameObject);
        } else {
            anim.SetTrigger("UnarmedAttack");
            canMove = false;
            yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Unarmed"));
            yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Unarmed"));

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position,UnarmedRange);
            foreach(Collider2D hit in hits) {
                Character character = hit.GetComponent<Character>();
                if(character != null && character.gameObject != gameObject) {
                    character.ChangeHP(-UnarmedDamage);
                    if(SoundSource != null && FistSound != null) {
                        SoundSource.PlayOneShot(FistSound,FistVolume);
                    }
                    break;
                }
            }
            if(!me.IsDead())
                canMove = true;
        }
        
        float deltaT = Time.fixedTime - beforeAnim;
        if(deltaT < 1/AttackRate) {
            yield return new WaitForSeconds(1/AttackRate - deltaT);
        }

        canAttack = true;
    }

    void FixedUpdate() {
        input.x = Input.GetAxisRaw("Horizontal") * speed;
        input.y = Input.GetAxisRaw("Vertical") * speed;

        if(Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
            Vector3 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition)
                            - transform.position).normalized;
            StartCoroutine(Attack(direction));
        }

        if(canMove) {
            rb.velocity = input.normalized * speed;
            anim.SetFloat("Speed",rb.velocity.SqrMagnitude());
            walkTimer += Time.deltaTime;
            if(rb.velocity.sqrMagnitude > 0 && SoundSource != null 
                && WalkSound != null && walkTimer >= WalkSound.length) {
                walkTimer = 0;
                SoundSource.PlayOneShot(WalkSound,WalkVolume);
            }
        } else {
            rb.velocity = new Vector2(0,0);
            anim.SetFloat("Speed", 0);
        }
        if(input.x != 0)
            anim.SetFloat("Horizontal", input.x);
    }

    void Kill() {
        canMove = false;
        canAttack = false;
        GameManager.GameOver = true;
    }
}
