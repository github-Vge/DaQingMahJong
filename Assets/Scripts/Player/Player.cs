using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameNetMessage;
using static WinTileCheckData;


public class Player : IDisposable
{

    /// <summary>玩家Id</summary>
    public int PlayerId { get; protected set; }
    /// <summary>玩家的状态</summary>
    public PlayerState State { get; protected set; }
    /// <summary>玩家是否听牌</summary>
    public bool IsListening { get; protected set; }
    /// <summary>玩家的听牌数据，仅当IsListening为true时有效</summary>
    public ListeningTilesData ListeningTiles { get; set; }

    /// <summary>是否是东家</summary>
    public bool IsHost { get; protected set; }

    /// <summary>玩家的牌堆</summary>
    public MahJongTiles PlayerTiles;

    /// <summary>玩家的听牌数据</summary>
    public WinTileCheckData WinTileCheckData { get; set; }

    /// <summary>
    /// 初始化玩家
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="host">是否是东家</param>
    public virtual void InitPlayer(int playerId, bool host)
    {
        //记录玩家Id
        PlayerId = playerId;
        //记录东家信息
        IsHost = host;
        //获取牌堆数据
        PlayerTiles = MahJongTilesManager.Instance.mPlayerTiles[playerId];
        //注册事件
        EventHandler.DealATileToPlayerEvent += OnDealATileToPlayerEvent;
        EventHandler.PlayerPlayATileEvent += OnPlayerPlayATileEvent;
    }

    /// <summary>
    /// 为玩家发一张牌（单人游戏）
    /// </summary>
    public virtual void DealATile()
    {
        //为玩家发一张牌
        MahJongType mahJongType = MahJongTilesManager.Instance.DealATile(PlayerId);
        //构建胡牌数据
        WinOperationData winOperationData = new WinOperationData();
        winOperationData.mahJongType = mahJongType;
        //判断玩家是否胡牌（自摸）
        if (CheckWin(ref winOperationData, selfTouch: true))
        {
            //玩家胡牌操作
            if (PlayerId != 1)
            {
                //AI玩家胡牌
                Debug.Log($"AI玩家{PlayerId}赢了！");
            }
            else
            {
                Debug.Log("自摸了");
                //选择胡牌阶段
                State = PlayerState.ChooseWhetherToWin;
                //玩家胡牌操作
                WinOperation(winOperationData);
            }
            return;
        }
        //玩家进入出牌阶段
        PlayingTile();


        if (PlayerId != 1)
        {
            //随机生成手牌中的一个麻将
            MahJongType playMahJongType = MahJongTilesManager.Instance.mPlayerTiles[PlayerId].tiles[
                UnityEngine.Random.Range(0, MahJongTilesManager.Instance.mPlayerTiles[PlayerId].tiles.Count)
                ];
            //AI玩家打出牌[!重要代码]
            MahJongTilesManager.Instance.PlayTile(PlayerId, mahJongType);
            //打出了牌
            GameManager.Instance.PlayerPlayingTile(PlayerId, playMahJongType);
        }
    }

    /// <summary>
    /// 出牌
    /// </summary>
    /// <param name="mahJongType"></param>
    public virtual void PlayingTile()
    {
        //设置玩家状态
        State = PlayerState.Playing;
    }

    /// <summary>
    /// 玩家出了牌
    /// </summary>
    /// <param name="mahJongType"></param>
    public virtual void PlayedTile(MahJongType mahJongType)
    {
        //设置玩家状态
        State = PlayerState.Idle;
    }


    /// <summary>
    /// 玩家听牌操作
    /// </summary>
    public virtual void ListenOperation()
    {
        //设置状态
        State = PlayerState.ChooseWhetherToListen;
    }

    /// <summary>
    /// 游戏结束
    /// </summary>
    public virtual void GameOver()
    {
        //设置状态
        State = PlayerState.GameOver;
    }

    /// <summary>
    /// 玩家检查出牌
    /// </summary>
    /// <param name="mahJongType"></param>
    public virtual void EatTile(EatTileOperation eatTileOperation)
    {
        //设置玩家状态
        State = PlayerState.Eat;
        if (PlayerId == 1)//主玩家
        {
            //调起吃牌UI
            EatTileUI.Instance.OnHaveTileToEatEvent(eatTileOperation);
        }
        else
        {
            //AI玩家回调
            EatTileManager.Instance.TotalNumberOfOperationsCurrentlyInProgress.Add(eatTileOperation);
        }

    }

