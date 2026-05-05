using UnityEngine;

[CreateAssetMenu(fileName = "SoundCollectionSo", menuName = "Scriptable Objects/SoundCollectionSo")]
public class SoundCollectionSo : ScriptableObject
{
    public SoundSo[] JumpSounds;
    public SoundSo[] DeathSounds;
    public SoundSo[] ShootingSounds;
    public SoundSo[] CollectionSounds;

    public SoundSo[] MusicTracks;
    public SoundSo[] MenuMusicTracks;
    public SoundSo[] GameoverMusicTracks;
    
}
