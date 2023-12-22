using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameNetMessage;

public class AIPlayer : Player
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


    /// <summary>
    /// 仅在单人游戏下生效，多人游戏时不需要此函数
    /// </summary>
    public override void DealATile()
    {
        if(GameClient.IsConnect ==false)//单人游戏
        {
            //为玩家发一张牌
            MahJongType mahJongType = MahJongTilesManager.Instance.DealATile(PlayerId);
            //构建胡牌数据
            WinOperationData winOperationData = new WinOperationData();
            winOperationData.mahJongType = mahJongType;
            //判断玩家是否胡牌（自摸）
            if (CheckWin(ref winOperationData, selfTouch: true))
            {
                //AI玩家胡牌
                //AIPlayerManager.Instance.Win(PlayerId);
                return;
            }
            //玩家进入出牌阶段
            PlayingTile();
            //随机生成手牌中的一个麻将
            MahJongType playMahJongType = MahJongTilesManager.Instance.mPlayerTiles[PlayerId].tiles[
                UnityEngine.Random.Range(0, MahJongTilesManager.Instance.mPlayerTiles[PlayerId].tiles.Count)
                ];
            //AI玩家打出牌[!重要代码]
            MahJongTilesManager.Instance.PlayTile(PlayerId, playMahJongType);
            //打出了牌
            GameManager.Instance.PlayerPlayingTile(PlayerId, playMahJongType);
            
        }

    }

    /// <summary>
    /// AI玩家可以吃牌了（仅在单人游戏下有用）
    /// </summary>
    /// <param name="eatTileOperation">吃牌操作</param>
    public override void EatTile(EatTileOperation eatTileOperation)
    {
        //TODO:AI玩家吃牌逻辑，这里可以写AI玩家的吃牌策略，现在默认不吃牌

        //回调
        EatTileManager.Instance.TotalNumberOfOperationsCurrentlyInProgress.Add(eatTileOperation);
    }










}
