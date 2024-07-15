using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteScript : MonoBehaviour
{
    [HideInInspector]
    public GameManager manager;

    void Update(){
        if(transform.position.y < -4.5f){
            Destroy(gameObject);
            manager.AddScore(100f);
        }
    }
}
