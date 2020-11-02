using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammunition : MonoBehaviour
{
    public ProjectileController projectilePrefab;
    public void Fire(Vector3 position, Vector3 velocity, GameObject source) {
        if(projectilePrefab != null) {
            GameObject instance = GameObject.Instantiate(
                projectilePrefab.gameObject,position,
                Quaternion.LookRotation(velocity,Vector3.forward)
                );
            instance.GetComponent<ProjectileController>().Fire(velocity,source);
        }
    }
}
