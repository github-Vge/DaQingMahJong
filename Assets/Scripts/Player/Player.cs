using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameNetMessage;
using static WinTileCheckData;


public class Player : IDisposable
{

    /// <summary>���Id</summary>
    public int PlayerId { get; protected set; }
    /// <summary>��ҵ�״̬</summary>
    public PlayerState State { get; protected set; }
    /// <summary>����Ƿ�����</summary>
    public bool IsListening { get; protected set; }
    /// <summary>��ҵ��������ݣ�����IsListeningΪtrueʱ��Ч</summary>
    public ListeningTilesData ListeningTiles { get; set; }

    /// <summary>�Ƿ��Ƕ���</summary>
    public bool IsHost { get; protected set; }

    /// <summary>��ҵ��ƶ�</summary>
    public MahJongTiles PlayerTiles;

    /// <summary>��ҵ���������</summary>
    public WinTileCheckData WinTileCheckData { get; set; }

    /// <summary>
    /// ��ʼ�����
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="host">�Ƿ��Ƕ���</param>
    public virtual void InitPlayer(int playerId, bool host)
    {
        //��¼���Id
        PlayerId = playerId;
        //��¼������Ϣ
        IsHost = host;
        //��ȡ�ƶ�����
        PlayerTiles = MahJongTilesManager.Instance.mPlayerTiles[playerId];
        //ע���¼�
        EventHandler.DealATileToPlayerEvent += OnDealATileToPlayerEvent;
        EventHandler.PlayerPlayATileEvent += OnPlayerPlayATileEvent;
    }

