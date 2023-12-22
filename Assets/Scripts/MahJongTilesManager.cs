using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 管理玩家麻将牌数据的类
/// </summary>
public class MahJongTilesManager : MonoBehaviour
{
    /// <summary>单例</summary>
    public static MahJongTilesManager Instance;

    /// <summary>麻将的初始牌，只读</summary>
    private readonly List<MahJongType> mInitMahJongList = new List<MahJongType>();
    /// <summary>当前的麻将列表</summary>
    [HideInInspector]
    public List<MahJongType> mCurrentMahJongList = new List<MahJongType>();
    /// <summary>4个玩家的牌堆，key：玩家Id，value：玩家的牌。默认玩家1是当前玩家</summary>
    public Dictionary<int, MahJongTiles> mPlayerTiles = new Dictionary<int, MahJongTiles>();
    /// <summary>宝牌</summary>
    public MahJongType Treasure { get; private set; }

    private void Awake()
    {
        //初始化单例
        Instance = this;
        //每种类型的麻将添加4张
        for (int i = 1; i < Enum.GetValues(typeof(MahJongType)).Length; i++)
        {
            for (int j = 0; j < 4; j++)//添加4张
            {
                mInitMahJongList.Add((MahJongType)i);
            }
        }
        //当前的麻将列表（复制一份）
        mCurrentMahJongList = mInitMahJongList.ToList();
        //生成宝牌
        GenerateTreasure();
        //新建玩家牌堆
        for (int i = 1; i <= 4; i++)
        {
            mPlayerTiles[i] = new MahJongTiles();
        }
    }

    private void Start()
    {
        //显示剩余牌数
        MainUI.Instance.RemainingTileCount = mCurrentMahJongList.Count;

    }

    /// <summary>
    /// 生成宝牌
    /// </summary>
    private void GenerateTreasure()
    {
        //随机一张牌
        int randomIndex = UnityEngine.Random.Range(0, mCurrentMahJongList.Count);
        //添加到玩家的手牌中
        Treasure = mCurrentMahJongList[randomIndex];
        //从麻将列表中移除
        mCurrentMahJongList.Remove(Treasure);
    }

    /// <summary>
    /// 给指定玩家发牌（一次发多张牌）
    /// </summary>
    /// <param name="playerId">玩家Id（1-4）</param>
    /// <param name="tileCount">发牌数量</param>
    public void DealTiles(int playerId, int tileCount)
    {
        for (int i = 0; i < tileCount; i++)
        {
            //随机一张牌
            int randomIndex = UnityEngine.Random.Range(0, mCurrentMahJongList.Count);
            //添加到玩家的手牌中
            mPlayerTiles[playerId].tiles.Add(mCurrentMahJongList[randomIndex]);
            //从麻将列表中移除
            mCurrentMahJongList.RemoveAt(randomIndex);
        }
        //更改剩余牌数
        MainUI.Instance.RemainingTileCount -= tileCount;
    }

    /// <summary>
    /// 为玩家发一张牌
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    /// <returns>发了哪张牌</returns>
    public MahJongType DealATile(int playerId)
    {
        //没牌了，游戏结束
        if (mCurrentMahJongList.Count == 0)
        {
            //构建游戏结束数据
            GameNetMessage.GameOver gameOver = new GameNetMessage.GameOver
            {
                overType = GameNetMessage.OverType.NoTileLeft,
                mahJongType = MahJongType.None,
                fromPlayerId = playerId,
                winPlayerId = 0,
            };
            //游戏结束
            GameManager.Instance.GameOver(gameOver);
            return MahJongType.None;
        }
        //随机一张牌
        int randomIndex = UnityEngine.Random.Range(0, mCurrentMahJongList.Count);
        //获取牌的类型
        MahJongType mahJongType = mCurrentMahJongList[randomIndex];
        //添加到玩家的手牌中
        mPlayerTiles[playerId].tiles.Add(mCurrentMahJongList[randomIndex]);
        //从麻将列表中移除
        mCurrentMahJongList.RemoveAt(randomIndex);
        //更改剩余牌数
        MainUI.Instance.RemainingTileCount--;

        //显示发牌
        MahJongManager.Instance.DealedATile(playerId, mahJongType);

        //返回发的牌
        return mahJongType;
    }

