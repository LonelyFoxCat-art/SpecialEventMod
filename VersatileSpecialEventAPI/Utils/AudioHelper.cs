using NAudio.Wave;
using Sound;
using UnityEngine;

namespace VersatileSpecialEventAPI.Utils;

/// <summary>
/// 音频资源管理与播放辅助类。
/// 负责 AudioClip 的内存缓存、文件动态加载解析以及统一的声音播放调度。
/// </summary>
public static class AudioHelper {
    private static Dictionary<string, AudioClip> AudioList = new Dictionary<string, AudioClip>();
    private static SoundEffectManager Instance = SingletonBehavior<SoundEffectManager>.Instance;

    /// <summary>
    /// 获取指定名称的音频剪辑。
    /// </summary>
    /// <param name="Name">音频的唯一标识名称。</param>
    /// <returns>对应的 AudioClip 实例。</returns>
    /// <exception cref="KeyNotFoundException">当指定的 Name 不存在于缓存中时抛出。</exception>
    public static AudioClip GetData(string Name) => AudioList[Name];

    /// <summary>
    /// 获取当前缓存的所有音频数据字典。
    /// </summary>
    /// <returns>包含所有已加载音频的字典引用。</returns>
    public static Dictionary<string, AudioClip> GetDataAll() => AudioList;

    /// <summary>
    /// 检查指定名称的音频是否已加载到缓存中。
    /// </summary>
    /// <param name="Name">音频的唯一标识名称。</param>
    /// <returns>若存在返回 true，否则返回 false。</returns>
    public static bool IsAudioExist(string Name) => AudioList.ContainsKey(Name);

    /// <summary>
    /// 将音频剪辑添加到缓存中。
    /// </summary>
    /// <param name="Name">音频的唯一标识名称。</param>
    /// <param name="Audio">要缓存的 AudioClip 实例。</param>
    public static void AddAudio(string Name, AudioClip Audio) {
        if (!IsAudioExist(Name)) AudioList[Name] = Audio;
    }

    /// <summary>
    /// 从本地文件系统读取音频文件，动态解析并转换为 AudioClip 后加入缓存。
    /// </summary>
    /// <param name="FilePath">音频文件的绝对或相对物理路径。</param>
    /// <param name="FileName">用于在缓存中注册该音频的唯一标识名称。</param>
    public static void AddAudioByFile(string FilePath, string FileName) {
        using var audioFile = new AudioFileReader(FilePath);

        var waveFormat = audioFile.WaveFormat;
        int sampleRate = waveFormat.SampleRate;
        int channels = waveFormat.Channels;
        int totalSamples = (int)(audioFile.Length / 4);

        float[] samples = new float[totalSamples];
        int readCount = audioFile.Read(samples, 0, totalSamples);

        if (readCount < totalSamples) {
            Array.Resize(ref samples, readCount);
            totalSamples = readCount;
        }

        int samplesPerChannel = totalSamples / channels;
        var audioClip = AudioClip.Create(FileName, samplesPerChannel, channels, sampleRate, false);
        audioClip.SetData(samples, 0);

        AddAudio(FileName, audioClip);
    }

    /// <summary>
    /// 播放指定名称的音频。
    /// </summary>
    /// <param name="audio">要播放的音频标识名称。</param>
    /// <param name="loop">是否循环播放，默认为 false。</param>
    /// <param name="volume">播放音量 (0.0 - 1.0)，默认为 1f。</param>
    /// <param name="parent">音频播放器的挂载父节点，用于控制 3D 空间音效，默认为 null。</param>
    /// <returns>返回生成的 SoundEffectPlayer 实例，可用于后续控制（如停止、淡出）。</returns>
    /// <exception cref="KeyNotFoundException">当指定的 audio 名称不存在于缓存中时抛出。</exception>
    public static SoundEffectPlayer? Play(string audio, bool loop = false, float volume = 1f, Transform? parent = null) => Instance.PlayClip(AudioList[audio], loop, volume, parent);
}