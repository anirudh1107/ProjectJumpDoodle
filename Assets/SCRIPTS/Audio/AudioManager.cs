using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager _instance;

    [SerializeField] private SoundCollectionSo soundCollectionSo;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void PlayJumpSound() {
        PlayRandomSound(soundCollectionSo.JumpSounds);
    }

    public void PlayDeathSound() {
        PlayRandomSound(soundCollectionSo.DeathSounds);
    }

    public void PlayShootingSound() {
        PlayRandomSound(soundCollectionSo.ShootingSounds);
    }

    public void PlayCollectionSound() {
        PlayRandomSound(soundCollectionSo.CollectionSounds);
    }

    public void PlayMusicTrack() {
        PlayRandomSound(soundCollectionSo.MusicTracks);
    }

    public void PlayRandomSound(SoundSo[] soundSos) {
        if (soundSos.Length == 0) {
            Debug.LogWarning("No SoundSos provided to PlayRandomSound.");
            return;
        }

        int randomIndex = Random.Range(0, soundSos.Length);
        SoundSo randomSoundSo = soundSos[randomIndex];
        SetupAndPlaySound(randomSoundSo);
    }

    private void SetupAndPlaySound(SoundSo soundSo) {

        float pitch = soundSo.pitch;
        if (soundSo.pitchRandomization) {
            pitch += Random.Range(-soundSo.pitchRandomizationModifier, soundSo.pitchRandomizationModifier);
        }

        PlaySound(soundSo.audioClip, soundSo.volume, pitch, soundSo.loop);
    }

    private void PlaySound(AudioClip audioClip, float volume, float pitch, bool loop) {
        GameObject soundObject = new GameObject("Sound_" + audioClip.name);
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.loop = loop;

        audioSource.Play();

        if(!loop) {
            StartCoroutine(DestroyAudioSourceAfterPlaying(soundObject, audioClip.length));
        }
        
    }

    private System.Collections.IEnumerator DestroyAudioSourceAfterPlaying(GameObject soundObject, float clipLength) {
        yield return new WaitForSeconds(clipLength);
        Destroy(soundObject);
    }
}
