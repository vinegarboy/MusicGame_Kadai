using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteScript : MonoBehaviour
{
    [HideInInspector]
    public GameManager manager;

    void Update(){
        if(transform.position.y < -5.0f){
            manager.AddScore(2000);
            Destroy(gameObject);
        }
    }
}
