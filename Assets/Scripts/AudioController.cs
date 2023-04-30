using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public GameObject AudioPrefab;
    public static AudioController instance;
    string musicID;

    static int counter = 1;

    void OnEnable() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    // Special case of PlaySound automatically overwrites any existing music
    public string PlayMusic(string soundName) {
        if (musicID != null) {
            StopByID(musicID);
        }
        musicID = PlaySound(soundName, true);
        return musicID;
    }

    // Stop an audio source based on a given key
    public void StopByID(string soundID) {
        if (string.IsNullOrEmpty(soundID)) {
            return;
        }
        Transform audioTransform = transform.Find(soundID);
        if (audioTransform != null) {
            Destroy(audioTransform.gameObject);
        }
    }

    // Play the sound at the given index and return a key to stop it later
    public string PlaySound(string soundName, bool looping = false, float volume = 1f) {
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{soundName}");
        if(clip == null) {
            Debug.LogError($"Could not find audio resource - {soundName}");
            return null;
        }

        GameObject go = Instantiate(AudioPrefab, Vector3.zero, Quaternion.identity, transform);
        go.name = go.name + counter.ToString();
        counter++;
        AudioSource audio = go.GetComponent<AudioSource>();
        audio.clip = clip;
        audio.loop = looping;
        audio.volume = volume;
        audio.Play();
        return go.name;
    }
}
