using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    private Dictionary<string, AudioClip> bgmClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    [System.Serializable]
    public struct NamedAudioClip
    {
        public string name;
        public AudioClip clip;
    }

    public NamedAudioClip[] bgmClipList;
    public NamedAudioClip[] sfxClipList;

    private Coroutine currentBGMCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioClip();
            SoundManager.Instance.PlayBGM("TitleBgm", 0.2f);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    //private void Start()
    //{
    //    string activeSceneName = SceneManager.GetActiveScene().name;
    //    OnSceneLoaded(activeSceneName);
    //}

    //public void OnSceneLoaded(string sceneName)
    //{
    //    if (sceneName == "GameScene")
    //    {
    //        PlayBGM("GameScene1", 1.0f);
    //    }
    //    else if (sceneName == "GameScene2")
    //    {
    //        PlayBGM("GameScene2", 1.0f);
    //    }
    //}

    void InitializeAudioClip()
    {
        foreach (var bgm in bgmClipList)
        {
            if (!bgmClips.ContainsKey(bgm.name))
            {
                bgmClips.Add(bgm.name, bgm.clip);
            }
        }

        foreach (var sfx in sfxClipList)
        {
            if (!sfxClips.ContainsKey(sfx.name))
            {
                sfxClips.Add(sfx.name, sfx.clip);
            }
        }
    }

    public void PlayBGM(string name, float fadeDuration = 1.0f)
    {
        if (bgmClips.ContainsKey(name))
        {
            if (currentBGMCoroutine != null) // 코루틴 중복 방지
            {
                StopCoroutine(currentBGMCoroutine);
            }

            currentBGMCoroutine = StartCoroutine(FadeOutBGM(fadeDuration, () =>
            {
                bgmSource.spatialBlend = 0f;
                bgmSource.clip = bgmClips[name];
                bgmSource.Play();
                currentBGMCoroutine = StartCoroutine(FadeInBGM(fadeDuration));
            }));
        }
    }

    public void PlaySfx(string name, Vector3 position, float blend = 1.0f)
    {
        if (sfxClips.ContainsKey(name))
        {
            //sfxSource.PlayOneShot(sfxClips[name]);
            // 효과음은 오디오 소스를 하나로 사용하면 안됨

            // 특정 위치에 3D 사운드를 재생
            //AudioSource.PlayClipAtPoint(sfxClips[name], position);

            GameObject gameObject = new GameObject("One shot audio");
            gameObject.transform.position = position;
            AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
            audioSource.clip = sfxClips[name];
            audioSource.spatialBlend = blend;
            audioSource.volume = 1.0f;
            audioSource.Play();
            UnityEngine.Object.Destroy(gameObject, sfxClips[name].length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp(volume, 0, 1);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp(volume, 0, 1);
    }

    private IEnumerator FadeOutBGM(float duration, Action onFadeComplete)
    {
        float startVolume = bgmSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        bgmSource.volume = 0;
        onFadeComplete?.Invoke(); // 페이드 아웃 되면 다음 작업 실행
    }

    private IEnumerator FadeInBGM(float duration)
    {
        float startVolume = 0;
        bgmSource.volume = 0f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 1f, t / duration);
            yield return null;
        }

        bgmSource.volume = 1.0f;
    }
}
