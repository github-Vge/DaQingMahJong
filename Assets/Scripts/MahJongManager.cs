using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// ���ڴ�������ʾ�齫�Ĺ�����
/// </summary>
public class MahJongManager : MonoBehaviour
{
    /// <summary>����</summary>
    public static MahJongManager Instance;


    [Header("�����齫�Ƶ�Ԥ����")]
    public GameObject[] mMahJongPrefabs;
    [Header("4����ҵĳ�ʼ���Ƶ�λ��")]
    public Transform[] mTilesInitPositions;


    /// <summary>4����ҵ��ƶѵ��齫ʵ����key�����Id��value����ҵ��齫ʵ����Ĭ�����1�ǵ�ǰ���</summary>
    public Dictionary<int, MahJongGameObjects> mPlayerGameObjects { get; private set; } = new Dictionary<int, MahJongGameObjects>();

    /// <summary>һ���齫�Ŀ��0.67f</summary>
    private const float tileWidth = 0.67f;
    /// <summary>һ���齫�ĸ߶�0.9f</summary>
    private const float tileHeight = 0.9f;
    /// <summary>һ���齫�ĺ��0.45f</summary>
    private const float tileThick = 0.45f;

    private void Awake()
    {
        //��ʼ������
        Instance = this;
        //�½�����ƶѵ��齫ʵ��
        for (int i = 1; i <= 4; i++)
        {
            mPlayerGameObjects[i] = new MahJongGameObjects();
        }
    }



    /// <summary>
    /// ��ȡһ���µ��齫ʵ��
    /// </summary>
    /// <param name="mahJongType">�齫���ͣ�������</param>
    /// <param name="position">λ��</param>
    /// <param name="quaternion">��ת</param>
    /// <returns>�µ��齫ʵ��</returns>
    public GameObject CreateAMahJong(MahJongType mahJongType, Vector3 position, Quaternion quaternion)
    {
        //ʵ����
        GameObject mahJong = Instantiate(mMahJongPrefabs[(int)mahJongType - 1], position, quaternion, this.transform);
        //�����ײ�壬���齫���Խ������߼��
        mahJong.AddComponent<MeshCollider>();
        //��Ӹ�����������齫���Խ��ܸ�����ʾ
        mahJong.AddComponent<HighlightableObject>();
        //����齫�������ָ���齫����
        mahJong.AddComponent<MahjongTile>().MahJongType = mahJongType;
        //����
        return mahJong;
    }


    /// <summary>
    /// �齫�Ƴ�ʼ��
    /// </summary>
    public void MahJongTilesInited()
    {
        //Ϊ4�������ʾ����
        for (int i = 1; i <= 4; i++)
        {
            //��ȡ��ҵ�����
            MahJongTiles mahJongTiles = GameManager.Instance.Players[i].PlayerTiles;
            //��������TODO:��Ҫ��������
            mahJongTiles.tiles.Sort();
            //��ȡ��ҵ�����
            Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[i - 1].position).normalized;
            //��ȡ�齫������ߵ�λ��
            Vector3 mostLeftPosition = mTilesInitPositions[i - 1].position + (mahJongTiles.tiles.Count - 1) * tileWidth * 0.5f * leftVector;
            for (int j = 0; j < mahJongTiles.tiles.Count; j++)
            {
                //���ƫ��
                Vector3 position = mostLeftPosition - tileWidth * j * leftVector;
                //�����齫ʵ��
                GameObject mahJong = CreateAMahJong(mahJongTiles.tiles[j], position, mTilesInitPositions[i - 1].rotation);
                //��ӵ�������
                mPlayerGameObjects[i].tiles.Add(mahJong);
            }
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="playerId">���Id</param>
    /// <param name="mahJongType">�齫����</param>
    public void PlayTile(int playerId, MahJongType mahJongType)
    {
        //��ȡ���ѡ�������
        GameObject mahJong = mPlayerGameObjects[playerId].tiles.First(p => mahJongType == GetMahJongType(p));
        //����������Ƶ���ʾ
        mPlayerGameObjects[playerId].tiles.Remove(mahJong);
        //��������
        Destroy(mahJong);
        //������������
        ResortPlayerTiles(playerId);
        //���Ʒ��������ƶ���
        MoveTileToPlayedTiles(playerId, mahJongType);
    }

    /// <summary>
    /// Ϊ��ҷ���һ���ƣ�������ʾ���ƣ�
    /// </summary>
    /// <param name="playerId">���Id</param>
    /// <param name="mahJongType">�齫����</param>
    public void DealedATile(int playerId, MahJongType mahJongType)
    {
        //��ȡ��ҵ�����
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[playerId - 1].position).normalized;
        //��ȡ�齫�����ұߵ�λ��
        Vector3 mostRightPosition = mTilesInitPositions[playerId - 1].position - (mPlayerGameObjects[playerId].tiles.Count +2) * tileWidth * 0.5f * leftVector;
        //�½��齫ʵ��
        GameObject mahJong = CreateAMahJong(mahJongType, mostRightPosition, mTilesInitPositions[playerId - 1].rotation);
        //��ӵ��ֵ���
        mPlayerGameObjects[playerId].tiles.Add(mahJong);
    }

