using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    /// <summary>单例</summary>
    public static GameOverUI Instance { get; private set; }

    [Header("游戏结束面板")]
    public GameObject gameOverPanel;
    [Header("游戏结束显示的文字")]
    public TextMeshProUGUI gameOverText;


    [Header("再来一局 按钮")]
    [SerializeField]
    private Button regameButton;



    private void Awake()
    {
        //初始化单例
        Instance = this;
        //绑定按钮事件
        regameButton.onClick.AddListener(OnRegameButtonClick);
    }

    /// <summary>
    /// 调起游戏结束面板
    /// </summary>
    public void GameOver(GameNetMessage.GameOver gameOver)
    {
        if (gameOver.overType == GameNetMessage.OverType.NoTileLeft)//没有牌了
        {
            gameOverText.text = "没牌了\n游戏结束";
        }
        else//有玩家赢了
        {
            if (gameOver.winPlayerId == MainPlayer.Instance.PlayerId)//玩家是你自己
            {
                gameOverText.text = "你赢了！";
            }
            else//玩家不是你
            {
                gameOverText.text = "对方赢了\n游戏结束";
            }
        }

        //显示面板
        gameOverPanel.SetActive(true);
    }


    /// <summary>
    /// 再来一局按钮 按下时调用
    /// </summary>
    private void OnRegameButtonClick()
    {
        //隐藏当前面板
        gameOverPanel.SetActive(false);
        //显示等待玩家中面板
        ChoosePlayModeUI.Instance.waitingAnotherPlayerPanel.SetActive(true);
        if(GameClient.IsConnect == true)//多人游戏
        {
            //发送再来一局消息给服务端
            GameNetMessage.RestartGame_Handler.SendMessage();
        }
        else//单人游戏
        {
            GameManager.Instance.RestartGame();
        }

    }




}
