using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    [SerializeField]
    GameObject[] tapArea;
    void Update(){
        if(Input.GetKey(KeyCode.A)){
            RaycastHit hit;
            if (Physics.Raycast(tapArea[0].transform.position,new Vector3(0,1,0),out hit ,8.5f)){
                Debug.Log("Hit!!"+hit.collider.gameObject.name+"<=Name Distance=>"+hit.distance);
            }
        }
    }
}
