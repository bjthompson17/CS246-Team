using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryKeypressHandler : MonoBehaviour
{
    [SerializeField] public Animator animator;
    [SerializeField] string triggername = "OpenClose";
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && animator != null) {
            animator.SetTrigger(triggername);
        }
    }
}
