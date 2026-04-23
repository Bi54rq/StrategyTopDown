using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip buyClip;
    public AudioClip attackClip;
    public AudioClip winClip;
    public AudioClip loseClip;

    private const string VolumeKey = "MasterVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        SetVolume(savedVolume);
    }

    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (sfxSource != null)
            sfxSource.volume = volume;

        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return PlayerPrefs.GetFloat(VolumeKey, 1f);
    }

    public void PlayBuy()
    {
        PlayClip(buyClip);
    }

    public void PlayAttack()
    {
        PlayClip(attackClip);
    }

    public void PlayWin()
    {
        PlayClip(winClip);
    }

    public void PlayLose()
    {
        PlayClip(loseClip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (sfxSource == null) return;
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayTestSound()
    {
        PlayClip(buyClip);
    }
}