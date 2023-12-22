using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameNetMessage;

public class AIPlayer : Player
{
    /// <summary>
    /// ��ʼ�������Ϣ
    /// </summary>
    /// <param name="playerId">���ID</param>
    /// <param name="host">�Ƿ�Ϊ����</param>
    public override void InitPlayer(int playerId, bool host)
    {
        base.InitPlayer(playerId, host);
    }


    /// <summary>
    /// ���ڵ�����Ϸ����Ч��������Ϸʱ����Ҫ�˺���
    /// </summary>
    public override void DealATile()
    {
        if(GameClient.IsConnect ==false)//������Ϸ
        {
            //Ϊ��ҷ�һ����
            MahJongType mahJongType = MahJongTilesManager.Instance.DealATile(PlayerId);
            //������������
            WinOperationData winOperationData = new WinOperationData();
            winOperationData.mahJongType = mahJongType;
            //�ж�����Ƿ���ƣ�������
            if (CheckWin(ref winOperationData, selfTouch: true))
            {
                //AI��Һ���
                //AIPlayerManager.Instance.Win(PlayerId);
                return;
            }
            //��ҽ�����ƽ׶�
            PlayingTile();
            //������������е�һ���齫
            MahJongType playMahJongType = MahJongTilesManager.Instance.mPlayerTiles[PlayerId].tiles[
                UnityEngine.Random.Range(0, MahJongTilesManager.Instance.mPlayerTiles[PlayerId].tiles.Count)
                ];
            //AI��Ҵ����[!��Ҫ����]
            MahJongTilesManager.Instance.PlayTile(PlayerId, playMahJongType);
            //�������
            GameManager.Instance.PlayerPlayingTile(PlayerId, playMahJongType);
            
        }

    }

    /// <summary>
    /// AI��ҿ��Գ����ˣ����ڵ�����Ϸ�����ã�
    /// </summary>
    /// <param name="eatTileOperation">���Ʋ���</param>
    public override void EatTile(EatTileOperation eatTileOperation)
    {
        //TODO:AI��ҳ����߼����������дAI��ҵĳ��Ʋ��ԣ�����Ĭ�ϲ�����

        //�ص�
        EatTileManager.Instance.TotalNumberOfOperationsCurrentlyInProgress.Add(eatTileOperation);
    }










}
