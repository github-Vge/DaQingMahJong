using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    /// <summary>����</summary>
    public static SoundManager Instance { get; private set; }

    [Header("��Ƶ���")]
    private AudioSource audioSource;


    /// <summary>���Ƶ������ֵ�</summary>
    private Dictionary<MahJongType, AudioClip> tileClipDict = new Dictionary<MahJongType, AudioClip>();
    /// <summary>������ص������ֵ�</summary>
    private Dictionary<EatSoundType, AudioClip> eatClipDict = new Dictionary<EatSoundType, AudioClip>();


    [Header("������Ƶ")]
    [SerializeField]
    private AudioClip[] tileAudioClip;
    [Header("������Ƶ")]
    [SerializeField]
    private AudioClip[] eatAudioClip;



    private void Awake()
    {
        //��ʼ������
        Instance = this;
        //��ʼ����Ƶ���
        audioSource = GetComponent<AudioSource>();

        //��¼��Ƶ����
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
    /// ���ų�������
    /// </summary>
    /// <param name="mahJongType">������</param>
    public void PlayTileSound(MahJongType mahJongType)
    {
        //��ȡ����
        audioSource.clip = tileClipDict[mahJongType];
        //��������
        audioSource.Play();
    }

    /// <summary>
    /// ���ų����������
    /// </summary>
    /// <param name="eatSoundType">������������</param>
    public void PlayEatSound(EatSoundType eatSoundType)
    {
        //��ȡ����
        audioSource.clip = eatClipDict[eatSoundType];
        //��������
        audioSource.Play();
    }
    
    /// <summary>
    /// ���� ���Ʒ�ʽ ��ȡ ������Ч���ͣ�һ���򵥵�ӳ��
    /// </summary>
    /// <param name="eatTileType">���Ʒ�ʽ</param>
    /// <returns>������Ч����</returns>
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
        /// <summary>��</summary>
        Eat,
        /// <summary>��</summary>
        Touch,
        /// <summary>��</summary>
        Gang,
        /// <summary>��</summary>
        Listen,
        /// <summary>��</summary>
        Win,
        /// <summary>��</summary>
        DontEat,
    }

}

