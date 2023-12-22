using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameNetMessage;

/// <summary>
/// 其他网络玩家（仅在多人游戏下有用）
/// </summary>
public class OtherPlayer : Player
{
    /// <summary>
    /// 初始化玩家信息
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="host">是否为东家</param>
    public override void InitPlayer(int playerId, bool host)
    {
        base.InitPlayer(playerId, host);
    }

    public override void DealATile()
    {
        //为玩家发一张牌
        MahJongTilesManager.Instance.DealATile(PlayerId);
        //玩家进入出牌阶段
        PlayingTile();
    }

    /// <summary>
    /// 玩家打出了牌
    /// </summary>
    /// <param name="mahJongType"></param>
    public override void PlayedTile(MahJongType mahJongType)
    {
        base.PlayedTile(mahJongType);


    }


}
