using UnityEngine;

namespace MyPool
{
    public class AudioPool : MyObjectPool
    {
        public void PlaySoundAtPoint(string key, AudioClip clip, Vector3 position, float minPitch = 0.92f, float maxPitch = 1.08f, bool isExplosion = false)
        {
            if (clip == null) return;
            GameObject audioObj = Get(key);
            if (audioObj == null) return;
            audioObj.transform.position = position;
            if (audioObj.TryGetComponent<AudioSource>(out var source))
            {
                if (AudioManager.Instance != null)
                {
                    source.outputAudioMixerGroup = AudioManager.Instance.GetSFXGroup();
                }
                source.pitch = Random.Range(minPitch, maxPitch);
                source.clip = clip;
                if (isExplosion)
                {
                    source.minDistance = 15f;
                    source.maxDistance = 50f;
                }
                else
                {
                    source.minDistance = 3f;
                    source.maxDistance = 30f;
                }
                source.Play();
                StartCoroutine(AutoReleaseRoutine(audioObj, key, clip.length));
            }
            else
            {
                Release(audioObj, key);
            }
        }

        private System.Collections.IEnumerator AutoReleaseRoutine(GameObject obj, string key, float duration)
        {
            yield return new WaitForSeconds(duration + 0.1f);
            if (obj != null && obj.activeSelf)
            {
                if (obj.TryGetComponent<AudioSource>(out var source))
                {
                    source.Stop();
                    source.clip = null;
                }
                Release(obj, key);
            }
        }

        public override void DeactivateAllPoolObjects()
        {
            StopAllCoroutines();
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf)
                {
                    if (child.gameObject.TryGetComponent<AudioSource>(out var source))
                    {
                        source.Stop();
                        source.clip = null;
                    }

                    if (child.gameObject.TryGetComponent<PoolItem>(out var poolItem))
                    {
                        Release(child.gameObject, poolItem.key);
                    }
                    else
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
    }
}