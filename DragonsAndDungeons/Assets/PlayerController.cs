using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public Rigidbody2D rb;
    Vector2 dir = new Vector2(0,0);
    // Update is called once per frame
    void Update()
    {
        dir.x = Input.GetAxisRaw("Horizontal");
        dir.y = Input.GetAxisRaw("Vertical");
        dir.Normalize();
    }

    void FixedUpdate() {
        rb.MovePosition(rb.position + dir * speed * Time.deltaTime);
    }
}
