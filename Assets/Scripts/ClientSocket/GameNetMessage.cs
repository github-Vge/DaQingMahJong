using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNetMessage : NetMessageBase
{

    /// <summary>
    /// ʣ����������Ϣ
    /// �����->�ͻ��ˣ�����������ڵ�ʣ������
    /// </summary>
    public struct RemainingTileCount
    {
        /// <summary>ʣ������</summary>
        public int count;
    }

    public class RemainingTileCount_Handler : NetMessageHandlerBase<RemainingTileCount>
    {
        /// <summary>
        /// �յ�����˵���Ϣ
        /// </summary>
        /// <param name="netMessageBase"></param>
        public override void OnMessage(RemainingTileCount netMessageBase)
        {
            MainUI.Instance.RemainingTileCount = netMessageBase.count;
        }

    }


    /// <summary>
    /// ʣ����������Ϣ
    /// �����->�ͻ��ˣ�֪ͨ��ҳ�ʼ������
    /// </summary>
    public struct InitTiles
    {
        /// <summary>key:���Id��value:��ʼ����</summary>
        public Dictionary<int, List<MahJongType>> tiles;
    }

    public class InitTiles_Handler : NetMessageHandlerBase<InitTiles>
    {
        /// <summary>
        /// �յ�����˵���Ϣ
        /// <param name="netMessage"></param>
        /// </summary>
        public override void OnMessage(InitTiles netMessage)
        {
            //֪ͨ��Ϣ
            EventHandler.CallTilesInited(netMessage.tiles);
        }
    }


    /// <summary>
    /// ����һ���Ƶ���Ϣ
    /// �����->�ͻ��ˣ�֪ͨ��ң�����˸�ĳ����ҷ���һ����
    /// </summary>
    public struct DealATileToPlayer
    {
        /// <summary>���Id</summary>
        public int playerId;
        /// <summary>������</summary>
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
    /// �������Ϣ
    /// �����->�ͻ��ˣ�ĳ����Ҵ����һ����
    /// �ͻ���->����ˣ���ǰ��Ҵ���Ʋ���
    /// </summary>
    public struct PlayerPlayedATile
    {
        /// <summary>���Id</summary>
        public int playerId;
        /// <summary>�������</summary>
        public MahJongType mahJongType;
    }


    public class PlayerPlayedATile_Handler : NetMessageHandlerBase<PlayerPlayedATile>
    {
        public static void SendMessage(MahJongType mahJongType)
        {
            //������Ϣ
            PlayerPlayedATile playerPlayedATile = new PlayerPlayedATile();
            playerPlayedATile.playerId = MainPlayer.Instance.PlayerId;
            playerPlayedATile.mahJongType = mahJongType;
            //������Ϣ
            SendMessage(playerPlayedATile);
        }

        public override void OnMessage(PlayerPlayedATile netMessage)
        {
            EventHandler.CallPlayerPlayedATile(netMessage.playerId, netMessage.mahJongType);
        }
    }

    /// <summary>
    /// ���Ʋ�������Ϣ
    /// �����->�ͻ��ˣ���ǰ����п��ԳԵ����ˣ���Ҫ����
    /// �ͻ���->����ˣ�֪ͨ����ˣ���Ҫ���ƣ����߲����ƣ����ز������
    /// </summary>
    public struct PlayerHaveTileToEat
    {
        /// <summary>���Ʋ����������ص�</summary>
        public EatTileOperation eatTileOperation;
    }


    public class PlayerHaveTileToEat_Handler : NetMessageHandlerBase<PlayerHaveTileToEat>
    {
        public static void SendMessage(EatTileOperation eatTileOperation)
        {
            //������Ϣ
            PlayerHaveTileToEat playerHaveTileToEat = new PlayerHaveTileToEat();
            playerHaveTileToEat.eatTileOperation = eatTileOperation;
            //������Ϣ
            SendMessage(playerHaveTileToEat);

        }

        public override void OnMessage(PlayerHaveTileToEat netMessageBase)
        {
            //��ǰ��ҵĳ��Ʋ���
            MainPlayer.Instance.EatTile(netMessageBase.eatTileOperation);
        }

    }


    /// <summary>
    /// ��ҳ���һ����
    /// �����->�ͻ��ˣ�֪ͨһ�������ڵ���ң�����ҳ�����
    /// </summary>
    public struct PlayerAteATile
    {
        /// <summary>���Id</summary>
        public int playerId;
        /// <summary>���Ʋ���</summary>
        public EatTileOperation eatTileOperation;
    }

    public class PlayerAteATile_Handler : NetMessageHandlerBase<PlayerAteATile>
    {
        public override void OnMessage(PlayerAteATile netMessageBase)
        {
            //��Ӧ��ҳ��Ʋ���
            MahJongTilesManager.Instance.EatTile(netMessageBase.eatTileOperation);
            //��ҿ�ʼ����
            GameManager.Instance.Players[netMessageBase.playerId].PlayingTile();
        }
    }

    /// <summary>
    /// ��ҵ����Ʋ���
    /// �����->�ͻ��ˣ�֪ͨ��ҿ��Խ������Ʋ���
    /// �ͻ���->����ˣ���Ҵ������Ʋ���
    /// </summary>
    public struct PlayerListenOperation
    {
        /// <summary>�Ƿ�Ҫ����</summary>
        public bool whetherToListen;
        /// <summary>����������ƣ��ݴ棩</summary>
        public MahJongType mahJongType;
    }

    public class PlayerListenOperation_Handler : NetMessageHandlerBase<PlayerListenOperation>
    {
        public static void SendMessage(bool whetherToListen, MahJongType mahJongType)
        {
            //������Ϣ
            PlayerListenOperation playerListenOperation = new PlayerListenOperation();
            playerListenOperation.whetherToListen = whetherToListen;
            playerListenOperation.mahJongType = mahJongType;
            //������˷�����Ϣ
            SendMessage(playerListenOperation);
        }

        public override void OnMessage(PlayerListenOperation netMessage)
        {
            //�������״̬
            MainPlayer.Instance.ListenOperation();
            //������Ʋ���
            EatTileUI.Instance.ListeningOperation(netMessage.mahJongType);
        }
    }


    /// <summary>
    /// ����ұ���
    /// �����->�ͻ��ˣ�����ҷ�����
    /// </summary>
    public struct SendTreasure
    {
        /// <summary>���Id</summary>
        public int playerId;
        /// <summary>����</summary>
        public MahJongType mahJongType;
    }

    public class SendTreasure_Handler : NetMessageHandlerBase<SendTreasure>
    {
        public override void OnMessage(SendTreasure netMessage)
        {
            //��Ӧ�������
            GameManager.Instance.Players[netMessage.playerId].Listening(netMessage.mahJongType);
        }
    }


    /// <summary>
    /// ��Һ��Ʋ���
    /// �����->�ͻ��ˣ���ҿ���ʤ����
    /// �ͻ���->����ˣ��Ƿ����
    /// </summary>
    public struct WinOperation
    {
        /// <summary>���Ʋ�����Ҫ������</summary>
        public WinOperationData winOperationData;
        /// <summary>����Ƿ�ҪӮ�����ͻ��˴��ط��������</summary>
        public bool whetherToWin;
    }

    public struct WinOperationData
    {
        /// <summary>���ķ�ʽ</summary>
        public WinOperationType winOperationType;
        /// <summary>�ĸ���Ҵ������</summary>
        public int fromPlayerId;
        /// <summary>�ĸ���Һ���</summary>
        public int toPlayerId;
        /// <summary>����������</summary>
        public MahJongType mahJongType;
        /// <summary>�����õ�����</summary>
        public List<MahJongType> fitTiles;
    }

    public enum WinOperationType
    {
        None,
        /// <summary>�������δ�磨��ʱû���ϣ�</summary>
        SelfTouch_StrongWind,
        /// <summary>����������</summary>
        SelfTouch_RedDragon,
        /// <summary>����������</summary>
        SelfTouch_Treasure,
        /// <summary>��������������</summary>
        SelfTouch_EatTile,
        /// <summary>�����Ǳ��˴������</summary>
        OtherPlayed,

    }

    public class WinOperation_Handler : NetMessageHandlerBase<WinOperation>
    {
        public static void SendMessage(MahJongType mahJongType, bool whetherToWin)
        {
            //������Ϣ
            WinOperation winOperation = new WinOperation();
            winOperation.winOperationData.mahJongType = mahJongType;
            winOperation.whetherToWin = whetherToWin;
            //��ָ����ҷ�����Ϣ����ҪӮ��
            SendMessage(winOperation);
        }


        public override void OnMessage(WinOperation netMessage)
        {
            //�������״̬
            MainPlayer.Instance.WinOperation(netMessage.winOperationData);
            //��Һ��Ʋ���
            EatTileUI.Instance.WinOperation(netMessage.winOperationData);
        }


    }

    /// <summary>
    /// ��Ϸ������Ϣ
    /// �����->�ͻ��ˣ�֪ͨ�����ڵ������Ϸ����
    /// </summary>
    public struct GameOver
    {
        /// <summary>�����ķ�ʽ</summary>
        public OverType overType;
        /// <summary>Ӯ�����</summary>
        public int winPlayerId;
        /// <summary>����Ƶ����</summary>
        public int fromPlayerId;
        /// <summary>����������</summary>
        public MahJongType mahJongType;
    }

    public enum OverType
    {
        None, 
        /// <summary>û����</summary>
        NoTileLeft,
        /// <summary>����</summary>
        SelfTouch,
        /// <summary>���˴������</summary>
        OtherPlayed,
    }

    public class GameOver_Handler : NetMessageHandlerBase<GameOver>
    {
        public override void OnMessage(GameOver netMessage)
        {
            //��ǰ��Һ���
            if (netMessage.winPlayerId == MainPlayer.Instance.PlayerId)
            {
                //������Ч
                SoundManager.Instance.PlayEatSound(SoundManager.EatSoundType.Win);
            }

            //��Ϸ����
            GameManager.Instance.GameOver(netMessage);
        }
    }

    /// <summary>
    /// ���¿�ʼ��Ϸ ��Ϣ
    /// �ͻ���->����ˣ������Ҫ�ٿ�һ��
    /// �����->�ͻ��ˣ�֪ͨ�����ڵ���ң����¿�ʼ��Ϸ
    /// </summary>
    public struct RestartGame
    {

    }

    public class RestartGame_Handler:NetMessageHandlerBase<RestartGame>
    {
        public static void SendMessage()
        {
            //������Ϣ
            RestartGame restartGame = new RestartGame();
            //������Ϣ
            SendMessage(restartGame);
        }

        public override void OnMessage(RestartGame netMessageBase)
        {
            //���صȴ���������
            ChoosePlayModeUI.Instance.waitingAnotherPlayerPanel.SetActive(false);
            //���¿�ʼ��Ϸ
            GameManager.Instance.RestartMultiPlayerGame();
        }

    }


}