    /// <summary>
    /// Ϊ��ҷ�һ���ƣ�������Ϸ��
    /// </summary>
    public virtual void DealATile()
    {
        //Ϊ��ҷ�һ����
        MahJongType mahJongType = MahJongTilesManager.Instance.DealATile(PlayerId);
        //������������
        WinOperationData winOperationData = new WinOperationData();
        winOperationData.mahJongType = mahJongType;
        //�ж�����Ƿ���ƣ�������
        if (CheckWin(ref winOperationData, selfTouch: true))
        {
            //��Һ��Ʋ���
            if (PlayerId != 1)
            {
                //AI��Һ���
                Debug.Log($"AI���{PlayerId}Ӯ�ˣ�");
            }
            else
            {
                Debug.Log("������");
                //ѡ����ƽ׶�
                State = PlayerState.ChooseWhetherToWin;
                //��Һ��Ʋ���
                WinOperation(winOperationData);
            }
            return;
        }
        //��ҽ�����ƽ׶�
        PlayingTile();


        if (PlayerId != 1)
        {
            //������������е�һ���齫
            MahJongType playMahJongType = MahJongTilesManager.Instance.mPlayerTiles[PlayerId].tiles[
                UnityEngine.Random.Range(0, MahJongTilesManager.Instance.mPlayerTiles[PlayerId].tiles.Count)
                ];
            //AI��Ҵ����[!��Ҫ����]
            MahJongTilesManager.Instance.PlayTile(PlayerId, mahJongType);
            //�������
            GameManager.Instance.PlayerPlayingTile(PlayerId, playMahJongType);
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="mahJongType"></param>
    public virtual void PlayingTile()
    {
        //�������״̬
        State = PlayerState.Playing;
    }

    /// <summary>
    /// ��ҳ�����
    /// </summary>
    /// <param name="mahJongType"></param>
    public virtual void PlayedTile(MahJongType mahJongType)
    {
        //�������״̬
        State = PlayerState.Idle;
    }


    /// <summary>
    /// ������Ʋ���
    /// </summary>
    public virtual void ListenOperation()
    {
        //����״̬
        State = PlayerState.ChooseWhetherToListen;
    }

    /// <summary>
    /// ��Ϸ����
    /// </summary>
    public virtual void GameOver()
    {
        //����״̬
        State = PlayerState.GameOver;
    }

    /// <summary>
    /// ��Ҽ�����
    /// </summary>
    /// <param name="mahJongType"></param>
    public virtual void EatTile(EatTileOperation eatTileOperation)
    {
        //�������״̬
        State = PlayerState.Eat;
        if (PlayerId == 1)//�����
        {
            //�������UI
            EatTileUI.Instance.OnHaveTileToEatEvent(eatTileOperation);
        }
        else
        {
            //AI��һص�
            EatTileManager.Instance.TotalNumberOfOperationsCurrentlyInProgress.Add(eatTileOperation);
        }

    }

    /// <summary>
    /// ����˷���һ���Ƹ����
    /// </summary>
    /// <param name="playerId">���Id</param>
    /// <param name="mahJongType">����������</param>
    private void OnDealATileToPlayerEvent(int playerId, MahJongType mahJongType)
    {
        //���ǵ�ǰAI��ң���ִ�в���
        if (PlayerId != playerId) return;

        //������������
        MahJongTilesManager.Instance.mCurrentMahJongList.Remove(mahJongType);
        MahJongTilesManager.Instance.mPlayerTiles[PlayerId].tiles.Add(mahJongType);
        //��ʾ����
        MahJongManager.Instance.DealedATile(PlayerId, mahJongType);
        //��ҽ�����ƽ׶�
        PlayingTile();
    }

    /// <summary>
    /// ���������Ҵ����һ����
    /// </summary>
    /// <param name="playerId">AI��ҵ����Id</param>
    /// <param name="mahJongType">���������</param>
    private void OnPlayerPlayATileEvent(int playerId, MahJongType mahJongType)
    {
        //���ǵ�ǰ��ң���ִ�в���
        if (PlayerId != playerId) return;

        //������Ч
        SoundManager.Instance.PlayTileSound(mahJongType);

        //��Ҵ������
        PlayedTile(mahJongType);
        //��ȡ�齫ʵ��
        //GameObject mahJong = MahJongManager.Instance.mPlayerGameObjects[playerId].tiles.First(p => MahJongManager.Instance.GetMahJongType(p) == mahJongType);
        //��Ҵ����[!��Ҫ����]
        MahJongTilesManager.Instance.PlayTile(PlayerId, mahJongType);
    }



    /// <summary>
    /// �������
    /// </summary>
    public virtual void Listening(MahJongType mahJongType)
    {
        //����
        IsListening = true;

        //������
        //string str = "{";
        //WinTileCheckData.winTileItemList.ForEach(item => { str += item.mahJongType.ToString() + ","; });
        //str += "}";
        //Debug.Log($"���{PlayerId}���ƣ�����Ϊ��{str}");

        //������Ч
        SoundManager.Instance.PlayEatSound(SoundManager.EatSoundType.Listen);
        //����ҷ��������ƣ�
        MahJongManager.Instance.ShowTreasure(PlayerId, mahJongType);

        if(GameClient.IsConnect == false)//������Ϸ
        {
            //��Ҵ������
            GameManager.Instance.PlayerPlayingTile(PlayerId, mahJongType);
        }

    }

    /// <summary>
    /// �������Ƿ���ƣ�TODO������������Բ��ŵ�������
    /// </summary>
    /// <param name="mahJongType">�����������������</param>
    /// <param name="selfTouch">�Ƿ�Ϊ����</param>
    public virtual bool CheckWin(ref WinOperationData winOperationData, bool selfTouch = false)
    {
        //û�к������ݣ�Ĭ�ϲ���
        if (ListeningTiles.eatMahJongTypeList == null) return false;

        //��ȡ��Ҫ������
        MahJongType mahJongType = winOperationData.mahJongType;

        if (selfTouch == true)//���������
        {
            if (mahJongType == MahJongType.RedDragon//����������
            || mahJongType == MahJongTilesManager.Instance.Treasure)//�� ����������
            {
                //����������
                winOperationData.winOperationType = mahJongType == MahJongType.RedDragon
                    ? WinOperationType.SelfTouch_RedDragon : WinOperationType.SelfTouch_Treasure;
                //������Ҫ����
                List<MahJongType> fitTiles = new List<MahJongType>();
                ListeningTiles.listenItemList.ForEach(p1 =>
                {
                    if (p1.listenType != ListenType.StrongWind)//���ǹδ�磬��Ϊ�δ����в������Ƶ���
                    {
                        if (p1.listenType == ListenType.TwoDoubleTile) p1.otherTiles.ForEach(p2 => fitTiles.Add(p2));
                        p1.listenTiles.ForEach(p2 => fitTiles.Add(p2));
                    }
                });
                winOperationData.fitTiles = fitTiles;
                //��Һ���
                return true;
            }
            else if (winOperationData.winOperationType == WinOperationType.SelfTouch_StrongWind)//����ǹδ��
            {
                ListenItem listenItem = ListeningTiles.listenItemList.FirstOrDefault(p => p.listenType == ListenType.StrongWind && p.winTiles.Contains(mahJongType));
                if (listenItem.listenType != default) //�δ����
                {
                    //����
                    winOperationData.winOperationType = WinOperationType.SelfTouch_StrongWind;

                    //������Ҫ����
                    winOperationData.fitTiles = listenItem.listenTiles;

                    Console.WriteLine($"���{PlayerId}����");
                    //��Һ���
                    return true;
                }
            }
            else//����������
            {
                ListenItem listenItem = ListeningTiles.listenItemList.FirstOrDefault(p => p.listenType != ListenType.StrongWind && p.winTiles.Contains(mahJongType));
                if (listenItem.listenType != default) //��ֵ 
                {
                    //���� �� ���ǣ������˴���ģ�
                    winOperationData.winOperationType = WinOperationType.SelfTouch_EatTile;

                    if (listenItem.listenType == ListenType.TwoDoubleTile)//�� ������
                    {
                        //������Ҫ����
                        winOperationData.fitTiles = listenItem.listenTiles.Contains(mahJongType) ?
                            listenItem.listenTiles : listenItem.otherTiles;
                    }
                    else
                    {
                        //������Ҫ����
                        winOperationData.fitTiles = listenItem.listenTiles;
                    }

                    Console.WriteLine($"���{PlayerId}����");
                    //��Һ���
                    return true;
                }
            }
        }
        else//��������
        {
            ListenItem listenItem = ListeningTiles.listenItemList.FirstOrDefault(p => p.listenType != ListenType.StrongWind && p.winTiles.Contains(mahJongType));
            if (listenItem.listenType != default) //��ֵ 
            {
                //���� �� ���ǣ������˴���ģ�
                winOperationData.winOperationType = WinOperationType.OtherPlayed;

                if (listenItem.listenType == ListenType.TwoDoubleTile)//�� ������
                {
                    //������Ҫ����
                    winOperationData.fitTiles = listenItem.listenTiles.Contains(mahJongType) ?
                        listenItem.listenTiles : listenItem.otherTiles;
                }
                else
                {
                    //������Ҫ����
                    winOperationData.fitTiles = listenItem.listenTiles;
                }

                Console.WriteLine($"���{PlayerId}����");
                //��Һ���
                return true;
            }
        }


        return false;


    }


    /// <summary>
    /// ��Һ���
    /// </summary>
    public virtual void WinOperation(WinOperationData winOperationData)
    {
        //����״̬
        State = PlayerState.ChooseWhetherToWin;

        if (GameClient.IsConnect == false && PlayerId == 1)//������Ϸ�������������
        {
            Debug.Log("��Һ��Ʋ���");
            //��Һ��Ʋ���
            EatTileUI.Instance.WinOperation(winOperationData);
        }
        //Debug.Log($"���{PlayerId}���ˣ�");
    }

    /// <summary>
    /// ���ٶ���
    /// </summary>
    public virtual void Dispose()
    {
        //ע���¼�
        EventHandler.DealATileToPlayerEvent -= OnDealATileToPlayerEvent;
        EventHandler.PlayerPlayATileEvent -= OnPlayerPlayATileEvent;
    }
}

public enum PlayerState
{
    None,
    /// <summary>���ƽ׶�</summary>
    Playing,
    /// <summary>���ý׶Σ����ܲ�����</summary>
    Idle,
    /// <summary>���ƽ׶Σ������ж��Ƿ����</summary>
    Eat,
    /// <summary>׼�����ƽ׶�</summary>
    ChooseWhetherToListen,
    /// <summary>ѡ���Ƿ���ƽ׶�</summary>
    ChooseWhetherToWin,
    /// <summary>��Ϸ�����׶�</summary>
    GameOver,

}


/// <summary>
/// ������ƺ����������
/// </summary>
public struct ListeningTilesData
{
    /// <summary>�Ѿ����˵��Ƶ��б�</summary>
    public List<List<MahJongType>> eatMahJongTypeList;
    /// <summary>ÿ�ֿ��ܺ��Ƶķ�ʽ</summary>
    public List<ListenItem> listenItemList;
}

public struct ListenItem
{
    /// <summary>���Ƶķ�ʽ�������ַ�ʽȥ��</summary>
    public ListenType listenType;
    /// <summary>�����</summary>
    public List<List<MahJongType>> wildMahJongTypeList;
    /// <summary>������</summary>
    public List<MahJongType> listenTiles;
    /// <summary>�������ƣ���������</summary>
    public List<MahJongType> otherTiles;
    /// <summary>Ӯ��Щ��</summary>
    public List<MahJongType> winTiles;

}

/// <summary>
/// ���Ƶķ�ʽ
/// </summary>
public enum ListenType
{
    None,
    /// <summary>ʣһ���ƣ�Ҳ��һ����</summary>
    SingleTile,
    /// <summary>����</summary>
    SideWay,
    /// <summary>����</summary>
    SandwichWay,
    /// <summary>������</summary>
    TwoDoubleTile,
    /// <summary>��һ�ԣ�֧�ԣ�</summary>
    OneDoubleTile,
    /// <summary>�δ��</summary>
    StrongWind,
}