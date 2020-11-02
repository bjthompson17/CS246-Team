using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    public float SpawnRate = 0.2f;
    private float time_passed = 0f;

    public EnemyController EnemyPrefab;
    public Character target;
    public Vector2 RangeLow = new Vector2(-7.5f,-9f);
    public Vector2 RangeHigh = new Vector2(18f,4f);
    public bool Active = false;

    void Awake() {
        Active = true;
    }

    // Update is called once per frame 
    void FixedUpdate()
    {
        if(GameManager.GameOver) {
            Active = false;
            gameObject.SetActive(false);
        }
        
        time_passed += Time.deltaTime;
        if(time_passed >= 1/SpawnRate) {
            time_passed = 0;
            if(Active)
                SpawnEnemy();
        }
    }

    void SpawnEnemy() {
        if(EnemyPrefab != null) {
            Vector3 newPos;
            newPos = new Vector3(Random.Range(RangeLow.x,RangeHigh.x),Random.Range(RangeLow.y,RangeHigh.y),0);

            EnemyController instance = GameObject.Instantiate(EnemyPrefab,newPos,Quaternion.identity);
            instance.target = target;
        }
        
    }
}
