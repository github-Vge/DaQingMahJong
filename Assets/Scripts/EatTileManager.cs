using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 吃牌算法类
/// </summary>
public class EatTileManager : MonoBehaviour
{
    /// <summary>单例</summary>
    public static EatTileManager Instance;

    /// <summary>所有可以吃牌玩家的操作回调。吃牌则whetherToEat为true，不吃牌为false</summary>
    public List<EatTileOperation> TotalNumberOfOperationsCurrentlyInProgress { get; set; } = new List<EatTileOperation>();

    private void Awake()
    {
        //初始化单例
        Instance = this;
    }

    /// <summary>
    /// 检查其他玩家是否要吃牌
    /// </summary>
    /// <param name="playerId">打出牌的玩家</param>
    /// <param name="mahJongType"></param>
    /// <returns>false代表没有玩家要吃牌，true代表有玩家要吃牌</returns>
    public bool CheckEat(int playerId, MahJongType mahJongType)
    {
        //清空树的节点
        TreeNode<List<MahJongType>>.allTreeNodeList.Clear();

        //无牌时不需要判断（无牌即玩家已经打出了牌，经过吃牌阶段时，其他玩家没有吃牌操作后，有None类型的牌，即无牌）
        if (mahJongType == MahJongType.None) return false;

        //需要检查的玩家列表
        List<int> checkPlayerIdList = new List<int>();
        //遍历4个玩家
        for (int i = 1; i <= 4; i++)
        {
            //如果是出牌玩家 或者 玩家已经听牌，则不需要吃牌判断
            if (i == playerId
                || GameManager.Instance.Players[i].IsListening == true
                )
            {
                continue;
            }
            checkPlayerIdList.Add(i);
        }




        //记录吃牌优先级（1最高，最优先）
        int eatTilePriority = 1;
        //每个玩家可以吃的牌的字典，key:玩家索引，value:所有可以吃的牌以及吃牌优先级
        List<EatTileOperation> eatTileOperationList = new List<EatTileOperation>();



        //吃牌并听牌 或 碰牌并听牌 判断
        foreach (int checkPlayerId in checkPlayerIdList)
        {
            List<MahJongType> listeningMagJongTypeList = new List<MahJongType>();
            //可以右吃牌并听牌
            if (CheckRightEatAndListening(checkPlayerId, mahJongType, out List<MahJongType> fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //可以右吃牌
                eatTileOperationList.Add(
                    new EatTileOperation
                    {
                        fromPlayerId = playerId,
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.RightEatAndListening,
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }
            //可以中吃牌并听牌
            if (CheckMiddleEatAndListening(checkPlayerId, mahJongType, out fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //可以中吃牌
                eatTileOperationList.Add(
                    new EatTileOperation
                    {
                        fromPlayerId = playerId,
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.MiddleEatAndListening,
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }

            //可以左吃牌并听牌
            if (CheckLeftEatAndListening(checkPlayerId, mahJongType, out fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //可以左吃牌
                eatTileOperationList.Add(
                    new EatTileOperation
                    {
                        fromPlayerId = playerId,
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.LeftEatAndListening,
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }
            //可以碰牌并听牌
            if (CheckTouchAndListening(checkPlayerId, mahJongType, out fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //可以碰牌
                eatTileOperationList.Add(
                    new EatTileOperation
                    {
                        fromPlayerId = playerId,
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.TouchAndListening,
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }

        }



        //杠牌判断
        foreach (int checkPlayerId in checkPlayerIdList)
        {
            if (CheckGang(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out List<MahJongType> fitEatMahJongTypeList))//可以碰牌
            {
                //可以碰牌
                eatTileOperationList.Add(
                    new EatTileOperation 
                    { 
                        fromPlayerId = playerId, 
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.Gang, 
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }

        }

        //碰牌判断
        foreach (int checkPlayerId in checkPlayerIdList)
        {
            if (CheckTouch(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out List<MahJongType> fitEatMahJongTypeList))//可以碰牌
            {
                //可以碰牌
                eatTileOperationList.Add(
                    new EatTileOperation
                    {
                        fromPlayerId = playerId,
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.Touch,
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }


        }

        //吃牌判断
        foreach (int checkPlayerId in checkPlayerIdList)
        {
            if (checkPlayerId == playerId % 4 + 1) //是下一个玩家
            {

                if (CheckRightEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out List<MahJongType> fitEatMahJongTypeList))//可以右吃牌
                {
                    //可以右吃牌
                    eatTileOperationList.Add(
                        new EatTileOperation
                        {
                            fromPlayerId = playerId,
                            toPlayerId = checkPlayerId,
                            eatTileType = EatTileType.RightEat,
                            fitEatMahJongTypeList = fitEatMahJongTypeList,
                            mahJongType = mahJongType,
                            eatTilePriority = eatTilePriority,
                        });
                    //优先级加1
                    eatTilePriority++;
                }
                if (CheckMiddleEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))//可以中吃牌
                {
                    //可以中吃牌
                    eatTileOperationList.Add(
                        new EatTileOperation
                        {
                            fromPlayerId = playerId,
                            toPlayerId = checkPlayerId,
                            eatTileType = EatTileType.MiddleEat,
                            fitEatMahJongTypeList = fitEatMahJongTypeList,
                            mahJongType = mahJongType,
                            eatTilePriority = eatTilePriority,
                        });
                    //优先级加1
                    eatTilePriority++;
                }

                if (CheckLeftEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))//可以左吃牌
                {
                    //可以左吃牌
                    eatTileOperationList.Add(
                        new EatTileOperation
                        {
                            fromPlayerId = playerId,
                            toPlayerId = checkPlayerId,
                            eatTileType = EatTileType.LeftEat,
                            fitEatMahJongTypeList = fitEatMahJongTypeList,
                            mahJongType = mahJongType,
                            eatTilePriority = eatTilePriority,
                        });
                    //优先级加1
                    eatTilePriority++;
                }
            }


        }
        if(eatTileOperationList.Count == 0)//没有需要吃牌的玩家
        {
            return false;
        }

        //清空吃牌操作回调池
        TotalNumberOfOperationsCurrentlyInProgress.Clear();

        //对每个需要吃的牌进行操作（是否要吃？）
        foreach (EatTileOperation eatTileOperation in eatTileOperationList)
        {
            //呼叫事件，有牌吃了[!重要代码]
            EventHandler.CallHaveTileToEat(eatTileOperation);         
        }

        //开始监听吃牌操作
        StartCoroutine(EatTileRoundCallbackCoroutine(eatTilePriority - 1));

        return true;

    }

    #region 检查对应吃牌类型是否有可以吃的牌

    /// <summary>
    /// 检查是否有左吃牌并听牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <param name="fitEatMahJongTypeList"></param>
    /// <param name="listeningMagJongTypeList">打出哪张牌可以听牌</param>
    /// <returns></returns>
    private bool CheckLeftEatAndListening(int checkPlayerId, MahJongType mahJongType, out List<MahJongType> fitEatMahJongTypeList, out List<MahJongType> listeningMagJongTypeList)
    {
        //新建返回数据
        listeningMagJongTypeList = new List<MahJongType>();
        //有右吃牌
        if (CheckLeftEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))
        {
            //需要排除的麻将列表
            List<MahJongType> exceptMahJongTypeList = fitEatMahJongTypeList.ToList();
            //手牌
            List<MahJongType> playTiles = MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].tiles.ToList();
            //排除手牌
            exceptMahJongTypeList.ForEach(a => playTiles.Remove(a));
            //检查听牌
            foreach (MahJongType checkMahJongType in playTiles.ToList())
            {
                //先移除判断是否可以听牌
                playTiles.Remove(checkMahJongType);
                if (ListenManager.CheckListening(playTiles, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].eatTiles, out _))
                {
                    //打出checkMahJongType可以听牌
                    listeningMagJongTypeList.Add(checkMahJongType);
                }
                //再加回来
                playTiles.Add(checkMahJongType);
            }
        }
        if (listeningMagJongTypeList.Count != 0)
        {
            string listeningLog = "{";
            listeningMagJongTypeList.ForEach(a => listeningLog += a.ToString() + ",");
            listeningLog += "}";
            print($"玩家{checkPlayerId}有左吃牌并可以听牌！吃牌为{Enum.GetName(typeof(MahJongType), mahJongType)}，需打出{listeningLog}可以听牌");
        }


        return listeningMagJongTypeList.Count == 0 ? false : true;
    }

    /// <summary>
    /// 检查是否有中吃牌并听牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <param name="fitEatMahJongTypeList"></param>
    /// <param name="listeningMagJongTypeList">打出哪张牌可以听牌</param>
    /// <returns></returns>
    private bool CheckMiddleEatAndListening(int checkPlayerId, MahJongType mahJongType, out List<MahJongType> fitEatMahJongTypeList, out List<MahJongType> listeningMagJongTypeList)
    {
        //新建返回数据
        listeningMagJongTypeList = new List<MahJongType>();
        //有右吃牌
        if (CheckMiddleEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))
        {
            //需要排除的麻将列表
            List<MahJongType> exceptMahJongTypeList = fitEatMahJongTypeList.ToList();
            //手牌
            List<MahJongType> playTiles = MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].tiles.ToList();
            //排除手牌
            exceptMahJongTypeList.ForEach(a => playTiles.Remove(a));
            //检查听牌
            foreach (MahJongType checkMahJongType in playTiles.ToList())
            {
                //先移除判断是否可以听牌
                playTiles.Remove(checkMahJongType);
                if (ListenManager.CheckListening(playTiles, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].eatTiles, out _))
                {
                    //打出checkMahJongType可以听牌
                    listeningMagJongTypeList.Add(checkMahJongType);
                }
                //再加回来
                playTiles.Add(checkMahJongType);
            }
        }

        if (listeningMagJongTypeList.Count != 0)
        {
            string listeningLog = "{";
            listeningMagJongTypeList.ForEach(a => listeningLog += a.ToString() + ",");
            listeningLog += "}";
            print($"玩家{checkPlayerId}有中吃牌并可以听牌！吃牌为{Enum.GetName(typeof(MahJongType), mahJongType)}，需打出{listeningLog}可以听牌");
        }

        return listeningMagJongTypeList.Count == 0 ? false : true;
    }

    /// <summary>
    /// 检查是否有右吃牌并听牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <param name="fitEatMahJongTypeList"></param>
    /// <param name="listeningMagJongTypeList">打出哪张牌可以听牌</param>
    /// <returns></returns>
    private bool CheckRightEatAndListening(int checkPlayerId, MahJongType mahJongType, out List<MahJongType> fitEatMahJongTypeList, out List<MahJongType> listeningMagJongTypeList)
    {
        //新建返回数据
        listeningMagJongTypeList = new List<MahJongType>();
        //有右吃牌
        if (CheckRightEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))
        {
            //需要排除的麻将列表
            List<MahJongType> exceptMahJongTypeList = fitEatMahJongTypeList.ToList();
            //手牌
            List<MahJongType> playTiles = MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].tiles.ToList();
            //排除手牌
            exceptMahJongTypeList.ForEach(a => playTiles.Remove(a));
            //检查听牌
            foreach (MahJongType checkMahJongType in playTiles.ToList())
            {
                //先移除判断是否可以听牌
                playTiles.Remove(checkMahJongType);
                if (ListenManager.CheckListening(playTiles, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].eatTiles, out _))
                {
                    //打出checkMahJongType可以听牌
                    listeningMagJongTypeList.Add(checkMahJongType);
                }
                //再加回来
                playTiles.Add(checkMahJongType);
            }
        }

        if (listeningMagJongTypeList.Count != 0)
        {
            string listeningLog = "{";
            listeningMagJongTypeList.ForEach(a => listeningLog += a.ToString() + ",");
            listeningLog += "}";
            print($"玩家{checkPlayerId}有右吃牌并可以听牌！吃牌为{Enum.GetName(typeof(MahJongType), mahJongType)}，需打出{listeningLog}可以听牌");
        }

        return listeningMagJongTypeList.Count == 0 ? false : true;
    }

    /// <summary>
    /// 检查是否有碰牌并听牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <param name="fitEatMahJongTypeList"></param>
    /// <param name="listeningMagJongTypeList">打出哪张牌可以听牌</param>
    /// <returns></returns>
    private bool CheckTouchAndListening(int checkPlayerId, MahJongType mahJongType, out List<MahJongType> fitEatMahJongTypeList, out List<MahJongType> listeningMagJongTypeList)
    {
        //新建返回数据
        listeningMagJongTypeList = new List<MahJongType>();
        //有碰牌
        if (CheckTouch(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))
        {
            //需要排除的麻将列表
            List<MahJongType> exceptMahJongTypeList = fitEatMahJongTypeList.ToList();
            //手牌
            List<MahJongType> playTiles = MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].tiles.ToList();
            //排除手牌
            exceptMahJongTypeList.ForEach(a => playTiles.Remove(a));
            //吃牌
            List<List<MahJongType>> eatTiles = MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].eatTiles.ToList();
            //添加一组吃牌
            eatTiles.Add(fitEatMahJongTypeList.Concat(new List<MahJongType> { mahJongType }).ToList());
            //检查听牌
            foreach (MahJongType checkMahJongType in playTiles.ToList())
            {
                //先移除判断是否可以听牌
                playTiles.Remove(checkMahJongType);
                if (ListenManager.CheckListening(playTiles, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].eatTiles, out _))
                {
                    //打出checkMahJongType可以听牌
                    listeningMagJongTypeList.Add(checkMahJongType);
                }
                //再加回来
                playTiles.Add(checkMahJongType);
            }
        }

        if (listeningMagJongTypeList.Count != 0)
        {
            string listeningLog = "{";
            listeningMagJongTypeList.ForEach(a => listeningLog += a.ToString() + ",");
            listeningLog += "}";
            print($"玩家{checkPlayerId}有碰牌并可以听牌！碰牌为{Enum.GetName(typeof(MahJongType), mahJongType)}，需打出{listeningLog}可以听牌");
        }

        return listeningMagJongTypeList.Count == 0 ? false : true;
    }



    /// <summary>
    /// 检查是否有左吃牌，例1：有四万、五万，吃三万，例2：有二条、三条，吃一条。
    /// </summary>
    /// <param name="checkPlayerId">要吃牌的玩家Id</param>
    /// <param name="mahJongType">吃的哪张牌</param>
    /// <returns></returns>
    public bool CheckLeftEat(MahJongType mahJongType, MahJongTiles mahJongTiles , out List<MahJongType> fitEatMahJongTypeList)
    {
        //初始化配合吃牌的牌列表
        fitEatMahJongTypeList = new List<MahJongType>();
        if (int.TryParse(Enum.GetName(typeof(MahJongType), mahJongType).Last().ToString(), out int number))//是数字牌
        {
            if (number < 8)
            {
                MahJongType mahJongType1 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType + 1);
                MahJongType mahJongType2 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType + 2);
                if (mahJongType1 != default && mahJongType2 != default)
                {
                    //Console.WriteLine($"玩家{checkPlayerId}有左吃牌！吃牌为{Enum.GetName(typeof(MahJongType), mahJongType)}");
                    //传回配合吃牌的牌
                    fitEatMahJongTypeList.Add(mahJongType1);
                    fitEatMahJongTypeList.Add(mahJongType2);
                    //可以左吃牌
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 检查是否有中吃牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType">吃的哪张牌</param>
    /// <returns></returns>
    public bool CheckMiddleEat(MahJongType mahJongType, MahJongTiles mahJongTiles, out List<MahJongType> fitEatMahJongTypeList)
    {
        //初始化配合吃牌的牌列表
        fitEatMahJongTypeList = new List<MahJongType>();
        if (int.TryParse(Enum.GetName(typeof(MahJongType), mahJongType).Last().ToString(), out int number))//是数字牌
        {
            if (number > 1 && number < 9)
            {
                MahJongType mahJongType1 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType - 1);
                MahJongType mahJongType2 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType + 1);
                if (mahJongType1 != default && mahJongType2 != default)
                {
                    //Console.WriteLine($"玩家{checkPlayerId}有中吃牌！吃牌为{Enum.GetName(typeof(MahJongType), mahJongType)}");
                    //传回配合吃牌的牌
                    fitEatMahJongTypeList.Add(mahJongType1);
                    fitEatMahJongTypeList.Add(mahJongType2);
                    //可以中吃牌
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 检查是否有右吃牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <param name="fitEatMahJongTypeList">配合吃牌的牌列表</param>
    /// <returns></returns>
    public bool CheckRightEat(MahJongType mahJongType, MahJongTiles mahJongTiles, out List<MahJongType> fitEatMahJongTypeList)
    {
        //初始化配合吃牌的牌列表
        fitEatMahJongTypeList = new List<MahJongType>();
        if (int.TryParse(Enum.GetName(typeof(MahJongType), mahJongType).Last().ToString(), out int number))//是数字牌
        {
            if (number > 2)
            {
                MahJongType mahJongType1 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType - 1);
                MahJongType mahJongType2 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType - 2);
                if (mahJongType1 != default && mahJongType2 != default)
                {
                    //Console.WriteLine($"玩家{checkPlayerId}有右吃牌！吃牌为{Enum.GetName(typeof(MahJongType), mahJongType)}");
                    //传回配合吃牌的牌
                    fitEatMahJongTypeList.Add(mahJongType1);
                    fitEatMahJongTypeList.Add(mahJongType2);
                    //可以右吃牌
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 检查是否有碰牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <returns></returns>
    public bool CheckTouch(MahJongType mahJongType, MahJongTiles mahJongTiles, out List<MahJongType> fitEatMahJongTypeList)
    {
        //初始化配合吃牌的牌列表
        fitEatMahJongTypeList = new List<MahJongType>();
        if (mahJongTiles.tiles.Count(p => p == mahJongType) == 2)
        {
            //Console.WriteLine($"玩家{checkPlayerId}有碰牌！碰牌为{Enum.GetName(typeof(MahJongType), mahJongType)}");
            //传回配合吃牌的牌
            fitEatMahJongTypeList.Add(mahJongType);
            fitEatMahJongTypeList.Add(mahJongType);
            //可以碰牌
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查是否有杠牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <returns></returns>
    private bool CheckGang(MahJongType mahJongType, MahJongTiles mahJongTiles, out List<MahJongType> fitEatMahJongTypeList)
    {
        //初始化配合吃牌的牌列表
        fitEatMahJongTypeList = new List<MahJongType>();
        if (mahJongTiles.tiles.Count(p => p == mahJongType) == 3)
        {
            //Console.WriteLine($"玩家{checkPlayerId}有杠牌！杠牌为{Enum.GetName(typeof(MahJongType), mahJongType)}");
            //传回配合吃牌的牌
            fitEatMahJongTypeList.Add(mahJongType);
            fitEatMahJongTypeList.Add(mahJongType);
            fitEatMahJongTypeList.Add(mahJongType);
            //可以杠牌
            return true;
        }

        return false;
    }

    /// <summary>
    /// 调试用，输出树的所有数据
    /// </summary>
    /// <param name="layerCount"></param>
    public void PrintTree(int playerId)
    {
        string log = string.Empty;
        log += $"玩家{playerId}-----\n";
        bool needLog = false;
        //输出整个树（调试用）
        for (int i = 1; i < 5; i++)
        {
            foreach (TreeNode<List<MahJongType>> node in TreeNode<List<MahJongType>>.GetNodesAtLayer(i))
            {
                string tiles = string.Empty;
                //获取节点所在枝上的所有麻将
                TreeNode<List<MahJongType>> exceptNode = node;
                for (int j = 0; j < i; j++)
                {
                    tiles += $"层级{i - j}:{{";
                    exceptNode.Data.ForEach(a => tiles += a.ToString() + ",");
                    tiles += "},";
                    exceptNode = exceptNode.Parent;
                }
                needLog = true;
                log += tiles + "\n";
            }
        }
        if (needLog)
            Debug.Log(log);
    }



    #endregion

    /// <summary>
    /// 吃牌回合的回调函数，一直运行直到所有玩家结束吃牌回合
    /// </summary>
    /// <param name="totalOperationCount">吃牌操作的计数</param>
    /// <returns>协程</returns>
    private IEnumerator EatTileRoundCallbackCoroutine(int totalOperationCount)
    {
        while (totalOperationCount != TotalNumberOfOperationsCurrentlyInProgress.Count)
        {
            //等待操作完成
            yield return null;
        }

        List<EatTileOperation> sortedOperations = TotalNumberOfOperationsCurrentlyInProgress.OrderBy(p=>p.eatTilePriority).ToList();

        foreach (EatTileOperation op in sortedOperations)
        {
            if (op.whetherToEat == true)
            {
                //对应玩家吃牌操作
                MahJongTilesManager.Instance.EatTile(op);

                print($"玩家{op.toPlayerId}吃了玩家{op.fromPlayerId}的牌，吃牌方式：{op.eatTileType}，牌：{op.mahJongType}");

                //玩家继续出牌
                GameManager.Instance.Players[op.toPlayerId].PlayingTile();

                yield break;
            }
        }

        //所有玩家都不吃牌
        GameManager.Instance.PlayerPlayedTile(sortedOperations[0].fromPlayerId, sortedOperations[0].mahJongType);

        yield break;
    }



}

public struct EatTileOperation
{
    /// <summary>出牌的玩家</summary>
    public int fromPlayerId;
    /// <summary>吃牌的玩家</summary>
    public int toPlayerId;
    /// <summary>哪张麻将牌</summary>
    public MahJongType mahJongType;
    /// <summary>配合吃牌的牌，例：吃二万，那配合吃牌的牌就是一万和三万两张牌。</summary>
    public List<MahJongType> fitEatMahJongTypeList;
    /// <summary>吃牌的方式</summary>
    public EatTileType eatTileType;
    /// <summary>吃牌优先级</summary>
    public int eatTilePriority;

    /// <summary>是否吃牌？仅由玩家传回此类时有用</summary>
    public bool whetherToEat;
}


public enum EatTileType
{
    None,
    /// <summary>左吃牌，例：自己有8万、9万，别人打出了7万</summary>
    LeftEat,
    /// <summary>中吃牌，例：自己有4万、6万，别人打出了5万</summary>
    MiddleEat,
    /// <summary>右吃牌，例：自己有1万、2万，别人打出了3万</summary>
    RightEat,
    /// <summary>碰牌</summary>
    Touch,
    /// <summary>杠牌</summary>
    Gang,

    /// <summary>左吃牌，并可以听牌</summary>
    LeftEatAndListening,
    /// <summary>中吃牌，并可以听牌</summary>
    MiddleEatAndListening,           
    /// <summary>右吃牌，并可以听牌</summary>
    RightEatAndListening,
    /// <summary>碰牌，并可以听牌</summary>
    TouchAndListening,
    /// <summary>杠牌，并可以听牌</summary>
    GangAndListening,


}

/// <summary>
/// 用于玩家胡牌判断的数据类
/// </summary>
public class WinTileCheckData
{
    /// <summary>胡牌项的列表</summary>
    public List<WinTileItem> winTileItemList = new List<WinTileItem>();

    public struct WinTileItem
    {
        /// <summary>胡的哪张牌</summary>
        public MahJongType mahJongType;
        /// <summary>胡牌的倍数（番数）</summary>
        public int factor;
    }
}



/// <summary>
/// 听牌的麻将的树结构
/// </summary>
/// <typeparam name="T"></typeparam>
public class TreeNode<T>
{
    public T Data { get; set; }

    /// <summary>当前节点的父节点</summary>
    public TreeNode<T> Parent { get; set; }
    //public List<TreeNode<T>> Children { get; set; }

    public TreeNode(T data)
    {
        this.Data = data;
        allTreeNodeList.Add(this);
    }

    /// <summary>
    /// 获取当前节点的层数
    /// </summary>
    /// <returns></returns>
    public int GetNodeLayer()
    {
        int layer = 0;

        TreeNode<T> temp = this;

        while (temp.Parent != null)
        {
            layer++;
            temp = temp.Parent;
        }

        return layer;
    }


    /// <summary>保存所有节点的列表</summary>
    public static List<TreeNode<T>> allTreeNodeList = new List<TreeNode<T>>();

    /// <summary>
    /// 获取树的某一层的所有节点
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static List<TreeNode<T>> GetNodesAtLayer(int layer)
    {
        List<TreeNode<T>> treeNodes = new List<TreeNode<T>>();

        foreach (TreeNode<T> node in allTreeNodeList)
        {
            if(node.GetNodeLayer() == layer)
            {
                treeNodes.Add(node);
            }
        }

        return treeNodes;
    }





}