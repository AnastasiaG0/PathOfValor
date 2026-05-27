using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Музыка")]
    public AudioClip backgroundMusic;

    [Header("Звуковые эффекты")]
    public AudioClip victorySound;
    public AudioClip deathSound;
    public AudioClip winGameSound;
    public AudioClip loseGameSound;
    public AudioClip[] swordSounds;
    public AudioClip[] arrowSounds;
    public AudioClip[] fireSounds;
    public AudioClip[] throwSounds;
    public AudioClip[] heavySounds;
    public AudioClip[] enemyDeathSounds;
    public AudioClip buttonClickSound;

    private Dictionary<string, AudioClip> soundLibrary = new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeSounds();
    }

    void Start()
    {
        // Воспроизводим фоновую музыку
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    void InitializeSounds()
    {
        // Генерация звуков, если они не заданы

        if (victorySound == null)
            victorySound = SimpleSoundGenerator.GenerateBeepSound(880, 0.3f);

        if (deathSound == null)
            deathSound = SimpleSoundGenerator.GenerateBeepSound(220, 0.5f);

        if (winGameSound == null)
            winGameSound = SimpleSoundGenerator.GenerateBeepSound(523.25f, 1.5f); // Нота До

        if (loseGameSound == null)
            loseGameSound = SimpleSoundGenerator.GenerateNoiseSound(1f);

        if (swordSounds == null || swordSounds.Length == 0)
        {
            swordSounds = new AudioClip[] { SimpleSoundGenerator.GenerateSwordSound(0.2f) };
        }

        if (arrowSounds == null || arrowSounds.Length == 0)
        {
            arrowSounds = new AudioClip[] { SimpleSoundGenerator.GenerateBeepSound(1200, 0.1f) };
        }

        if (fireSounds == null || fireSounds.Length == 0)
        {
            fireSounds = new AudioClip[] { SimpleSoundGenerator.GenerateMagicSound(0.3f) };
        }

        if (throwSounds == null || throwSounds.Length == 0)
        {
            throwSounds = new AudioClip[] { SimpleSoundGenerator.GenerateBeepSound(600, 0.15f) };
        }

        if (heavySounds == null || heavySounds.Length == 0)
        {
            heavySounds = new AudioClip[] { SimpleSoundGenerator.GenerateExplosionSound(0.4f) };
        }

        if (enemyDeathSounds == null || enemyDeathSounds.Length == 0)
        {
            enemyDeathSounds = new AudioClip[] { SimpleSoundGenerator.GenerateBeepSound(300, 0.2f) };
        }

        if (buttonClickSound == null)
            buttonClickSound = SimpleSoundGenerator.GenerateBeepSound(800, 0.05f);

        // Заполняем библиотеку звуков для быстрого доступа
        soundLibrary["Victory"] = victorySound;
        soundLibrary["Death"] = deathSound;
        soundLibrary["Win"] = winGameSound;
        soundLibrary["Lose"] = loseGameSound;
        soundLibrary["Swing"] = swordSounds.Length > 0 ? swordSounds[0] : null;
        soundLibrary["Arrow"] = arrowSounds.Length > 0 ? arrowSounds[0] : null;
        soundLibrary["Fire"] = fireSounds.Length > 0 ? fireSounds[0] : null;
        soundLibrary["Throw"] = throwSounds.Length > 0 ? throwSounds[0] : null;
        soundLibrary["Heavy"] = heavySounds.Length > 0 ? heavySounds[0] : null;
        soundLibrary["EnemyDeath"] = enemyDeathSounds.Length > 0 ? enemyDeathSounds[0] : null;
        soundLibrary["ButtonClick"] = buttonClickSound;

        Debug.Log("Все звуки успешно инициализированы!");
    }

    public void PlaySound(string soundName)
    {
        if (soundLibrary.ContainsKey(soundName) && soundLibrary[soundName] != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(soundLibrary[soundName]);
        }
        else
        {
            Debug.LogWarning($"Звук '{soundName}' не найден!");
        }
    }

    public void PlayRandomSound(string soundGroup)
    {
        AudioClip clip = null;

        switch (soundGroup)
        {
            case "Sword":
                if (swordSounds != null && swordSounds.Length > 0)
                    clip = swordSounds[Random.Range(0, swordSounds.Length)];
                break;
            case "Arrow":
                if (arrowSounds != null && arrowSounds.Length > 0)
                    clip = arrowSounds[Random.Range(0, arrowSounds.Length)];
                break;
            case "Fire":
                if (fireSounds != null && fireSounds.Length > 0)
                    clip = fireSounds[Random.Range(0, fireSounds.Length)];
                break;
            case "Throw":
                if (throwSounds != null && throwSounds.Length > 0)
                    clip = throwSounds[Random.Range(0, throwSounds.Length)];
                break;
            case "Heavy":
                if (heavySounds != null && heavySounds.Length > 0)
                    clip = heavySounds[Random.Range(0, heavySounds.Length)];
                break;
            case "EnemyDeath":
                if (enemyDeathSounds != null && enemyDeathSounds.Length > 0)
                    clip = enemyDeathSounds[Random.Range(0, enemyDeathSounds.Length)];
                break;
        }

        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void PlayBackgroundMusic(AudioClip music)
    {
        if (musicSource != null && music != null)
        {
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
}