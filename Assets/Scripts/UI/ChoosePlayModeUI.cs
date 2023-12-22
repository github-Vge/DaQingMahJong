using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePlayModeUI : MonoBehaviour
{
    /// <summary>单例</summary>
    public static ChoosePlayModeUI Instance { get; private set; }


    [Header("选择游戏模式面板（当前面板）")]
    public GameObject choosePlayModePanel;
    [Header("等待玩家面板")]
    public GameObject waitingAnotherPlayerPanel;
    [Header("单人游戏 按钮")]
    [SerializeField]
    private Button singlePlayerButton;
    [Header("多人游戏 按钮")]
    [SerializeField]
    private Button multiPlayerButton;



    private void Awake()
    {
        //初始化单例
        Instance = this;
        //绑定按钮事件
        singlePlayerButton.onClick.AddListener(OnSinglePlayerButtonClick);
        multiPlayerButton.onClick.AddListener(OnMultiPlayerButtonClick);

    }
    /// <summary>
    /// 单人游戏 按钮按下时调用
    /// </summary>
    private void OnSinglePlayerButtonClick()
    {
        //隐藏当前面板
        choosePlayModePanel.SetActive(false);
        //开始游戏
        GameManager.Instance.StartGame();
    }
    /// <summary>
    /// 多人游戏 按钮按下时调用
    /// </summary>
    private void OnMultiPlayerButtonClick()
    {
        //尝试连接服务端
        GameClient.Instance.Connect();
        //隐藏选择模式面板
        choosePlayModePanel.SetActive(false);
        //加入房间
        NetMessage.JoinRoom_Handler.SendMessage();
        //显示等待玩家面板
        waitingAnotherPlayerPanel.SetActive(true);
    }
}