    /// <summary>
    /// 服务端发了一张牌给玩家
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    /// <param name="mahJongType">发的哪张牌</param>
    private void OnDealATileToPlayerEvent(int playerId, MahJongType mahJongType)
    {
        //不是当前AI玩家，则不执行操作
        if (PlayerId != playerId) return;

        //更新手牌数据
        MahJongTilesManager.Instance.mCurrentMahJongList.Remove(mahJongType);
        MahJongTilesManager.Instance.mPlayerTiles[PlayerId].tiles.Add(mahJongType);
        //显示手牌
        MahJongManager.Instance.DealedATile(PlayerId, mahJongType);
        //玩家进入出牌阶段
        PlayingTile();
    }

    /// <summary>
    /// 服务端中玩家打出了一张牌
    /// </summary>
    /// <param name="playerId">AI玩家的玩家Id</param>
    /// <param name="mahJongType">打的哪张牌</param>
    private void OnPlayerPlayATileEvent(int playerId, MahJongType mahJongType)
    {
        //不是当前玩家，则不执行操作
        if (PlayerId != playerId) return;

        //播放音效
        SoundManager.Instance.PlayTileSound(mahJongType);

        //玩家打出了牌
        PlayedTile(mahJongType);
        //获取麻将实例
        //GameObject mahJong = MahJongManager.Instance.mPlayerGameObjects[playerId].tiles.First(p => MahJongManager.Instance.GetMahJongType(p) == mahJongType);
        //玩家打出牌[!重要代码]
        MahJongTilesManager.Instance.PlayTile(PlayerId, mahJongType);
    }



    /// <summary>
    /// 玩家听牌
    /// </summary>
    public virtual void Listening(MahJongType mahJongType)
    {
        //听牌
        IsListening = true;

        //测试用
        //string str = "{";
        //WinTileCheckData.winTileItemList.ForEach(item => { str += item.mahJongType.ToString() + ","; });
        //str += "}";
        //Debug.Log($"玩家{PlayerId}听牌，胡牌为：{str}");

        //听牌音效
        SoundManager.Instance.PlayEatSound(SoundManager.EatSoundType.Listen);
        //给玩家发宝（宝牌）
        MahJongManager.Instance.ShowTreasure(PlayerId, mahJongType);

        if(GameClient.IsConnect == false)//单人游戏
        {
            //玩家打出了牌
            GameManager.Instance.PlayerPlayingTile(PlayerId, mahJongType);
        }

    }

    /// <summary>
    /// 检查玩家是否胡牌，TODO：这个函数可以不放到此类中
    /// </summary>
    /// <param name="mahJongType">打出或自摸的哪张牌</param>
    /// <param name="selfTouch">是否为自摸</param>
    public virtual bool CheckWin(ref WinOperationData winOperationData, bool selfTouch = false)
    {
        //没有胡牌数据，默认不胡
        if (ListeningTiles.eatMahJongTypeList == null) return false;

        //获取到要胡的牌
        MahJongType mahJongType = winOperationData.mahJongType;

        if (selfTouch == true)//如果是自摸
        {
            if (mahJongType == MahJongType.RedDragon//自摸到红中
            || mahJongType == MahJongTilesManager.Instance.Treasure)//或 自摸到宝牌
            {
                //自摸到红中
                winOperationData.winOperationType = mahJongType == MahJongType.RedDragon
                    ? WinOperationType.SelfTouch_RedDragon : WinOperationType.SelfTouch_Treasure;
                //胡牌需要的牌
                List<MahJongType> fitTiles = new List<MahJongType>();
                ListeningTiles.listenItemList.ForEach(p1 =>
                {
                    if (p1.listenType != ListenType.StrongWind)//不是刮大风，因为刮大风会有不是手牌的牌
                    {
                        if (p1.listenType == ListenType.TwoDoubleTile) p1.otherTiles.ForEach(p2 => fitTiles.Add(p2));
                        p1.listenTiles.ForEach(p2 => fitTiles.Add(p2));
                    }
                });
                winOperationData.fitTiles = fitTiles;
                //玩家胡了
                return true;
            }
            else if (winOperationData.winOperationType == WinOperationType.SelfTouch_StrongWind)//如果是刮大风
            {
                ListenItem listenItem = ListeningTiles.listenItemList.FirstOrDefault(p => p.listenType == ListenType.StrongWind && p.winTiles.Contains(mahJongType));
                if (listenItem.listenType != default) //刮大风了
                {
                    //自摸
                    winOperationData.winOperationType = WinOperationType.SelfTouch_StrongWind;

                    //胡牌需要的牌
                    winOperationData.fitTiles = listenItem.listenTiles;

                    Console.WriteLine($"玩家{PlayerId}胡了");
                    //玩家胡了
                    return true;
                }
            }
            else//自摸到胡牌
            {
                ListenItem listenItem = ListeningTiles.listenItemList.FirstOrDefault(p => p.listenType != ListenType.StrongWind && p.winTiles.Contains(mahJongType));
                if (listenItem.listenType != default) //有值 
                {
                    //自摸 或 不是（其他人打出的）
                    winOperationData.winOperationType = WinOperationType.SelfTouch_EatTile;

                    if (listenItem.listenType == ListenType.TwoDoubleTile)//胡 两个对
                    {
                        //胡牌需要的牌
                        winOperationData.fitTiles = listenItem.listenTiles.Contains(mahJongType) ?
                            listenItem.listenTiles : listenItem.otherTiles;
                    }
                    else
                    {
                        //胡牌需要的牌
                        winOperationData.fitTiles = listenItem.listenTiles;
                    }

                    Console.WriteLine($"玩家{PlayerId}胡了");
                    //玩家胡了
                    return true;
                }
            }
        }
        else//不是自摸
        {
            ListenItem listenItem = ListeningTiles.listenItemList.FirstOrDefault(p => p.listenType != ListenType.StrongWind && p.winTiles.Contains(mahJongType));
            if (listenItem.listenType != default) //有值 
            {
                //自摸 或 不是（其他人打出的）
                winOperationData.winOperationType = WinOperationType.OtherPlayed;

                if (listenItem.listenType == ListenType.TwoDoubleTile)//胡 两个对
                {
                    //胡牌需要的牌
                    winOperationData.fitTiles = listenItem.listenTiles.Contains(mahJongType) ?
                        listenItem.listenTiles : listenItem.otherTiles;
                }
                else
                {
                    //胡牌需要的牌
                    winOperationData.fitTiles = listenItem.listenTiles;
                }

                Console.WriteLine($"玩家{PlayerId}胡了");
                //玩家胡了
                return true;
            }
        }


        return false;


    }


