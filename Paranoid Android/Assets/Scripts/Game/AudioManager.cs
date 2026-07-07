using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioMixer mainMixer;

    public AudioSource bgmSource;

    public AudioMixerGroup sfxGroup;

    public AudioClip weaponFire;
    public AudioClip fleshHitClip;
    public AudioClip wallHitClip;
    public AudioClip genericExplosionClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return;
        StartCoroutine(FadeTrackRoutine(clip));
    }

    private IEnumerator FadeTrackRoutine(AudioClip newClip)
    {
        float fadeTime = 0.8f;
        float startVolume = bgmSource.volume;

        if (bgmSource.isPlaying)
        {
            while (bgmSource.volume > 0)
            {
                bgmSource.volume -= startVolume * (Time.deltaTime / fadeTime);
                yield return null;
            }
        }

        bgmSource.clip = newClip;
        bgmSource.loop = true;
        bgmSource.Play();

        while (bgmSource.volume < startVolume)
        {
            bgmSource.volume += startVolume * (Time.deltaTime / fadeTime);
            yield return null;
        }
        bgmSource.volume = startVolume;
    }

    public void SetGroupVolume(string parameterName, float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        mainMixer.SetFloat(parameterName, dB);
    }

    public void Play3DSound(AudioSource source, AudioClip clip, float minPitch = 0.92f, float maxPitch = 1.08f)
    {
        if (source == null || clip == null) return;

        source.pitch = Random.Range(minPitch, maxPitch);
        source.PlayOneShot(clip);
    }

    public void PlayHitSound(AudioSource source, bool isFlesh)
    {
        AudioClip clipToPlay = isFlesh ? fleshHitClip : wallHitClip;
        Play3DSound(source, clipToPlay);
    }

    public AudioMixerGroup GetSFXGroup()
    {
        return sfxGroup;
    }

    internal AudioClip GetWeaponFireClip()
    {
        return weaponFire;
    }
}