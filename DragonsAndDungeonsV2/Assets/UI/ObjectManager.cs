using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [SerializeField]
    public Token TokenPrefab;

    void Start() {
        GameManager.objectManager = this;
    }
    public void CreateNewToken(Character setCharacter) {
        if(TokenPrefab == null) {
            Debug.LogError("You need to set the Token Prefab in the Object Manager.");
            return;
        }
        
        Token instance = GameObject.Instantiate<Token>(TokenPrefab,transform);
        instance.transform.SetParent(transform);
        instance.transform.position = 
        new Vector3(Camera.main.transform.position.x, 
                    Camera.main.transform.position.y, 
                    instance.transform.position.z);
                    
        if(setCharacter != null) {
            instance.LinkedCharacter = setCharacter;
        }

        instance.gameObject.SetActive(true);
    }

    public void CreateNewToken() {
        CreateNewToken(null);
    }
}
