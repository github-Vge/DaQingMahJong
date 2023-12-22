using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    /// <summary>单例</summary>
    public static SoundManager Instance { get; private set; }

    [Header("音频组件")]
    private AudioSource audioSource;


    /// <summary>打牌的声音字典</summary>
    private Dictionary<MahJongType, AudioClip> tileClipDict = new Dictionary<MahJongType, AudioClip>();
    /// <summary>吃牌相关的声音字典</summary>
    private Dictionary<EatSoundType, AudioClip> eatClipDict = new Dictionary<EatSoundType, AudioClip>();


    [Header("出牌音频")]
    [SerializeField]
    private AudioClip[] tileAudioClip;
    [Header("吃牌音频")]
    [SerializeField]
    private AudioClip[] eatAudioClip;



    private void Awake()
    {
        //初始化单例
        Instance = this;
        //初始化音频组件
        audioSource = GetComponent<AudioSource>();

        //记录音频数据
        for (int i = 1; i <= 28; i++)
        {
            tileClipDict[(MahJongType)i] = tileAudioClip[i];
        }
        for (int i = 0; i < 6; i++)
        {
            eatClipDict[(EatSoundType)i] = eatAudioClip[i];
        }

    }


    /// <summary>
    /// 播放出牌声音
    /// </summary>
    /// <param name="mahJongType">哪张牌</param>
    public void PlayTileSound(MahJongType mahJongType)
    {
        //获取声音
        audioSource.clip = tileClipDict[mahJongType];
        //播放声音
        audioSource.Play();
    }

    /// <summary>
    /// 播放吃牌相关声音
    /// </summary>
    /// <param name="eatSoundType">吃牌声音类型</param>
    public void PlayEatSound(EatSoundType eatSoundType)
    {
        //获取声音
        audioSource.clip = eatClipDict[eatSoundType];
        //播放声音
        audioSource.Play();
    }
    
    /// <summary>
    /// 根据 吃牌方式 获取 吃牌音效类型，一个简单的映射
    /// </summary>
    /// <param name="eatTileType">吃牌方式</param>
    /// <returns>吃牌音效类型</returns>
    public EatSoundType GetEatSoundTypeFromEatTileType(EatTileType eatTileType)
    {
        return eatTileType switch
        {
            EatTileType.LeftEat => SoundManager.EatSoundType.Eat,
            EatTileType.MiddleEat => SoundManager.EatSoundType.Eat,
            EatTileType.RightEat => SoundManager.EatSoundType.Eat,
            EatTileType.Touch => SoundManager.EatSoundType.Touch,
            EatTileType.Gang => SoundManager.EatSoundType.Gang,
            EatTileType.LeftEatAndListening => SoundManager.EatSoundType.Eat,
            EatTileType.MiddleEatAndListening => SoundManager.EatSoundType.Eat,
            EatTileType.RightEatAndListening => SoundManager.EatSoundType.Eat,
            EatTileType.TouchAndListening => SoundManager.EatSoundType.Touch,
            EatTileType.GangAndListening => SoundManager.EatSoundType.Gang,
            _ => SoundManager.EatSoundType.Eat,
        };
    }


    public enum EatSoundType
    {
        /// <summary>吃</summary>
        Eat,
        /// <summary>岔</summary>
        Touch,
        /// <summary>杠</summary>
        Gang,
        /// <summary>听</summary>
        Listen,
        /// <summary>胡</summary>
        Win,
        /// <summary>过</summary>
        DontEat,
    }

}

