using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainUI : MonoBehaviour
{
    /// <summary>单例</summary>
    public static MainUI Instance;

    [Header("主面板")]
    public GameObject mainPanel;

    [Header("剩余牌数文本")]
    public TextMeshProUGUI mRemainingTileCountText;
    /// <summary>剩除牌数</summary>
    private int remainingTileCount = 112;
    /// <summary>剩余牌数量，与UI显示双向绑定</summary>
    [HideInInspector]
    public int RemainingTileCount
    {
        get { return remainingTileCount; }
        set { remainingTileCount = value; mRemainingTileCountText.text = $"剩余牌数：<b>{remainingTileCount}</b>"; }
    }

    private void Awake()
    {
        //初始化单例
        Instance = this;
    }






}
