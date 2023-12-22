using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���ģʽ֮�۲���ģʽ���ܺ��õ����ģʽ
/// </summary>
public static class EventHandler
{

    /// <summary>
    /// ��һ������п��ԳԵ���ʱ��֪ͨ�����
    /// </summary>
    /// <param name="fromPlayerId">����Ƶ����</param>
    /// <param name="toPlayerId">���Ƶ����</param>
    /// <param name="mahJongType">�Ե�������</param>
    /// <param name="eatTileType">���Ʒ�ʽ</param>
    /// <param name="eatTilePriority">�������ȼ���1��ߣ�������</param>
    /// <returns>����Ƿ������</returns>
    public static void CallHaveTileToEat(EatTileOperation eatTileOperation)
    {
        //��ҳ���
        GameManager.Instance.Players[eatTileOperation.toPlayerId].EatTile(eatTileOperation);
    }


    public static event Action<Dictionary<int, List<MahJongType>>> TilesInitedEvent;
    /// <summary>
    /// ������ҵ��Ƴ�ʼ���ˣ������ˣ�
    /// </summary>
    /// <param name="tiles">��ʼ����</param>
    public static void CallTilesInited(Dictionary<int, List<MahJongType>> tiles)
    {
        TilesInitedEvent?.Invoke(tiles);
    }


    public static event Action<int, MahJongType> DealATileToPlayerEvent;
    /// <summary>
    /// ��AI��ҷ�����
    /// </summary>
    /// <param name="playerId">AI��ҵ����Id</param>
    /// <param name="mahJongType">����������</param>
    public static void CallDealATileToPlayer(int playerId, MahJongType mahJongType)
    {
        DealATileToPlayerEvent?.Invoke(playerId, mahJongType);
    }

    public static event Action<int, MahJongType> PlayerPlayATileEvent;
    /// <summary>
    /// AI��Ҵ����һ����
    /// </summary>
    /// <param name="playerId">AI��ҵ����Id</param>
    /// <param name="mahJongType">���������</param>
    public static void CallPlayerPlayedATile(int playerId, MahJongType mahJongType)
    {
        PlayerPlayATileEvent?.Invoke(playerId, mahJongType);
    }


}
