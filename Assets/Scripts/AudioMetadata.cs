using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMetadata : MonoBehaviour
{
    public float localVolume = 1.0f;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!audioSource.isPlaying) {
            Destroy(gameObject);
        }
    }

    public void SetVolume(float volumeModifier) {
        audioSource.volume = localVolume * volumeModifier;
    }
}
