using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public Transform PlayerHolderTransform;

    void OnEnable() {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerHolderTransform = transform.Find("/Players");
    }
}
