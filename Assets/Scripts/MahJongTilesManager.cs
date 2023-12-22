using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ��������齫�����ݵ���
/// </summary>
public class MahJongTilesManager : MonoBehaviour
{
    /// <summary>����</summary>
    public static MahJongTilesManager Instance;

    /// <summary>�齫�ĳ�ʼ�ƣ�ֻ��</summary>
    private readonly List<MahJongType> mInitMahJongList = new List<MahJongType>();
    /// <summary>��ǰ���齫�б�</summary>
    [HideInInspector]
    public List<MahJongType> mCurrentMahJongList = new List<MahJongType>();
    /// <summary>4����ҵ��ƶѣ�key�����Id��value����ҵ��ơ�Ĭ�����1�ǵ�ǰ���</summary>
    public Dictionary<int, MahJongTiles> mPlayerTiles = new Dictionary<int, MahJongTiles>();
    /// <summary>����</summary>
    public MahJongType Treasure { get; private set; }

    private void Awake()
    {
        //��ʼ������
        Instance = this;
        //ÿ�����͵��齫���4��
        for (int i = 1; i < Enum.GetValues(typeof(MahJongType)).Length; i++)
        {
            for (int j = 0; j < 4; j++)//���4��
            {
                mInitMahJongList.Add((MahJongType)i);
            }
        }
        //��ǰ���齫�б�����һ�ݣ�
        mCurrentMahJongList = mInitMahJongList.ToList();
        //���ɱ���
        GenerateTreasure();
        //�½�����ƶ�
        for (int i = 1; i <= 4; i++)
        {
            mPlayerTiles[i] = new MahJongTiles();
        }
    }

    private void Start()
    {
        //��ʾʣ������
        MainUI.Instance.RemainingTileCount = mCurrentMahJongList.Count;

    }

    /// <summary>
    /// ���ɱ���
    /// </summary>
    private void GenerateTreasure()
    {
        //���һ����
        int randomIndex = UnityEngine.Random.Range(0, mCurrentMahJongList.Count);
        //��ӵ���ҵ�������
        Treasure = mCurrentMahJongList[randomIndex];
        //���齫�б����Ƴ�
        mCurrentMahJongList.Remove(Treasure);
    }

    /// <summary>
    /// ��ָ����ҷ��ƣ�һ�η������ƣ�
    /// </summary>
    /// <param name="playerId">���Id��1-4��</param>
    /// <param name="tileCount">��������</param>
    public void DealTiles(int playerId, int tileCount)
    {
        for (int i = 0; i < tileCount; i++)
        {
            //���һ����
            int randomIndex = UnityEngine.Random.Range(0, mCurrentMahJongList.Count);
            //��ӵ���ҵ�������
            mPlayerTiles[playerId].tiles.Add(mCurrentMahJongList[randomIndex]);
            //���齫�б����Ƴ�
            mCurrentMahJongList.RemoveAt(randomIndex);
        }
        //����ʣ������
        MainUI.Instance.RemainingTileCount -= tileCount;
    }

