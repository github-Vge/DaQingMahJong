using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameNetMessage;

/// <summary>
/// 游戏主要类，非常重要的类
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>单例</summary>
    public static GameManager Instance { get; private set; }

    [Header("主场景预制体")]
    public GameObject mainScenePrefab;

    /// <summary>东家默认是第1个玩家</summary>
    public const int hostId = 1;

    /// <summary>四个玩家的实例</summary>
    public Dictionary<int, Player> Players { get; set; }

    /// <summary>主场景实例</summary>
    private GameObject mainScene;

    private void Awake()
    {
        //初始化单例
        Instance = this;
    }

    private void Start()
    {
        //实例化一个新的主场景
        mainScene = Instantiate(mainScenePrefab);
    }

    private void OnEnable()
    {
        //注册事件
        EventHandler.TilesInitedEvent += OnTilesInitedEvent;
    }

    private void OnDisable()
    {
        //取消注册事件
        EventHandler.TilesInitedEvent -= OnTilesInitedEvent;
    }

    /// <summary>
    /// 开始单人游戏
    /// </summary>
    public void StartGame()
    {
        //玩家字典
        Players = new Dictionary<int, Player>();
        //新建4个玩家实例
        {
            Player player = new MainPlayer();
            Players.Add(1, player);
            player.InitPlayer(1, true);
        }
        for (int i = 1; i < 4; i++)
        {
            Player player = new AIPlayer();
            Players.Add(i + 1, player);
            player.InitPlayer(i + 1, false);
        }
        //为每个玩家发牌
        for (int i = 1; i <= 4; i++)
        {
            MahJongTilesManager.Instance.DealTiles(i, 13);
        }
        //为东家再发一张牌
        MahJongTilesManager.Instance.DealTiles(hostId, 1);
        //显示手牌
        MahJongManager.Instance.MahJongTilesInited();
        //东家出牌
        Players[hostId].PlayingTile();
        //移动摄像机
        CameraManager.Instance.MoveCameraToPlayer(1);
    }


    /// <summary>
    /// 开始多人游戏
    /// </summary>
    /// <param name="playerId">当前玩家的Id</param>
    public void StartMultiPlayerGame(int playerId)
    {
        //隐藏选择游戏模式面板
        ChoosePlayModeUI.Instance.choosePlayModePanel.SetActive(false);
        //玩家字典
        Players = new Dictionary<int, Player>();
        //新建4个玩家实例
        if (playerId == 1)
        {
            //1
            Player player = new MainPlayer();
            Players.Add(1, player);
            player.InitPlayer(1, true);
            //2
            player = new AIPlayer();
            Players.Add(2, player);
            player.InitPlayer(2, false);
            //3
            player = new OtherPlayer();
            Players.Add(3, player);
            player.InitPlayer(3, false);
            //4
            player = new AIPlayer();
            Players.Add(4, player);
            player.InitPlayer(4, false);
        }
        else if (playerId == 3)
        {
            //1
            Player player = new OtherPlayer();
            Players.Add(1, player);
            player.InitPlayer(1, true);
            //2
            player = new AIPlayer();
            Players.Add(2, player);
            player.InitPlayer(2, false);
            //3
            player = new MainPlayer();
            Players.Add(3, player);
            player.InitPlayer(3, false);
            //4
            player = new AIPlayer();
            Players.Add(4, player);
            player.InitPlayer(4, false);
        }
        //移动摄像机
        CameraManager.Instance.MoveCameraToPlayer(playerId);
    }



    private void OnTilesInitedEvent(Dictionary<int, List<MahJongType>> tiles)
    {
        //为每个玩家发牌
        for (int i = 1; i <= 4; i++)
        {
            //给玩家发牌
            Players[i].PlayerTiles.tiles = tiles[i];
            //从牌池中移除
            tiles[i].ForEach(a => MahJongTilesManager.Instance.mCurrentMahJongList.Remove(a));
        }
        //显示手牌
        MahJongManager.Instance.MahJongTilesInited();
        //东家出牌
        Players[hostId].PlayingTile();

    }

    /// <summary>
    /// 玩家打出了牌（主要用于胡牌和吃牌判断）
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    public void PlayerPlayingTile(int playerId, MahJongType mahJongType)
    {
        //胡牌判断
        foreach (Player player in Players.Values)
        {
            //新建胡牌操作数据
            WinOperationData winOperationData = new WinOperationData();
            winOperationData.mahJongType = mahJongType;
            if (player.PlayerId != playerId && player.CheckWin(ref winOperationData) == true)
            {
                //设置赢牌操作数据
                winOperationData.fromPlayerId = playerId;
                winOperationData.toPlayerId = player.PlayerId;
                //其他玩家胡牌
                player.WinOperation(winOperationData);
                return;
            }
        }
        //吃牌判断。如果有其他玩家吃牌，则跳转到其他玩家的吃牌回合
        if (EatTileManager.Instance.CheckEat(playerId, mahJongType) == true)
        {
            //其他玩家吃牌回合
            return;
        }

        //玩家打出了牌
        PlayerPlayedTile(playerId, mahJongType);
    }

    /// <summary>
    /// 玩家打出了牌
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    public void PlayerPlayedTile(int playerId, MahJongType mahJongType)
    {
        //播放语音
        SoundManager.Instance.PlayTileSound(mahJongType);
        //下一个玩家行动
        int nextPlayerId = playerId % 4 + 1;
        //给玩家发一张牌
        Players[nextPlayerId].DealATile();

    }

    /// <summary>
    /// 没牌了，游戏结束
    /// </summary>
    /// <param name="gameOver">0代表没牌了，1-4代表有赢家，数字代表赢家Id</param>
    public void GameOver(GameNetMessage.GameOver gameOver)
    {
        //游戏结束，不能行动
        Players.Values.ToList().ForEach(a => a.GameOver());
        //移动摄像机到顶部
        CameraManager.Instance.MoveToTop();
        //隐藏剩余牌数UI
        MainUI.Instance.mainPanel.SetActive(false);
        if(gameOver.overType != GameNetMessage.OverType.NoTileLeft)
        {
            //展示宝牌
            MahJongManager.Instance.LayTreasure(gameOver.winPlayerId);
            //展示胡牌
            MahJongManager.Instance.ShowWin(gameOver);
        }

        //调起游戏结束UI
        GameOverUI.Instance.GameOver(gameOver);

        print($"游戏结束，赢家：{gameOver.winPlayerId}");
    }


    /// <summary>
    /// 重新开始一个新游戏（单人游戏）
    /// </summary>
    public void RestartGame()
    {
        //销毁主场景
        Destroy(mainScene);
        //销毁所有玩家
        Players.Values.ToList().ForEach(action => { action.Dispose(); });
        //实例化一个新的主场景
        mainScene = Instantiate(mainScenePrefab);
        //隐藏选择游戏模式面板
        ChoosePlayModeUI.Instance.choosePlayModePanel.SetActive(false);
        //开始单人游戏
        StartGame();
    }

    /// <summary>
    /// 重新开始一个新游戏（多人游戏）
    /// </summary>
    public void RestartMultiPlayerGame()
    {
        //销毁主场景
        Destroy(mainScene);
        //销毁所有玩家
        Players.Values.ToList().ForEach(action => { action.Dispose(); });
        //实例化一个新的主场景
        mainScene = Instantiate(mainScenePrefab);
        //开始多人游戏
        StartMultiPlayerGame(MainPlayer.Instance.PlayerId);
    }



}
