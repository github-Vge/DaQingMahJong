using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameNetMessage;

/// <summary>
/// ����������ң����ڶ�����Ϸ�����ã�
/// </summary>
public class OtherPlayer : Player
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

    public override void DealATile()
    {
        //Ϊ��ҷ�һ����
        MahJongTilesManager.Instance.DealATile(PlayerId);
        //��ҽ�����ƽ׶�
        PlayingTile();
    }

    /// <summary>
    /// ��Ҵ������
    /// </summary>
    /// <param name="mahJongType"></param>
    public override void PlayedTile(MahJongType mahJongType)
    {
        base.PlayedTile(mahJongType);


    }


}
