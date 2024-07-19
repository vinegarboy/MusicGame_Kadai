using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteScript_WebGL : MonoBehaviour
{
    [HideInInspector]
    public GameManager_WebGL manager;

    void Update(){
        if(transform.position.y < -5.0f){
            manager.AddScore(2000);
            Destroy(gameObject);
        }
    }
}