    /// <summary>
    /// ���Ʒ��������ƶ���
    /// </summary>
    /// <param name="playerId">���Id</param>
    /// <param name="mahJongType">�齫����</param>
    private void MoveTileToPlayedTiles(int playerId, MahJongType mahJongType)
    {
        //��ȡ���д��������
        List<GameObject> playedTiles = mPlayerGameObjects[playerId].playedTiles;
        //��ȡ��ҵ�����
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[playerId - 1].position).normalized;
        //��ȡ��Һ����λ��
        Vector3 backVector = mTilesInitPositions[playerId - 1].position.normalized;
        //��ȡ�齫��Ӧ�ڵ�λ��
        Vector3 mostLeftPosition = mTilesInitPositions[playerId - 1].position * 0.4f
            + 1f * tileWidth * leftVector
            - (playedTiles.Count % 8) * tileWidth * leftVector
            + (playedTiles.Count / 8) * tileHeight * backVector;
        //�����齫ʵ��
        GameObject mahJong = CreateAMahJong(mahJongType, mostLeftPosition, mTilesInitPositions[playerId - 1].rotation);
        //��ת�齫�����齫���������ϣ�
        mahJong.transform.Rotate(new Vector3(-90f, 0f, 0f), Space.Self);
        //��ӵ��齫ʵ��������
        playedTiles.Add(mahJong);
    }

    /// <summary>
    /// ����������ҵ�����
    /// </summary>
    /// <param name="playerId">���Id</param>
    private void ResortPlayerTiles(int playerId)
    {
        //��ȡ�������Ƶ�ʵ��
        List<GameObject> tiles = mPlayerGameObjects[playerId].tiles;
        //��������
        tiles = tiles.OrderBy(o => GetMahJongType(o)).ToList();
        //��ȡ��ҵ�����
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[playerId - 1].position).normalized;
        //��ȡ�齫������ߵ�λ��
        Vector3 mostLeftPosition = mTilesInitPositions[playerId - 1].position + (tiles.Count - 1) * tileWidth * 0.5f * leftVector;
        for (int i = 0; i < tiles.Count; i++)
        {
            //��ȡ�齫Ӧ�ڵ�λ��
            Vector3 position = mostLeftPosition - tileWidth * i * leftVector;
            //�ƶ�
            tiles[i].transform.position = position;
        }


    }


    /// <summary>
    /// ��ʾ���Ʋ���
    /// </summary>
    /// <param name="eatTileOperation">���ƵĲ��������а����˳��Ʋ�����Ҫ����Ϣ</param>
    public void EatTile(EatTileOperation eatTileOperation)
    {
        //�Ƴ�������ҵ��齫
        {
            //��ȡ����
            GameObject mahJong = mPlayerGameObjects[eatTileOperation.fromPlayerId].playedTiles.First(p => eatTileOperation.mahJongType == GetMahJongType(p));
            //��������
            Destroy(mahJong);
            //�Ƴ��б�
            mPlayerGameObjects[eatTileOperation.fromPlayerId].playedTiles.Remove(mahJong);
        }
        //�Ƴ�������ҵ�����
        foreach (MahJongType mahJongType in eatTileOperation.fitEatMahJongTypeList)
        {
            //��ȡ����
            GameObject mahJong = mPlayerGameObjects[eatTileOperation.toPlayerId].tiles.First(p => GetMahJongType(p) == mahJongType);
            //��������
            Destroy(mahJong);
            //�Ƴ��б�
            mPlayerGameObjects[eatTileOperation.toPlayerId].tiles.Remove(mahJong);
        }
        //��ȡ���еĳ���
        List<List<GameObject>> eatenTiles = mPlayerGameObjects[eatTileOperation.toPlayerId].eatTiles;
        //��ȡ��ҵ�����
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[eatTileOperation.toPlayerId - 1].position).normalized;
        //��ȡ�齫��Ӧ�ڵ�λ��
        Vector3 mostLeftPosition = mTilesInitPositions[eatTileOperation.toPlayerId - 1].position * 0.8f
            + 6f * tileWidth * leftVector;
        //�����е�������
        List<MahJongType> mahJongTypeList = eatTileOperation.fitEatMahJongTypeList.ToList();
        //����Ե�����
        mahJongTypeList.Insert(1, eatTileOperation.mahJongType);
        //����һ��������
        eatenTiles.Add(new List<GameObject>());
        //��������齫ʵ��
        foreach (MahJongType mahJongType in mahJongTypeList)
        {
            //�����齫ʵ��
            GameObject mahJong = CreateAMahJong(mahJongType, 
                mostLeftPosition - eatenTiles.Sum(list => list.Count) * tileWidth * leftVector + Vector3.down * 0.2f,
                mTilesInitPositions[eatTileOperation.toPlayerId - 1].rotation);
            //��ת�齫�����齫���������ϣ�
            mahJong.transform.Rotate(new Vector3(-90f, 0f, 0f), Space.Self);
            //��ӵ��齫ʵ��������
            eatenTiles.Last().Add(mahJong);
        }

    }

    /// <summary>
    /// Ϊ�����ʾ����
    /// </summary>
    /// <param name="playerId">���Id</param>
    /// <param name="treasure">������������</param>
    public void ShowTreasure(int playerId, MahJongType treasure)
    {
        //��ȡ��ҵ�����
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[playerId - 1].position).normalized;
        //��ȡ������Ӧ�ڵ�λ��
        Vector3 treasurePosition = mTilesInitPositions[playerId - 1].position * 0.8f
            + 6f * tileWidth * leftVector + Vector3.down * 0.2f
            - mPlayerGameObjects[playerId].eatTiles.Sum(list => list.Count) * tileWidth * leftVector;
        //�����齫ʵ��
        GameObject mahJong = CreateAMahJong(treasure, treasurePosition, mTilesInitPositions[playerId - 1].rotation);
        //��ת�齫�����齫���������ϣ�
        mahJong.transform.Translate( -Vector3.forward * 0.225f + Vector3.up * (mTilesInitPositions[0].position.y - mahJong.transform.position.y), Space.Self);
        //��ӵ��齫ʵ��������
        mPlayerGameObjects[playerId].treasure = mahJong;
    }

    /// <summary>
    /// ���齫�����£���Һ���ʱ���ã�
    /// </summary>
    /// <param name="playerId">���Id</param>
    /// <param name="mahJongType">�齫����</param>
    public void LayTreasure(int playerId)
    {
        //��ȡ��ҵ�����
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[playerId - 1].position).normalized;
        //��ȡ������Ӧ�ڵ�λ��
        Vector3 treasurePosition = mTilesInitPositions[playerId - 1].position * 0.8f
            + 6f * tileWidth * leftVector + Vector3.down * 0.2f
            - mPlayerGameObjects[playerId].eatTiles.Sum(list => list.Count) * tileWidth * leftVector;
        //�����齫ʵ��
        GameObject mahJong = mPlayerGameObjects[playerId].treasure;
        //�ƶ���λ��
        mahJong.transform.position = treasurePosition;
        //��ת�齫�����齫���������ϣ�
        mahJong.transform.Rotate(new Vector3(-90f, 0f, 0f), Space.Self);
        //���ƺ�����
        mahJong.transform.Translate(Vector3.left * tileWidth * 0.5f, Space.Self);
    }

    /// <summary>
    /// ��ʾ����Ч��
    /// </summary>
    /// <param name="gameOver">��������</param>
    public void ShowWin(GameNetMessage.GameOver gameOver)
    {
        //�ú��Ƶ���ҵ����ƶ�����
        mPlayerGameObjects[gameOver.winPlayerId].tiles.ForEach(
            a => a.transform.Rotate(new Vector3(-90f, 0f, 0f), Space.Self)
            );
        //��������һ��
        mPlayerGameObjects[gameOver.winPlayerId].tiles.ForEach(
            a => a.transform.Translate(Vector3.back * (tileHeight - tileThick) * 0.5f, Space.Self)
            );
        //���ٴ������
        if(gameOver.overType == GameNetMessage.OverType.SelfTouch)//����
        {
            //��ȡ����
            GameObject selfTouchMahJong = mPlayerGameObjects[gameOver.winPlayerId].tiles.Last();
            //���齫����
            selfTouchMahJong.GetComponent<HighlightableObject>()?.FlashingOn(Color.cyan, Color.white);
        }
        else//�����Ҵ������
        {
            //��ȡ��ҵ�����
            Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[gameOver.winPlayerId - 1].position).normalized;
            //��ȡ�齫�����ұߵ�λ��
            Vector3 mostRightPosition = mTilesInitPositions[gameOver.winPlayerId - 1].position
                - (mPlayerGameObjects[gameOver.winPlayerId].tiles.Count + 2) * tileWidth * 0.5f * leftVector;
            //��ȡ����
            GameObject destoryMahJong = mPlayerGameObjects[gameOver.fromPlayerId].playedTiles.Last();
            //�Ƴ��б�
            mPlayerGameObjects[gameOver.fromPlayerId].playedTiles.Remove(destoryMahJong);
            //��������
            Destroy(destoryMahJong);
            //�½��齫ʵ��
            GameObject mahJong = CreateAMahJong(gameOver.mahJongType, mostRightPosition, mTilesInitPositions[gameOver.winPlayerId - 1].rotation);
            //���齫����
            mahJong.transform.Rotate(new Vector3(-90f, 0f, 0f), Space.Self);
            //��������һ��
            mahJong.transform.Translate(Vector3.back * (tileHeight - tileThick) * 0.5f, Space.Self);
            //���齫����
            mahJong.GetComponent<HighlightableObject>()?.FlashingOn(Color.cyan, Color.white);
        }


    }



    /// <summary>
    /// �����齫ʵ����ȡ�齫������
    /// </summary>
    /// <param name="mahJong">�齫ʵ��</param>
    /// <returns>�齫����</returns>
    public MahJongType GetMahJongType(GameObject mahJong)
    {
        return mahJong.GetComponent<MahjongTile>().MahJongType;
    }




}


public enum MahJongType
{
    None,
    /// <summary>����</summary>
    RedDragon,
    /// <summary>����1-9��</summary>
    Circle1, Circle2, Circle3, Circle4, Circle5, Circle6, Circle7, Circle8, Circle9,
    /// <summary>����1-9��</summary>
    Stick1, Stick2, Stick3, Stick4, Stick5, Stick6, Stick7, Stick8, Stick9,
    /// <summary>��1-9��</summary>
    Thousand1, Thousand2, Thousand3, Thousand4, Thousand5, Thousand6, Thousand7, Thousand8, Thousand9,

}


public class MahJongGameObjects
{
    /// <summary>���Id</summary>
    public int playerId;
    /// <summary>��ҵ�����</summary>
    public List<GameObject> tiles = new List<GameObject>();
    /// <summary>��ҳԵ���</summary>
    public List<List<GameObject>> eatTiles = new List<List<GameObject>>();
    /// <summary>��Ҵ������</summary>
    public List<GameObject> playedTiles = new List<GameObject>();
    /// <summary>����</summary>
    public GameObject treasure;
}