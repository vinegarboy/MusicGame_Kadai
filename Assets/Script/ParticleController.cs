using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        //パーティクルが終了したら削除
        if (!GetComponent<ParticleSystem>().IsAlive()){
            Destroy(gameObject);
        }
    }
}
