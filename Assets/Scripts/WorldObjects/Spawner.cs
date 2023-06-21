using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public float cooldown;
    
    void OnEnable()
    {
        cooldown = 0;
    }

    void Update()
    {
        cooldown -= Time.deltaTime;
    }
}
