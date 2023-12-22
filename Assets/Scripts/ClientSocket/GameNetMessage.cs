using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNetMessage : NetMessageBase
{

    /// <summary>
    /// 剩余牌数的消息
    /// 服务端->客户端：告诉玩家现在的剩余牌数
    /// </summary>
    public struct RemainingTileCount
    {
        /// <summary>剩余牌数</summary>
        public int count;
    }

    public class RemainingTileCount_Handler : NetMessageHandlerBase<RemainingTileCount>
    {
        /// <summary>
        /// 收到服务端的消息
        /// </summary>
        /// <param name="netMessageBase"></param>
        public override void OnMessage(RemainingTileCount netMessageBase)
        {
            MainUI.Instance.RemainingTileCount = netMessageBase.count;
        }

    }


    /// <summary>
    /// 剩余牌数的消息
    /// 服务端->客户端：通知玩家初始的手牌
    /// </summary>
    public struct InitTiles
    {
        /// <summary>key:玩家Id，value:初始手牌</summary>
        public Dictionary<int, List<MahJongType>> tiles;
    }

    public class InitTiles_Handler : NetMessageHandlerBase<InitTiles>
    {
        /// <summary>
        /// 收到服务端的消息
        /// <param name="netMessage"></param>
        /// </summary>
        public override void OnMessage(InitTiles netMessage)
        {
            //通知消息
            EventHandler.CallTilesInited(netMessage.tiles);
        }
    }


    /// <summary>
    /// 发了一张牌的消息
    /// 服务端->客户端：通知玩家，服务端给某个玩家发了一张牌
    /// </summary>
    public struct DealATileToPlayer
    {
        /// <summary>玩家Id</summary>
        public int playerId;
        /// <summary>发的牌</summary>
        public MahJongType mahJongType;
    }

    public class DealATileToPlayer_Handler : NetMessageHandlerBase<DealATileToPlayer>
    {
        public override void OnMessage(DealATileToPlayer netMessageBase)
        {
            EventHandler.CallDealATileToPlayer(netMessageBase.playerId, netMessageBase.mahJongType);
        }
    }

    /// <summary>
    /// 打出牌消息
    /// 服务端->客户端：某个玩家打出了一张牌
    /// 客户端->服务端：当前玩家打出牌操作
    /// </summary>
    public struct PlayerPlayedATile
    {
        /// <summary>玩家Id</summary>
        public int playerId;
        /// <summary>打出的牌</summary>
        public MahJongType mahJongType;
    }


    public class PlayerPlayedATile_Handler : NetMessageHandlerBase<PlayerPlayedATile>
    {
        public static void SendMessage(MahJongType mahJongType)
        {
            //构造消息
            PlayerPlayedATile playerPlayedATile = new PlayerPlayedATile();
            playerPlayedATile.playerId = MainPlayer.Instance.PlayerId;
            playerPlayedATile.mahJongType = mahJongType;
            //发送消息
            SendMessage(playerPlayedATile);
        }

        public override void OnMessage(PlayerPlayedATile netMessage)
        {
            EventHandler.CallPlayerPlayedATile(netMessage.playerId, netMessage.mahJongType);
        }
    }

    /// <summary>
    /// 吃牌操作的消息
    /// 服务端->客户端：当前玩家有可以吃的牌了，你要吃吗？
    /// 客户端->服务端：通知服务端，我要吃牌，过者不吃牌，传回操作结果
    /// </summary>
    public struct PlayerHaveTileToEat
    {
        /// <summary>吃牌操作，包含回调</summary>
        public EatTileOperation eatTileOperation;
    }


    public class PlayerHaveTileToEat_Handler : NetMessageHandlerBase<PlayerHaveTileToEat>
    {
        public static void SendMessage(EatTileOperation eatTileOperation)
        {
            //构造消息
            PlayerHaveTileToEat playerHaveTileToEat = new PlayerHaveTileToEat();
            playerHaveTileToEat.eatTileOperation = eatTileOperation;
            //发送消息
            SendMessage(playerHaveTileToEat);

        }

        public override void OnMessage(PlayerHaveTileToEat netMessageBase)
        {
            //当前玩家的吃牌操作
            MainPlayer.Instance.EatTile(netMessageBase.eatTileOperation);
        }

    }


    /// <summary>
    /// 玩家吃了一张牌
    /// 服务端->客户端：通知一个房间内的玩家，有玩家吃牌了
    /// </summary>
    public struct PlayerAteATile
    {
        /// <summary>玩家Id</summary>
        public int playerId;
        /// <summary>吃牌操作</summary>
        public EatTileOperation eatTileOperation;
    }

    public class PlayerAteATile_Handler : NetMessageHandlerBase<PlayerAteATile>
    {
        public override void OnMessage(PlayerAteATile netMessageBase)
        {
            //对应玩家吃牌操作
            MahJongTilesManager.Instance.EatTile(netMessageBase.eatTileOperation);
            //玩家开始出牌
            GameManager.Instance.Players[netMessageBase.playerId].PlayingTile();
        }
    }

    /// <summary>
    /// 玩家的听牌操作
    /// 服务端->客户端：通知玩家可以进行听牌操作
    /// 客户端->服务端：玩家传回听牌操作
    /// </summary>
    public struct PlayerListenOperation
    {
        /// <summary>是否要听牌</summary>
        public bool whetherToListen;
        /// <summary>打出的哪张牌（暂存）</summary>
        public MahJongType mahJongType;
    }

    public class PlayerListenOperation_Handler : NetMessageHandlerBase<PlayerListenOperation>
    {
        public static void SendMessage(bool whetherToListen, MahJongType mahJongType)
        {
            //构造消息
            PlayerListenOperation playerListenOperation = new PlayerListenOperation();
            playerListenOperation.whetherToListen = whetherToListen;
            playerListenOperation.mahJongType = mahJongType;
            //给服务端发送消息
            SendMessage(playerListenOperation);
        }

        public override void OnMessage(PlayerListenOperation netMessage)
        {
            //设置玩家状态
            MainPlayer.Instance.ListenOperation();
            //玩家听牌操作
            EatTileUI.Instance.ListeningOperation(netMessage.mahJongType);
        }
    }


    /// <summary>
    /// 给玩家宝牌
    /// 服务端->客户端：给玩家发宝牌
    /// </summary>
    public struct SendTreasure
    {
        /// <summary>玩家Id</summary>
        public int playerId;
        /// <summary>宝牌</summary>
        public MahJongType mahJongType;
    }

    public class SendTreasure_Handler : NetMessageHandlerBase<SendTreasure>
    {
        public override void OnMessage(SendTreasure netMessage)
        {
            //对应玩家听牌
            GameManager.Instance.Players[netMessage.playerId].Listening(netMessage.mahJongType);
        }
    }


    /// <summary>
    /// 玩家胡牌操作
    /// 服务端->客户端：玩家可以胜利了
    /// 客户端->服务端：是否胡牌
    /// </summary>
    public struct WinOperation
    {
        /// <summary>胡牌操作需要的数据</summary>
        public WinOperationData winOperationData;
        /// <summary>玩家是否要赢，仅客户端传回服务端有用</summary>
        public bool whetherToWin;
    }

    public struct WinOperationData
    {
        /// <summary>胡的方式</summary>
        public WinOperationType winOperationType;
        /// <summary>哪个玩家打出的牌</summary>
        public int fromPlayerId;
        /// <summary>哪个玩家胡牌</summary>
        public int toPlayerId;
        /// <summary>胡的哪张牌</summary>
        public MahJongType mahJongType;
        /// <summary>胡牌用到的牌</summary>
        public List<MahJongType> fitTiles;
    }

    public enum WinOperationType
    {
        None,
        /// <summary>自摸，刮大风（暂时没用上）</summary>
        SelfTouch_StrongWind,
        /// <summary>自摸到红中</summary>
        SelfTouch_RedDragon,
        /// <summary>自摸到宝牌</summary>
        SelfTouch_Treasure,
        /// <summary>正常自摸到手牌</summary>
        SelfTouch_EatTile,
        /// <summary>胡的是别人打出的牌</summary>
        OtherPlayed,

    }

    public class WinOperation_Handler : NetMessageHandlerBase<WinOperation>
    {
        public static void SendMessage(MahJongType mahJongType, bool whetherToWin)
        {
            //构造消息
            WinOperation winOperation = new WinOperation();
            winOperation.winOperationData.mahJongType = mahJongType;
            winOperation.whetherToWin = whetherToWin;
            //给指定玩家发送消息：你要赢吗？
            SendMessage(winOperation);
        }


        public override void OnMessage(WinOperation netMessage)
        {
            //设置玩家状态
            MainPlayer.Instance.WinOperation(netMessage.winOperationData);
            //玩家胡牌操作
            EatTileUI.Instance.WinOperation(netMessage.winOperationData);
        }


    }

    /// <summary>
    /// 游戏结束消息
    /// 服务端->客户端：通知房间内的玩家游戏结束
    /// </summary>
    public struct GameOver
    {
        /// <summary>结束的方式</summary>
        public OverType overType;
        /// <summary>赢的玩家</summary>
        public int winPlayerId;
        /// <summary>打出牌的玩家</summary>
        public int fromPlayerId;
        /// <summary>胡的哪张牌</summary>
        public MahJongType mahJongType;
    }

    public enum OverType
    {
        None, 
        /// <summary>没牌了</summary>
        NoTileLeft,
        /// <summary>自摸</summary>
        SelfTouch,
        /// <summary>别人打出的牌</summary>
        OtherPlayed,
    }

    public class GameOver_Handler : NetMessageHandlerBase<GameOver>
    {
        public override void OnMessage(GameOver netMessage)
        {
            //当前玩家胡牌
            if (netMessage.winPlayerId == MainPlayer.Instance.PlayerId)
            {
                //胡牌音效
                SoundManager.Instance.PlayEatSound(SoundManager.EatSoundType.Win);
            }

            //游戏结束
            GameManager.Instance.GameOver(netMessage);
        }
    }

    /// <summary>
    /// 重新开始游戏 消息
    /// 客户端->服务端：玩家想要再开一局
    /// 服务端->客户端：通知房间内的玩家，重新开始游戏
    /// </summary>
    public struct RestartGame
    {

    }

    public class RestartGame_Handler:NetMessageHandlerBase<RestartGame>
    {
        public static void SendMessage()
        {
            //构造消息
            RestartGame restartGame = new RestartGame();
            //发送消息
            SendMessage(restartGame);
        }

        public override void OnMessage(RestartGame netMessageBase)
        {
            //隐藏等待玩家中面板
            ChoosePlayModeUI.Instance.waitingAnotherPlayerPanel.SetActive(false);
            //重新开始游戏
            GameManager.Instance.RestartMultiPlayerGame();
        }

    }


}