    /// <summary>
    /// Ϊ��ҷ�һ����
    /// </summary>
    /// <param name="playerId">���Id</param>
    /// <returns>����������</returns>
    public MahJongType DealATile(int playerId)
    {
        //û���ˣ���Ϸ����
        if (mCurrentMahJongList.Count == 0)
        {
            //������Ϸ��������
            GameNetMessage.GameOver gameOver = new GameNetMessage.GameOver
            {
                overType = GameNetMessage.OverType.NoTileLeft,
                mahJongType = MahJongType.None,
                fromPlayerId = playerId,
                winPlayerId = 0,
            };
            //��Ϸ����
            GameManager.Instance.GameOver(gameOver);
            return MahJongType.None;
        }
        //���һ����
        int randomIndex = UnityEngine.Random.Range(0, mCurrentMahJongList.Count);
        //��ȡ�Ƶ�����
        MahJongType mahJongType = mCurrentMahJongList[randomIndex];
        //��ӵ���ҵ�������
        mPlayerTiles[playerId].tiles.Add(mCurrentMahJongList[randomIndex]);
        //���齫�б����Ƴ�
        mCurrentMahJongList.RemoveAt(randomIndex);
        //����ʣ������
        MainUI.Instance.RemainingTileCount--;

        //��ʾ����
        MahJongManager.Instance.DealedATile(playerId, mahJongType);

        //���ط�����
        return mahJongType;
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="playerId">���Id</param>
    /// <param name="mahJongType">�齫����</param>
    public void PlayTile(int playerId, MahJongType mahJongType)
    {
        //����Ѿ����ƣ����ܴ������
        if (GameManager.Instance.Players[playerId].IsListening == true //�Ѿ�����
            && mahJongType != mPlayerTiles[playerId].tiles.Last()) //�������һ����
        {
            return;
        }
        //���������Ƴ���
        mPlayerTiles[playerId].tiles.Remove(mahJongType);
        //��ӵ��������
        mPlayerTiles[playerId].playedTiles.Add(mahJongType);
        //��ʾ����
        MahJongManager.Instance.PlayTile(playerId, mahJongType);        
        //����Ƿ��������
        if (GameClient.IsConnect == false //������Ϸ
            && GameManager.Instance.Players[playerId].IsListening == false
            && CheckListening(playerId))
        {
            //�������״̬
            MainPlayer.Instance.ListenOperation();
            //������Ʋ���
            EatTileUI.Instance.ListeningOperation(mahJongType);

            return;
        }
        //��Ҵ������
        GameManager.Instance.Players[playerId].PlayedTile(mahJongType);
    }

    private void Update()
    {
        //�����ã������ǰ����
        if (Input.GetKeyDown(KeyCode.J))
        {
            int playerId = MainPlayer.Instance.PlayerId;

            string str = string.Empty;
            mPlayerTiles[playerId].tiles.Sort();
            foreach (var item in mPlayerTiles[playerId].tiles)
            {
                str += item.ToString() + ",";
            }
            Debug.Log(str);
            str = string.Empty;
            List<GameObject> tiles = MahJongManager.Instance.mPlayerGameObjects[playerId].tiles;
            tiles = tiles.OrderBy(p => MahJongManager.Instance.GetMahJongType(p)).ToList();
            foreach (var item in tiles)
            {
                str += MahJongManager.Instance.GetMahJongType(item).ToString() + ",";
            }
            Debug.Log(str);
        }
    }


    /// <summary>
    /// ����
    /// </summary>
    /// <param name="eatTileOperation">���ƵĲ��������а����˳��Ʋ�����Ҫ����Ϣ</param>
    public void EatTile(EatTileOperation eatTileOperation)
    {
        //�ӳ�����ҵĴ���Ƴ����Ƴ���
        mPlayerTiles[eatTileOperation.fromPlayerId].playedTiles.Remove(eatTileOperation.mahJongType);
        //��ϳ��Ƶ����б�
        List<MahJongType> fitEatMahJongTypeList = eatTileOperation.fitEatMahJongTypeList.ToList();
        //�ӳ�����ҵ��������Ƴ���ϳ��Ƶ���
        fitEatMahJongTypeList.ForEach(a => mPlayerTiles[eatTileOperation.toPlayerId].tiles.Remove(a));
        //�����Ҫ�Ե���
        fitEatMahJongTypeList.Insert(1, eatTileOperation.mahJongType);
        //����Ҫ�Ե������뵽������ҵĳ����Ƴ���
        mPlayerTiles[eatTileOperation.toPlayerId].eatTiles.Add(fitEatMahJongTypeList);
        //���ų�����Ч
        SoundManager.Instance.PlayEatSound(SoundManager.Instance.GetEatSoundTypeFromEatTileType(eatTileOperation.eatTileType));
        //��ʾ���Ʋ���
        MahJongManager.Instance.EatTile(eatTileOperation);
    }

    /// <summary>
    /// �������Ƿ��������
    /// </summary>
    /// <param name="playerId">���Id</param>
    /// <param name="mahJongType">�齫����</param>
    /// <returns>�Ƿ��������</returns>
    public bool CheckListening(int playerId)
    {
        //����Ƿ��������
        bool checkListening = ListenManager.CheckListening(mPlayerTiles[playerId].tiles, mPlayerTiles[playerId].eatTiles, out ListeningTilesData listeningTilesData);
        //������[*����Ҫ����]
        //EatTileManager.Instance.PrintTree(playerId);
        //�ݴ���ҵĺ�������
        GameManager.Instance.Players[playerId].ListeningTiles = checkListening ? listeningTilesData : default;
        //����
        return checkListening;
    }




}


public class MahJongTiles
{
    /// <summary>���Id</summary>
    public int playerId;
    /// <summary>��ҵ�����</summary>
    public List<MahJongType> tiles = new List<MahJongType>();
    /// <summary>��ҳԵ���</summary>
    public List<List<MahJongType>> eatTiles = new List<List<MahJongType>>();
    /// <summary>��Ҵ������</summary>
    public List<MahJongType> playedTiles = new List<MahJongType>();
}