    /// <summary>
    /// 出牌
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    /// <param name="mahJongType">麻将类型</param>
    public void PlayTile(int playerId, MahJongType mahJongType)
    {
        //玩家已经听牌，不能打出手牌
        if (GameManager.Instance.Players[playerId].IsListening == true //已经听牌
            && mahJongType != mPlayerTiles[playerId].tiles.Last()) //不是最后一张牌
        {
            return;
        }
        //从手牌中移除牌
        mPlayerTiles[playerId].tiles.Remove(mahJongType);
        //添加到打出的牌
        mPlayerTiles[playerId].playedTiles.Add(mahJongType);
        //显示出牌
        MahJongManager.Instance.PlayTile(playerId, mahJongType);        
        //检查是否可以听牌
        if (GameClient.IsConnect == false //单人游戏
            && GameManager.Instance.Players[playerId].IsListening == false
            && CheckListening(playerId))
        {
            //设置玩家状态
            MainPlayer.Instance.ListenOperation();
            //玩家听牌操作
            EatTileUI.Instance.ListeningOperation(mahJongType);

            return;
        }
        //玩家打出了牌
        GameManager.Instance.Players[playerId].PlayedTile(mahJongType);
    }

    private void Update()
    {
        //测试用，输出当前手牌
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
    /// 吃牌
    /// </summary>
    /// <param name="eatTileOperation">吃牌的操作，其中包含了吃牌操作需要的信息</param>
    public void EatTile(EatTileOperation eatTileOperation)
    {
        //从出牌玩家的打出牌池中移除牌
        mPlayerTiles[eatTileOperation.fromPlayerId].playedTiles.Remove(eatTileOperation.mahJongType);
        //配合吃牌的牌列表
        List<MahJongType> fitEatMahJongTypeList = eatTileOperation.fitEatMahJongTypeList.ToList();
        //从吃牌玩家的手牌中移除配合吃牌的牌
        fitEatMahJongTypeList.ForEach(a => mPlayerTiles[eatTileOperation.toPlayerId].tiles.Remove(a));
        //添加需要吃的牌
        fitEatMahJongTypeList.Insert(1, eatTileOperation.mahJongType);
        //将需要吃的牌移入到吃牌玩家的吃牌牌池中
        mPlayerTiles[eatTileOperation.toPlayerId].eatTiles.Add(fitEatMahJongTypeList);
        //播放吃牌音效
        SoundManager.Instance.PlayEatSound(SoundManager.Instance.GetEatSoundTypeFromEatTileType(eatTileOperation.eatTileType));
        //显示吃牌操作
        MahJongManager.Instance.EatTile(eatTileOperation);
    }

    /// <summary>
    /// 检查玩家是否可以听牌
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    /// <param name="mahJongType">麻将类型</param>
    /// <returns>是否可以听牌</returns>
    public bool CheckListening(int playerId)
    {
        //检查是否可以听牌
        bool checkListening = ListenManager.CheckListening(mPlayerTiles[playerId].tiles, mPlayerTiles[playerId].eatTiles, out ListeningTilesData listeningTilesData);
        //调试用[*不重要代码]
        //EatTileManager.Instance.PrintTree(playerId);
        //暂存玩家的胡牌数据
        GameManager.Instance.Players[playerId].ListeningTiles = checkListening ? listeningTilesData : default;
        //返回
        return checkListening;
    }




}


public class MahJongTiles
{
    /// <summary>玩家Id</summary>
    public int playerId;
    /// <summary>玩家的手牌</summary>
    public List<MahJongType> tiles = new List<MahJongType>();
    /// <summary>玩家吃的牌</summary>
    public List<List<MahJongType>> eatTiles = new List<List<MahJongType>>();
    /// <summary>玩家打出的牌</summary>
    public List<MahJongType> playedTiles = new List<MahJongType>();
}