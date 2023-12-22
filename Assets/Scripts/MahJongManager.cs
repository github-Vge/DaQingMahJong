using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// 用于创建和显示麻将的管理类
/// </summary>
public class MahJongManager : MonoBehaviour
{
    /// <summary>单例</summary>
    public static MahJongManager Instance;


    [Header("所有麻将牌的预制体")]
    public GameObject[] mMahJongPrefabs;
    [Header("4个玩家的初始手牌的位置")]
    public Transform[] mTilesInitPositions;


    /// <summary>4个玩家的牌堆的麻将实例，key：玩家Id，value：玩家的麻将实例。默认玩家1是当前玩家</summary>
    public Dictionary<int, MahJongGameObjects> mPlayerGameObjects { get; private set; } = new Dictionary<int, MahJongGameObjects>();

    /// <summary>一个麻将的宽度0.67f</summary>
    private const float tileWidth = 0.67f;
    /// <summary>一个麻将的高度0.9f</summary>
    private const float tileHeight = 0.9f;
    /// <summary>一个麻将的厚度0.45f</summary>
    private const float tileThick = 0.45f;

    private void Awake()
    {
        //初始化单例
        Instance = this;
        //新建玩家牌堆的麻将实例
        for (int i = 1; i <= 4; i++)
        {
            mPlayerGameObjects[i] = new MahJongGameObjects();
        }
    }



    /// <summary>
    /// 获取一个新的麻将实例
    /// </summary>
    /// <param name="mahJongType">麻将类型，哪张牌</param>
    /// <param name="position">位置</param>
    /// <param name="quaternion">旋转</param>
    /// <returns>新的麻将实例</returns>
    public GameObject CreateAMahJong(MahJongType mahJongType, Vector3 position, Quaternion quaternion)
    {
        //实例化
        GameObject mahJong = Instantiate(mMahJongPrefabs[(int)mahJongType - 1], position, quaternion, this.transform);
        //添加碰撞体，让麻将可以接收射线检测
        mahJong.AddComponent<MeshCollider>();
        //添加高亮组件，让麻将可以接受高亮显示
        mahJong.AddComponent<HighlightableObject>();
        //添加麻将组件，并指定麻将类型
        mahJong.AddComponent<MahjongTile>().MahJongType = mahJongType;
        //返回
        return mahJong;
    }