    /// <summary>
    /// 玩家胡了
    /// </summary>
    public virtual void WinOperation(WinOperationData winOperationData)
    {
        //设置状态
        State = PlayerState.ChooseWhetherToWin;

        if (GameClient.IsConnect == false && PlayerId == 1)//单人游戏，并且是主玩家
        {
            Debug.Log("玩家胡牌操作");
            //玩家胡牌操作
            EatTileUI.Instance.WinOperation(winOperationData);
        }
        //Debug.Log($"玩家{PlayerId}胡了！");
    }

    /// <summary>
    /// 销毁对象
    /// </summary>
    public virtual void Dispose()
    {
        //注册事件
        EventHandler.DealATileToPlayerEvent -= OnDealATileToPlayerEvent;
        EventHandler.PlayerPlayATileEvent -= OnPlayerPlayATileEvent;
    }
}

public enum PlayerState
{
    None,
    /// <summary>出牌阶段</summary>
    Playing,
    /// <summary>闲置阶段（不能操作）</summary>
    Idle,
    /// <summary>吃牌阶段，正在判断是否吃牌</summary>
    Eat,
    /// <summary>准备听牌阶段</summary>
    ChooseWhetherToListen,
    /// <summary>选择是否胡牌阶段</summary>
    ChooseWhetherToWin,
    /// <summary>游戏结束阶段</summary>
    GameOver,

}


/// <summary>
/// 玩家听牌后的手牌数据
/// </summary>
public struct ListeningTilesData
{
    /// <summary>已经吃了的牌的列表</summary>
    public List<List<MahJongType>> eatMahJongTypeList;
    /// <summary>每种可能胡牌的方式</summary>
    public List<ListenItem> listenItemList;
}

public struct ListenItem
{
    /// <summary>听牌的方式，以哪种方式去胡</summary>
    public ListenType listenType;
    /// <summary>癞子牌</summary>
    public List<List<MahJongType>> wildMahJongTypeList;
    /// <summary>听的牌</summary>
    public List<MahJongType> listenTiles;
    /// <summary>除出听牌，其它的牌</summary>
    public List<MahJongType> otherTiles;
    /// <summary>赢哪些牌</summary>
    public List<MahJongType> winTiles;

}

/// <summary>
/// 听牌的方式
/// </summary>
public enum ListenType
{
    None,
    /// <summary>剩一张牌，也胡一张牌</summary>
    SingleTile,
    /// <summary>胡边</summary>
    SideWay,
    /// <summary>胡夹</summary>
    SandwichWay,
    /// <summary>胡两对</summary>
    TwoDoubleTile,
    /// <summary>胡一对（支对）</summary>
    OneDoubleTile,
    /// <summary>刮大风</summary>
    StrongWind,
}