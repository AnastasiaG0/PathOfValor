using UnityEngine;

/// <summary>
/// Генератор звуковых эффектов без внешних файлов
/// </summary>
public class SimpleSoundGenerator : MonoBehaviour
{
    /// <summary>
    /// Генерирует простой звуковой сигнал (бип)
    /// </summary>
    /// <param name="frequency">Частота звука (Гц). 440 = нота Ля</param>
    /// <param name="duration">Длительность звука (секунды)</param>
    /// <returns>Сгенерированный AudioClip</returns>
    public static AudioClip GenerateBeepSound(float frequency = 440, float duration = 0.5f)
    {
        // Частота дискретизации (качество звука)
        int sampleRate = 44100;

        // Количество сэмплов = частота * длительность
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);

        // Создаём аудиоклип
        AudioClip clip = AudioClip.Create("GeneratedSound_" + frequency, sampleCount, 1, sampleRate, false);

        // Массив для хранения звуковых данных
        float[] samples = new float[sampleCount];

        // Заполняем массив синусоидой
        for (int i = 0; i < sampleCount; i++)
        {
            // Синусоидальная волна: sin(2π * частота * время)
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);

            // Плавное затухание в конце (чтобы не было щелчка)
            if (i > sampleCount * 0.8f)
            {
                float t = (i - sampleCount * 0.8f) / (sampleCount * 0.2f);
                samples[i] *= 1 - t;
            }
        }

        // Применяем данные к клипу
        clip.SetData(samples, 0);

        return clip;
    }

    /// <summary>
    /// Генерирует шумовой звук (для поражения, взрывов и т.д.)
    /// </summary>
    /// <param name="duration">Длительность звука (секунды)</param>
    /// <returns>Сгенерированный AudioClip</returns>
    public static AudioClip GenerateNoiseSound(float duration = 0.3f)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("NoiseSound", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            // Белый шум (случайные значения)
            samples[i] = Random.Range(-0.5f, 0.5f);

            // Плавное затухание
            if (i > sampleCount * 0.7f)
            {
                float t = (i - sampleCount * 0.7f) / (sampleCount * 0.3f);
                samples[i] *= 1 - t;
            }
        }

        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Генерирует звук меча (свист)
    /// </summary>
    public static AudioClip GenerateSwordSound(float duration = 0.2f)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("SwordSound", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            // Свист с повышением частоты
            float t = (float)i / sampleCount;
            float frequency = 400 + t * 600;
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);
            // Быстрое затухание
            samples[i] *= 1 - t * 2;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Генерирует взрывной звук (бас + шум)
    /// </summary>
    public static AudioClip GenerateExplosionSound(float duration = 0.5f)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("ExplosionSound", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            // Низкая частота + шум
            float lowFreq = Mathf.Sin(2 * Mathf.PI * 80 * i / sampleRate) * (1 - t);
            float noise = Random.Range(-0.3f, 0.3f) * (1 - t);
            samples[i] = lowFreq + noise;
            // Нормализация
            samples[i] = Mathf.Clamp(samples[i], -0.5f, 0.5f);
        }

        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Генерирует магический звук (для огня, заклинаний)
    /// </summary>
    public static AudioClip GenerateMagicSound(float duration = 0.4f)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("MagicSound", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            // Вибрато (быстрое изменение частоты)
            float frequency = 600 + Mathf.Sin(t * Mathf.PI * 20) * 100;
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);
            samples[i] *= 1 - t;
        }

        clip.SetData(samples, 0);
        return clip;
    }
}