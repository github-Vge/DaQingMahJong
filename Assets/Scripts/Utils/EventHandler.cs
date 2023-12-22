using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 设计模式之观察者模式，很好用的设计模式
/// </summary>
public static class EventHandler
{

    /// <summary>
    /// 当一个玩家有可以吃的牌时，通知该玩家
    /// </summary>
    /// <param name="fromPlayerId">打出牌的玩家</param>
    /// <param name="toPlayerId">吃牌的玩家</param>
    /// <param name="mahJongType">吃的哪张牌</param>
    /// <param name="eatTileType">吃牌方式</param>
    /// <param name="eatTilePriority">吃牌优先级，1最高，最优先</param>
    /// <returns>玩家是否吃了牌</returns>
    public static void CallHaveTileToEat(EatTileOperation eatTileOperation)
    {
        //玩家吃牌
        GameManager.Instance.Players[eatTileOperation.toPlayerId].EatTile(eatTileOperation);
    }


    public static event Action<Dictionary<int, List<MahJongType>>> TilesInitedEvent;
    /// <summary>
    /// 所有玩家的牌初始化了（发牌了）
    /// </summary>
    /// <param name="tiles">初始手牌</param>
    public static void CallTilesInited(Dictionary<int, List<MahJongType>> tiles)
    {
        TilesInitedEvent?.Invoke(tiles);
    }


    public static event Action<int, MahJongType> DealATileToPlayerEvent;
    /// <summary>
    /// 给AI玩家发了牌
    /// </summary>
    /// <param name="playerId">AI玩家的玩家Id</param>
    /// <param name="mahJongType">发的哪张牌</param>
    public static void CallDealATileToPlayer(int playerId, MahJongType mahJongType)
    {
        DealATileToPlayerEvent?.Invoke(playerId, mahJongType);
    }

    public static event Action<int, MahJongType> PlayerPlayATileEvent;
    /// <summary>
    /// AI玩家打出了一张牌
    /// </summary>
    /// <param name="playerId">AI玩家的玩家Id</param>
    /// <param name="mahJongType">打的哪张牌</param>
    public static void CallPlayerPlayedATile(int playerId, MahJongType mahJongType)
    {
        PlayerPlayATileEvent?.Invoke(playerId, mahJongType);
    }


}
