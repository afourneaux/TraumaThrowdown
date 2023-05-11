using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public GameObject AudioPrefab;
    public static AudioController instance;
    string musicID;

    float masterVolume = 1.0f;
    bool masterMute = false;
    float soundVolume = 1.0f;
    bool soundMute = false;
    float musicVolume = 1.0f;
    bool musicMute = false;
    int counter = 1;

    void OnEnable() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    // Play a song. Only one song may be played at once, new songs overwrite old
    public string PlayMusic(string soundName, float volume = 1f) {
        if (musicID != null) {
            StopByID(musicID);
        }
        musicID = CreateSoundObject(soundName, musicVolume, true, volume);
        return musicID;
    }

    // Special case of StopByID that stops the current music
    public void StopMusic() {
        if (musicID != null) {
            StopByID(musicID);
        }
    }

    // Stop an audio source based on a given key
    public void StopByID(string soundID) {
        if (string.IsNullOrEmpty(soundID)) {
            return;
        }
        Transform audioTransform = transform.Find(soundID);
        if (audioTransform != null) {
            Destroy(audioTransform.gameObject);
        } else {
            Debug.LogError($"Could not find Sound ID: {soundID}");
        }
    }

    // Play the sound at the given index and return a key to stop it later
    public string PlaySound(string soundName, bool looping = false, float volume = 1.0f) {
        return CreateSoundObject(soundName, soundVolume, looping, volume);
    }

    private string CreateSoundObject(string soundName, float volumeModifier, bool looping = false, float volume = 1.0f) {
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{soundName}");
        if(clip == null) {
            Debug.LogError($"Could not find audio resource - {soundName}");
            return null;
        }

        GameObject go = Instantiate(AudioPrefab, Vector3.zero, Quaternion.identity, transform);
        go.name = go.name + counter.ToString();
        counter++;
        AudioSource audio = go.GetComponent<AudioSource>();
        AudioMetadata data = go.GetComponent<AudioMetadata>();
        data.localVolume = volume;
        audio.clip = clip;
        audio.loop = looping;
        audio.volume = volume * masterVolume * volumeModifier;
        audio.Play();
        return go.name;
    }

    public void ChangeMasterVolume(float newVolume) {
        masterVolume = newVolume;
        ChangeMusicVolume(musicVolume);
        ChangeSoundVolume(soundVolume);
    }

    public void ChangeMusicVolume(float newVolume) {
        musicVolume = newVolume;
        if (string.IsNullOrEmpty(musicID)) {
            return;
        }
        AudioMetadata audio = transform.Find(musicID).GetComponent<AudioMetadata>();
        audio.SetVolume(masterVolume * musicVolume);
    }

    public void ChangeSoundVolume(float newVolume) {
        soundVolume = newVolume;
        foreach (AudioMetadata audio in transform.GetComponentsInChildren<AudioMetadata>()) {
            if (audio.transform.name != musicID) {
                audio.SetVolume(masterVolume * soundVolume);
            }
        }
    }
}
