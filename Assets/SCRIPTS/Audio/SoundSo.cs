using UnityEngine;

[CreateAssetMenu(fileName = "SoundSo", menuName = "Scriptable Objects/SoundSo")]
public class SoundSo : ScriptableObject
{
    public SoundType soundType;
    public AudioClip audioClip;
    [Range(0f, 1f)]
    public float volume;
    [Range(0.1f, 3f)]
    public float pitch;
    public bool loop;
    public bool pitchRandomization;
    [Range(0f, 1f)]
    public float pitchRandomizationModifier;

}

public enum SoundType
{
    SFX,
    Music
}
