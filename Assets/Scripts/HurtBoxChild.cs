using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBoxChild : MonoBehaviour
{
    Character parent;

    void Start()
    {
        transform.parent.GetComponent<Character>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        parent.OnHurtBox(collider);
    }
}