    /// <summary>
    /// 麻将牌初始化
    /// </summary>
    public void MahJongTilesInited()
    {
        //为4个玩家显示手牌
        for (int i = 1; i <= 4; i++)
        {
            //获取玩家的手牌
            MahJongTiles mahJongTiles = GameManager.Instance.Players[i].PlayerTiles;
            //手牌排序TODO:不要放在这里
            mahJongTiles.tiles.Sort();
            //获取玩家的左方向
            Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[i - 1].position).normalized;
            //获取麻将牌最左边的位置
            Vector3 mostLeftPosition = mTilesInitPositions[i - 1].position + (mahJongTiles.tiles.Count - 1) * tileWidth * 0.5f * leftVector;
            for (int j = 0; j < mahJongTiles.tiles.Count; j++)
            {
                //逐个偏移
                Vector3 position = mostLeftPosition - tileWidth * j * leftVector;
                //创建麻将实例
                GameObject mahJong = CreateAMahJong(mahJongTiles.tiles[j], position, mTilesInitPositions[i - 1].rotation);
                //添加到数据中
                mPlayerGameObjects[i].tiles.Add(mahJong);
            }
        }
    }

    /// <summary>
    /// 出牌
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    /// <param name="mahJongType">麻将类型</param>
    public void PlayTile(int playerId, MahJongType mahJongType)
    {
        //获取玩家选择的手牌
        GameObject mahJong = mPlayerGameObjects[playerId].tiles.First(p => mahJongType == GetMahJongType(p));
        //销毁玩家手牌的显示
        mPlayerGameObjects[playerId].tiles.Remove(mahJong);
        //销毁物体
        Destroy(mahJong);
        //重新排序手牌
        ResortPlayerTiles(playerId);
        //将牌放入打出的牌堆中
        MoveTileToPlayedTiles(playerId, mahJongType);
    }

    /// <summary>
    /// 为玩家发了一张牌（用于显示手牌）
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    /// <param name="mahJongType">麻将类型</param>
    public void DealedATile(int playerId, MahJongType mahJongType)
    {
        //获取玩家的左方向
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[playerId - 1].position).normalized;
        //获取麻将牌最右边的位置
        Vector3 mostRightPosition = mTilesInitPositions[playerId - 1].position - (mPlayerGameObjects[playerId].tiles.Count +2) * tileWidth * 0.5f * leftVector;
        //新建麻将实例
        GameObject mahJong = CreateAMahJong(mahJongType, mostRightPosition, mTilesInitPositions[playerId - 1].rotation);
        //添加到字典中
        mPlayerGameObjects[playerId].tiles.Add(mahJong);
    }

    /// <summary>
    /// 把牌放入打出的牌堆中
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    /// <param name="mahJongType">麻将类型</param>
    private void MoveTileToPlayedTiles(int playerId, MahJongType mahJongType)
    {
        //获取所有打出的手牌
        List<GameObject> playedTiles = mPlayerGameObjects[playerId].playedTiles;
        //获取玩家的左方向
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[playerId - 1].position).normalized;
        //获取玩家后方向的位置
        Vector3 backVector = mTilesInitPositions[playerId - 1].position.normalized;
        //获取麻将牌应在的位置
        Vector3 mostLeftPosition = mTilesInitPositions[playerId - 1].position * 0.4f
            + 1f * tileWidth * leftVector
            - (playedTiles.Count % 8) * tileWidth * leftVector
            + (playedTiles.Count / 8) * tileHeight * backVector;
        //创建麻将实例
        GameObject mahJong = CreateAMahJong(mahJongType, mostLeftPosition, mTilesInitPositions[playerId - 1].rotation);
        //旋转麻将（让麻将躺在桌子上）
        mahJong.transform.Rotate(new Vector3(-90f, 0f, 0f), Space.Self);
        //添加到麻将实例数据中
        playedTiles.Add(mahJong);
    }

    /// <summary>
    /// 重新排序玩家的手牌
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    private void ResortPlayerTiles(int playerId)
    {
        //获取所有手牌的实例
        List<GameObject> tiles = mPlayerGameObjects[playerId].tiles;
        //排序手牌
        tiles = tiles.OrderBy(o => GetMahJongType(o)).ToList();
        //获取玩家的左方向
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[playerId - 1].position).normalized;
        //获取麻将牌最左边的位置
        Vector3 mostLeftPosition = mTilesInitPositions[playerId - 1].position + (tiles.Count - 1) * tileWidth * 0.5f * leftVector;
        for (int i = 0; i < tiles.Count; i++)
        {
            //获取麻将应在的位置
            Vector3 position = mostLeftPosition - tileWidth * i * leftVector;
            //移动
            tiles[i].transform.position = position;
        }


    }


    /// <summary>
    /// 显示吃牌操作
    /// </summary>
    /// <param name="eatTileOperation">吃牌的操作，其中包含了吃牌操作需要的信息</param>
    public void EatTile(EatTileOperation eatTileOperation)
    {
        //移除出牌玩家的麻将
        {
            //获取物体
            GameObject mahJong = mPlayerGameObjects[eatTileOperation.fromPlayerId].playedTiles.First(p => eatTileOperation.mahJongType == GetMahJongType(p));
            //销毁物体
            Destroy(mahJong);
            //移出列表
            mPlayerGameObjects[eatTileOperation.fromPlayerId].playedTiles.Remove(mahJong);
        }
        //移除吃牌玩家的手牌
        foreach (MahJongType mahJongType in eatTileOperation.fitEatMahJongTypeList)
        {
            //获取物体
            GameObject mahJong = mPlayerGameObjects[eatTileOperation.toPlayerId].tiles.First(p => GetMahJongType(p) == mahJongType);
            //销毁物体
            Destroy(mahJong);
            //移出列表
            mPlayerGameObjects[eatTileOperation.toPlayerId].tiles.Remove(mahJong);
        }
        //获取所有的吃牌
        List<List<GameObject>> eatenTiles = mPlayerGameObjects[eatTileOperation.toPlayerId].eatTiles;
        //获取玩家的左方向
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[eatTileOperation.toPlayerId - 1].position).normalized;
        //获取麻将牌应在的位置
        Vector3 mostLeftPosition = mTilesInitPositions[eatTileOperation.toPlayerId - 1].position * 0.8f
            + 6f * tileWidth * leftVector;
        //吃牌列的所有牌
        List<MahJongType> mahJongTypeList = eatTileOperation.fitEatMahJongTypeList.ToList();
        //插入吃到的牌
        mahJongTypeList.Insert(1, eatTileOperation.mahJongType);
        //新增一个吃牌列
        eatenTiles.Add(new List<GameObject>());
        //逐个创建麻将实例
        foreach (MahJongType mahJongType in mahJongTypeList)
        {
            //创建麻将实例
            GameObject mahJong = CreateAMahJong(mahJongType, 
                mostLeftPosition - eatenTiles.Sum(list => list.Count) * tileWidth * leftVector + Vector3.down * 0.2f,
                mTilesInitPositions[eatTileOperation.toPlayerId - 1].rotation);
            //旋转麻将（让麻将躺在桌子上）
            mahJong.transform.Rotate(new Vector3(-90f, 0f, 0f), Space.Self);
            //添加到麻将实例数据中
            eatenTiles.Last().Add(mahJong);
        }

    }

    /// <summary>
    /// 为玩家显示宝牌
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    /// <param name="treasure">宝牌是哪张牌</param>
    public void ShowTreasure(int playerId, MahJongType treasure)
    {
        //获取玩家的左方向
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[playerId - 1].position).normalized;
        //获取宝牌牌应在的位置
        Vector3 treasurePosition = mTilesInitPositions[playerId - 1].position * 0.8f
            + 6f * tileWidth * leftVector + Vector3.down * 0.2f
            - mPlayerGameObjects[playerId].eatTiles.Sum(list => list.Count) * tileWidth * leftVector;
        //创建麻将实例
        GameObject mahJong = CreateAMahJong(treasure, treasurePosition, mTilesInitPositions[playerId - 1].rotation);
        //旋转麻将（让麻将躺在桌子上）
        mahJong.transform.Translate( -Vector3.forward * 0.225f + Vector3.up * (mTilesInitPositions[0].position.y - mahJong.transform.position.y), Space.Self);
        //添加到麻将实例数据中
        mPlayerGameObjects[playerId].treasure = mahJong;
    }

    /// <summary>
    /// 让麻将牌躺下（玩家胡了时调用）
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    /// <param name="mahJongType">麻将类型</param>
    public void LayTreasure(int playerId)
    {
        //获取玩家的左方向
        Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[playerId - 1].position).normalized;
        //获取宝牌牌应在的位置
        Vector3 treasurePosition = mTilesInitPositions[playerId - 1].position * 0.8f
            + 6f * tileWidth * leftVector + Vector3.down * 0.2f
            - mPlayerGameObjects[playerId].eatTiles.Sum(list => list.Count) * tileWidth * leftVector;
        //创建麻将实例
        GameObject mahJong = mPlayerGameObjects[playerId].treasure;
        //移动到位置
        mahJong.transform.position = treasurePosition;
        //旋转麻将（让麻将躺在桌子上）
        mahJong.transform.Rotate(new Vector3(-90f, 0f, 0f), Space.Self);
        //右移和下移
        mahJong.transform.Translate(Vector3.left * tileWidth * 0.5f, Space.Self);
    }

    /// <summary>
    /// 显示胡牌效果
    /// </summary>
    /// <param name="gameOver">胡牌数据</param>
    public void ShowWin(GameNetMessage.GameOver gameOver)
    {
        //让胡牌的玩家的手牌都倒下
        mPlayerGameObjects[gameOver.winPlayerId].tiles.ForEach(
            a => a.transform.Rotate(new Vector3(-90f, 0f, 0f), Space.Self)
            );
        //再往下移一点
        mPlayerGameObjects[gameOver.winPlayerId].tiles.ForEach(
            a => a.transform.Translate(Vector3.back * (tileHeight - tileThick) * 0.5f, Space.Self)
            );
        //销毁打出的牌
        if(gameOver.overType == GameNetMessage.OverType.SelfTouch)//自摸
        {
            //获取物体
            GameObject selfTouchMahJong = mPlayerGameObjects[gameOver.winPlayerId].tiles.Last();
            //让麻将高亮
            selfTouchMahJong.GetComponent<HighlightableObject>()?.FlashingOn(Color.cyan, Color.white);
        }
        else//别的玩家打出的牌
        {
            //获取玩家的左方向
            Vector3 leftVector = Vector3.Cross(Vector3.up, mTilesInitPositions[gameOver.winPlayerId - 1].position).normalized;
            //获取麻将牌最右边的位置
            Vector3 mostRightPosition = mTilesInitPositions[gameOver.winPlayerId - 1].position
                - (mPlayerGameObjects[gameOver.winPlayerId].tiles.Count + 2) * tileWidth * 0.5f * leftVector;
            //获取物体
            GameObject destoryMahJong = mPlayerGameObjects[gameOver.fromPlayerId].playedTiles.Last();
            //移出列表
            mPlayerGameObjects[gameOver.fromPlayerId].playedTiles.Remove(destoryMahJong);
            //销毁物体
            Destroy(destoryMahJong);
            //新建麻将实例
            GameObject mahJong = CreateAMahJong(gameOver.mahJongType, mostRightPosition, mTilesInitPositions[gameOver.winPlayerId - 1].rotation);
            //让麻将躺下
            mahJong.transform.Rotate(new Vector3(-90f, 0f, 0f), Space.Self);
            //再往下移一点
            mahJong.transform.Translate(Vector3.back * (tileHeight - tileThick) * 0.5f, Space.Self);
            //让麻将高亮
            mahJong.GetComponent<HighlightableObject>()?.FlashingOn(Color.cyan, Color.white);
        }


    }



    /// <summary>
    /// 根据麻将实例获取麻将的类型
    /// </summary>
    /// <param name="mahJong">麻将实例</param>
    /// <returns>麻将类型</returns>
    public MahJongType GetMahJongType(GameObject mahJong)
    {
        return mahJong.GetComponent<MahjongTile>().MahJongType;
    }




}


public enum MahJongType
{
    None,
    /// <summary>红中</summary>
    RedDragon,
    /// <summary>饼（1-9）</summary>
    Circle1, Circle2, Circle3, Circle4, Circle5, Circle6, Circle7, Circle8, Circle9,
    /// <summary>条（1-9）</summary>
    Stick1, Stick2, Stick3, Stick4, Stick5, Stick6, Stick7, Stick8, Stick9,
    /// <summary>万（1-9）</summary>
    Thousand1, Thousand2, Thousand3, Thousand4, Thousand5, Thousand6, Thousand7, Thousand8, Thousand9,

}


public class MahJongGameObjects
{
    /// <summary>玩家Id</summary>
    public int playerId;
    /// <summary>玩家的手牌</summary>
    public List<GameObject> tiles = new List<GameObject>();
    /// <summary>玩家吃的牌</summary>
    public List<List<GameObject>> eatTiles = new List<List<GameObject>>();
    /// <summary>玩家打出的牌</summary>
    public List<GameObject> playedTiles = new List<GameObject>();
    /// <summary>宝牌</summary>
    public GameObject treasure;
}