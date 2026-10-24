using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AreaData[] allAreas;
    [SerializeField] private AudioClip menuMusic;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(AudioClip music)
    {
        if (musicSource.clip == music && musicSource.isPlaying)
            return;

        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayAreaMusic(AreaData areaNumber)
    {
        if (areaNumber != null)
            PlayMusic(areaNumber.backgroundMusic);
    }

    public void PlayAreaMusicInLevel(LevelData levelID)
    {
        foreach (AreaData area in allAreas)
        {
            if (area != null && area.levels.Contains(levelID))
            {
                PlayAreaMusic(area);
                return;
            }
        }
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlaySound(AudioClip _sound)
    {
        sfxSource.PlayOneShot(_sound);
    }
}
