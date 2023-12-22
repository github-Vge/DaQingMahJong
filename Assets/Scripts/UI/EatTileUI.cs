using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EatTileUI : MonoBehaviour
{
    /// <summary>单例</summary>
    public static EatTileUI Instance;

    [Header("吃牌操作面板")]
    public GameObject eatTilePanel;

    [Header("吃牌按钮预制体")]
    public GameObject eatButtonPrefab;


    /// <summary>当前的所有吃牌按钮</summary>
    private List<GameObject> eatButtonList = new List<GameObject>();


    private void Awake()
    {
        //初始化单例
        Instance = this;
    }

    /// <summary>
    /// 有主玩家有牌可以吃时调用，一次吃牌回合会被多次调用
    /// </summary>
    /// <param name="eatTileOperation"></param>
    public void OnHaveTileToEatEvent(EatTileOperation eatTileOperation)
    {
        //显示吃牌面板
        eatTilePanel.SetActive(true);
        if (eatButtonList.Count == 0)//这个函数被第一次调用
        {
            //实例化“过”按钮
            GameObject dontEatButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
            //更改按钮文本为“过”
            dontEatButton.GetComponentInChildren<TextMeshProUGUI>().text = "过";
            //绑定按钮事件
            dontEatButton.GetComponent<Button>().onClick.AddListener(() => {
                //复制吃牌操作
                EatTileOperation op = eatTileOperation;
                OnEatButtonClick(op, false);
            });
            //添加到列表中
            eatButtonList.Add(dontEatButton);
        }
        //吃牌按钮实例
        {
            //实例化一个按钮
            GameObject eatButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
            //设置按钮偏移
            eatButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f * eatButtonList.Count, 0f);
            //更改按钮文本
            eatButton.GetComponentInChildren<TextMeshProUGUI>().text = eatTileOperation.eatTileType switch
            {
                EatTileType.LeftEatAndListening => "吃听",
                EatTileType.MiddleEatAndListening => "吃听",
                EatTileType.RightEatAndListening => "吃听",
                EatTileType.TouchAndListening => "岔听",
                EatTileType.GangAndListening => "杠听",

                EatTileType.LeftEat => "吃",
                EatTileType.MiddleEat => "吃",
                EatTileType.RightEat => "吃",
                EatTileType.Touch => "岔",
                EatTileType.Gang => "杠",

                _ => "过",
            };

            //绑定按钮事件
            eatButton.GetComponent<Button>().onClick.AddListener(() => {             
                //复制吃牌操作
                EatTileOperation op = eatTileOperation;
                OnEatButtonClick(op, true);
                });
            //设置相应高亮的牌
            List<GameObject> highlightGameObjects = new List<GameObject>();
            foreach (MahJongType mahJongType in eatTileOperation.fitEatMahJongTypeList)
            {
                highlightGameObjects.Add(MahJongManager.Instance.mPlayerGameObjects[eatTileOperation.toPlayerId].tiles.First(p => 
                    mahJongType == MahJongManager.Instance.GetMahJongType(p) && !highlightGameObjects.Contains(p)
                ));
            }
            highlightGameObjects.Add(MahJongManager.Instance.mPlayerGameObjects[eatTileOperation.fromPlayerId].playedTiles.Last());
            //注册需要高亮的牌
            eatButton.GetComponent<EatButtonHighlight>().RegisterHighlightGameObject(highlightGameObjects);
            //添加到列表中
            eatButtonList.Add(eatButton);
        }



    }


    /// <summary>
    /// 吃牌面板的任意按钮 按下时调用
    /// </summary>
    /// <param name="eatTileOperation">吃牌的操作</param>
    /// <param name="whetherToEat">是否吃牌（是否点击了“过”按钮）</param>
    private void OnEatButtonClick(EatTileOperation eatTileOperation, bool whetherToEat)
    {
        //剩余操作回调（其它操作不吃牌）
        for (int i = 0; i < eatButtonList.Count - 2; i++)
        {
            //随便一个操作数据
            EatTileOperation op = new EatTileOperation();
            //优先级滞后
            op.eatTilePriority = 100;
            //其它操作回调
            if (GameClient.IsConnect == false)//单人游戏
            {
                EatTileManager.Instance.TotalNumberOfOperationsCurrentlyInProgress.Add(op);
            }
            else//多人游戏
            {
                //向服务端回调消息（不操作消息）
                GameNetMessage.PlayerHaveTileToEat_Handler.SendMessage(op);
            }
        }

        //销毁所有吃牌按钮
        eatButtonList.ForEach(a => Destroy(a));
        //清空列表
        eatButtonList.Clear();
        //隐藏吃牌面板
        eatTilePanel.SetActive(false);

        //吃牌操作
        eatTileOperation.whetherToEat = whetherToEat;
        if (GameClient.IsConnect == false)//单人游戏
        {
            //回调操作
            EatTileManager.Instance.TotalNumberOfOperationsCurrentlyInProgress.Add(eatTileOperation);
            //如果是杠，则为玩家再发一张牌
            if (eatTileOperation.eatTileType == EatTileType.Gang)
            {
                GameManager.Instance.Players[1].DealATile();
            }
        }
        else//多人游戏
        {
            //向服务端回调消息
            GameNetMessage.PlayerHaveTileToEat_Handler.SendMessage(eatTileOperation);
        }
        //玩家吃了牌了，正在选择出的牌
        GameManager.Instance.Players[1].PlayingTile();


    }

    /// <summary>
    /// 玩家听牌操作（玩家决定是否听牌）
    /// <param name="mahJongType">暂存要打出的牌</param>
    /// </summary>
    public void ListeningOperation(MahJongType mahJongType)
    {
        //显示吃牌面板
        eatTilePanel.SetActive(true);

        //实例化“听”按钮
        GameObject listeningButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
        //更改按钮文本为“听”
        listeningButton.GetComponentInChildren<TextMeshProUGUI>().text = "听";
        //设置按钮位置
        listeningButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f, 0f);
        //绑定按钮事件
        listeningButton.GetComponent<Button>().onClick.AddListener(() => OnListeningButtonClick(mahJongType, true));
        //添加到列表中
        eatButtonList.Add(listeningButton);

        //实例化“不听”按钮
        GameObject dontListeningButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
        //更改按钮文本为“不听”
        dontListeningButton.GetComponentInChildren<TextMeshProUGUI>().text = "不听";
        //绑定按钮事件
        dontListeningButton.GetComponent<Button>().onClick.AddListener(() => OnListeningButtonClick(mahJongType, false));
        //添加到列表中
        eatButtonList.Add(dontListeningButton);
    }

    /// <summary>
    /// 听牌面板的任意按钮 按下时调用
    /// </summary>
    /// <param name="eatTileOperation">吃牌的操作</param>
    /// <param name="whetherToEat">是否吃牌（是否点击了“过”按钮）</param>
    private void OnListeningButtonClick(MahJongType mahJongType, bool whetherToListening)
    {
        //销毁所有吃牌按钮
        eatButtonList.ForEach(a => Destroy(a));
        //清空列表
        eatButtonList.Clear();
        //隐藏吃牌面板
        eatTilePanel.SetActive(false);

        if(GameClient.IsConnect == false)//单人游戏
        {
            //回调操作
            if (whetherToListening == true)//听牌
            {
                GameManager.Instance.Players[MainPlayer.Instance.PlayerId].Listening(mahJongType);
            }
            else//不听牌
            {
                GameManager.Instance.PlayerPlayingTile(MainPlayer.Instance.PlayerId, mahJongType);
            }
        }
        else//多人游戏
        {
            //向服务端传回操作
            GameNetMessage.PlayerListenOperation_Handler.SendMessage(whetherToListening, mahJongType);
        }

    }

    /// <summary>
    /// 玩家胡牌操作（玩家决定是否胡牌）
    /// </summary>
    public void WinOperation(GameNetMessage.WinOperationData winOperationData)
    {
        //显示吃牌面板
        eatTilePanel.SetActive(true);

        //实例化“胡”按钮
        GameObject winButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
        //更改按钮文本为“胡”
        winButton.GetComponentInChildren<TextMeshProUGUI>().text = "胡";
        //设置按钮位置
        winButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f, 0f);
        //绑定按钮事件
        winButton.GetComponent<Button>().onClick.AddListener(() => OnWinButtonClick(winOperationData, true));
        //设置相应高亮的牌
        List<GameObject> highlightGameObjects = new List<GameObject>();
        if(winOperationData.winOperationType == GameNetMessage.WinOperationType.SelfTouch_StrongWind)//刮大风时，高亮的牌可能在吃牌中
        {
            foreach (MahJongType mahJongType in winOperationData.fitTiles)
            {
                highlightGameObjects.Add(MahJongManager.Instance.mPlayerGameObjects[winOperationData.toPlayerId].tiles
                    .Concat(MahJongManager.Instance.mPlayerGameObjects[winOperationData.toPlayerId].playedTiles)//可能不对，可以有4张牌符合winOperationData.fitTiles
                    .First(p =>
                    mahJongType == MahJongManager.Instance.GetMahJongType(p) && !highlightGameObjects.Contains(p)
                ));
            }
        }
        else//正常胡牌
        {
            foreach (MahJongType mahJongType in winOperationData.fitTiles)
            {
                highlightGameObjects.Add(MahJongManager.Instance.mPlayerGameObjects[winOperationData.toPlayerId].tiles.First(p =>
                    mahJongType == MahJongManager.Instance.GetMahJongType(p) && !highlightGameObjects.Contains(p)
                ));
            }
        }

        if(winOperationData.winOperationType == GameNetMessage.WinOperationType.OtherPlayed)//不是自摸
        {
            highlightGameObjects.Add(MahJongManager.Instance
                .mPlayerGameObjects[winOperationData.fromPlayerId].playedTiles.Last());
        }
        else//是自摸
        {
            highlightGameObjects.Add(MahJongManager.Instance
                .mPlayerGameObjects[winOperationData.fromPlayerId].tiles.Last());
        }
        winButton.GetComponent<EatButtonHighlight>().RegisterHighlightGameObject(highlightGameObjects);
        //添加到列表中
        eatButtonList.Add(winButton);

        //实例化“不胡”按钮
        GameObject dontWinButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
        //更改按钮文本为“不胡”
        dontWinButton.GetComponentInChildren<TextMeshProUGUI>().text = "不胡";
        //绑定按钮事件
        dontWinButton.GetComponent<Button>().onClick.AddListener(() => OnWinButtonClick(winOperationData, false));
        //添加到列表中
        eatButtonList.Add(dontWinButton);
    }


    /// <summary>
    /// 吃牌面板的任意按钮 按下时调用
    /// </summary>
    /// <param name="eatTileOperation">吃牌的操作</param>
    /// <param name="whetherToEat">是否吃牌（是否点击了“过”按钮）</param>
    private void OnWinButtonClick(GameNetMessage.WinOperationData winOperationData, bool whetherToWin)
    {
        //销毁所有吃牌按钮
        eatButtonList.ForEach(a => Destroy(a));
        //清空列表
        eatButtonList.Clear();
        //隐藏吃牌面板
        eatTilePanel.SetActive(false);


        if(GameClient.IsConnect == false)//单人游戏
        {
            //回调操作
            if (whetherToWin == true)//胡牌
            {
                //胡牌音效
                SoundManager.Instance.PlayEatSound(SoundManager.EatSoundType.Win);
                //游戏结束
                GameManager.Instance.GameOver(new GameNetMessage.GameOver
                {
                    overType = winOperationData.winOperationType switch
                    {
                        GameNetMessage.WinOperationType.SelfTouch_RedDragon => GameNetMessage.OverType.SelfTouch,
                        GameNetMessage.WinOperationType.SelfTouch_Treasure => GameNetMessage.OverType.SelfTouch,
                        GameNetMessage.WinOperationType.SelfTouch_EatTile => GameNetMessage.OverType.SelfTouch,
                        GameNetMessage.WinOperationType.OtherPlayed => GameNetMessage.OverType.OtherPlayed,
                        _ => GameNetMessage.OverType.None,
                    },
                    fromPlayerId = winOperationData.fromPlayerId,
                    winPlayerId = winOperationData.toPlayerId,
                    mahJongType = winOperationData.mahJongType,
                });
            }
            else//不胡牌
            {
                MainPlayer.Instance.PlayingTile();
                //TODO:
            }
        }
        else//多人游戏
        {
            //回调操作，向服务端传回消息
            GameNetMessage.WinOperation_Handler.SendMessage(winOperationData.mahJongType, true);
            
        }

    }




}
