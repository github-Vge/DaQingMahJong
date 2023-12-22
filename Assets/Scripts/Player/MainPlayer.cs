using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameNetMessage;

public class MainPlayer : Player
{
    /// <summary>单例</summary>
    public static MainPlayer Instance { get; private set; }

    /// <summary>
    /// 初始化玩家信息
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="host">是否为东家</param>
    public override void InitPlayer(int playerId, bool host)
    {
        base.InitPlayer(playerId, host);
        //初始化单例
        Instance = this;
    }

    /// <summary>
    /// 给玩家发了一张牌（仅在单人游戏下有效）
    /// </summary>
    public override void DealATile()
    {
        //为玩家发一张牌
        MahJongType mahJongType = MahJongTilesManager.Instance.DealATile(PlayerId);
        //构建胡牌数据
        WinOperationData winOperationData = new WinOperationData();
        winOperationData.mahJongType = mahJongType;
        //判断玩家是否胡牌（自摸）
        if (CheckWin(ref winOperationData, selfTouch: true))
        {
            Debug.Log("自摸！");
            //选择胡牌阶段
            State = PlayerState.ChooseWhetherToWin;
            //设置赢牌操作数据
            winOperationData.fromPlayerId = PlayerId;
            winOperationData.toPlayerId = PlayerId;
            //玩家胡牌操作
            WinOperation(winOperationData);

            return;
        }
        //玩家进入出牌阶段
        PlayingTile();
    }

    /// <summary>
    /// 玩家打出了牌
    /// </summary>
    /// <param name="mahJongType">打出的哪张牌</param>
    public override void PlayedTile(MahJongType mahJongType)
    {
        //设置玩家状态
        base.PlayedTile(mahJongType);
        //通知网络玩家打出了牌
        if(GameClient.IsConnect == true)//多人游戏
        {
            //发送消息：玩家打出了牌
            GameNetMessage.PlayerPlayedATile_Handler.SendMessage(mahJongType);
        }
        else//单人游戏
        {
            //玩家打出了牌
            GameManager.Instance.PlayerPlayingTile(PlayerId, mahJongType);
        }

    }

    /// <summary>
    /// 玩家需要选择吃牌操作
    /// </summary>
    /// <param name="eatTileOperation">吃牌操作</param>
    public override void EatTile(EatTileOperation eatTileOperation)
    {
        //调起吃牌UI
        EatTileUI.Instance.OnHaveTileToEatEvent(eatTileOperation);
    }




}
