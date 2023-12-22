using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameNetMessage;

public class MainPlayer : Player
{
    /// <summary>����</summary>
    public static MainPlayer Instance { get; private set; }

    /// <summary>
    /// ��ʼ�������Ϣ
    /// </summary>
    /// <param name="playerId">���ID</param>
    /// <param name="host">�Ƿ�Ϊ����</param>
    public override void InitPlayer(int playerId, bool host)
    {
        base.InitPlayer(playerId, host);
        //��ʼ������
        Instance = this;
    }

    /// <summary>
    /// ����ҷ���һ���ƣ����ڵ�����Ϸ����Ч��
    /// </summary>
    public override void DealATile()
    {
        //Ϊ��ҷ�һ����
        MahJongType mahJongType = MahJongTilesManager.Instance.DealATile(PlayerId);
        //������������
        WinOperationData winOperationData = new WinOperationData();
        winOperationData.mahJongType = mahJongType;
        //�ж�����Ƿ���ƣ�������
        if (CheckWin(ref winOperationData, selfTouch: true))
        {
            Debug.Log("������");
            //ѡ����ƽ׶�
            State = PlayerState.ChooseWhetherToWin;
            //����Ӯ�Ʋ�������
            winOperationData.fromPlayerId = PlayerId;
            winOperationData.toPlayerId = PlayerId;
            //��Һ��Ʋ���
            WinOperation(winOperationData);

            return;
        }
        //��ҽ�����ƽ׶�
        PlayingTile();
    }

    /// <summary>
    /// ��Ҵ������
    /// </summary>
    /// <param name="mahJongType">�����������</param>
    public override void PlayedTile(MahJongType mahJongType)
    {
        //�������״̬
        base.PlayedTile(mahJongType);
        //֪ͨ������Ҵ������
        if(GameClient.IsConnect == true)//������Ϸ
        {
            //������Ϣ����Ҵ������
            GameNetMessage.PlayerPlayedATile_Handler.SendMessage(mahJongType);
        }
        else//������Ϸ
        {
            //��Ҵ������
            GameManager.Instance.PlayerPlayingTile(PlayerId, mahJongType);
        }

    }

    /// <summary>
    /// �����Ҫѡ����Ʋ���
    /// </summary>
    /// <param name="eatTileOperation">���Ʋ���</param>
    public override void EatTile(EatTileOperation eatTileOperation)
    {
        //�������UI
        EatTileUI.Instance.OnHaveTileToEatEvent(eatTileOperation);
    }




}
