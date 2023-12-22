using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 听牌的管理类，包含与听牌有关的所有操作
/// </summary>
public static class ListenManager
{
    /// <summary>
    /// 听牌判断
    /// </summary>
    /// <param name="mahJongTypeList">手牌</param>
    /// <param name="eatMahJongTypeList">吃过的牌</param>
    /// <param name="listeningTilesData">胡牌数据（输出值）</param>
    /// <returns></returns>
    public static bool CheckListening(List<MahJongType> mahJongTypeList, List<List<MahJongType>> eatMahJongTypeList, out ListeningTilesData listeningTilesData)
    {
        //清空树
        TreeNode<List<MahJongType>>.allTreeNodeList.Clear();

        //新建听牌判断的树结构
        TreeNode<List<MahJongType>> listeningTreeNode = new TreeNode<List<MahJongType>>(new List<MahJongType> { MahJongType.None });


        //新建听牌数据
        listeningTilesData = new ListeningTilesData();
        //设置吃了的牌列表
        listeningTilesData.eatMahJongTypeList = eatMahJongTypeList;
        listeningTilesData.listenItemList = new List<ListenItem>();


        if (eatMahJongTypeList.Count == 0)//没有吃牌（没开门）
        {
            //不能听牌
            return false;
        }

        #region 判断是否有幺九
        //获取全部的牌（手牌+吃牌）
        List<MahJongType> wholeMahJongTypeList = mahJongTypeList.ToList();
        eatMahJongTypeList.ForEach(a => wholeMahJongTypeList.AddRange(a));

        if (wholeMahJongTypeList.FirstOrDefault(
            p =>
            p == MahJongType.Circle1 || p == MahJongType.Circle9
            || p == MahJongType.Stick1 || p == MahJongType.Stick9
            || p == MahJongType.Thousand1 || p == MahJongType.Thousand9
            || p == MahJongType.RedDragon
            ) == default)
        {
            //没有幺九
            return false;
        }


        #endregion

        //树总共的层数（（手牌数量 - 1）/ 3）
        int layerCount = (mahJongTypeList.Count - 1) / 3;

        if (mahJongTypeList.Count != 1
            && mahJongTypeList.Count != 4
            && mahJongTypeList.Count != 7
            && mahJongTypeList.Count != 10
            && mahJongTypeList.Count != 13
            )
        {
            Console.WriteLine("错误！判断听时，麻将牌数量错误！");
        }

        #region 判断顺子或刻子是否完整

        /*
         * 生成判断树，
         * 例：layerCount:4, 层级4:{Stick1,Stick2,Stick3,},层级3:{Circle7,Circle8,Circle9,},层级2:{Circle7,Circle8,Circle9,},层级1:{Circle7,Circle8,Circle9,},
         * 例：layerCount:3, 层级2:{Thousand4,Thousand5,Thousand6,},层级1:{Stick4,Stick5,Stick6,},（只有两层）
         */
        for (int i = 0; i < layerCount; i++)
        {
            foreach (TreeNode<List<MahJongType>> nodeAtLayer in TreeNode<List<MahJongType>>.GetNodesAtLayer(i))
            {
                //获取除去树节点元素之外剩余的元素
                List<MahJongType> remainingMahJongList = mahJongTypeList.ToList();
                //遍历当前树枝
                TreeNode<List<MahJongType>> exceptNode = nodeAtLayer;
                for (int j = 0; j < i; j++)
                {
                    //去除元素
                    exceptNode.Data.ForEach(a => remainingMahJongList.Remove(a));
                    exceptNode = exceptNode.Parent;
                }

                //去除重复元素
                List<MahJongType> remainingMahJongNoRepeatList = remainingMahJongList.ToHashSet().ToList();
                //获取最小元素，小于这个元素的麻将不再被检索
                MahJongType minMahJongType = nodeAtLayer.Data.Min();

                foreach (MahJongType mahJongType in remainingMahJongNoRepeatList)
                {
                    //小于minMahJongType的麻将不再被检索
                    if (mahJongType < minMahJongType) continue;

                    if (int.TryParse(Enum.GetName(typeof(MahJongType), mahJongType).Last().ToString(), out int number))//是数字牌
                    {
                        //顺子
                        if (number < 8)
                        {
                            MahJongType mahJongType1 = remainingMahJongList.FirstOrDefault(p => p == mahJongType + 1);
                            MahJongType mahJongType2 = remainingMahJongList.FirstOrDefault(p => p == mahJongType + 2);
                            if (mahJongType1 != default && mahJongType2 != default)
                            {
                                TreeNode<List<MahJongType>> node = new TreeNode<List<MahJongType>>(new List<MahJongType>
                                {
                                    mahJongType, mahJongType1, mahJongType2,
                                });

                                //添加到节点
                                node.Parent = nodeAtLayer;

                                continue;
                            }
                        }
                    }
                    //刻子
                    if (remainingMahJongList.Count(p => p == mahJongType) == 3)
                    {
                        TreeNode<List<MahJongType>> node = new TreeNode<List<MahJongType>>(new List<MahJongType>
                                {
                                    mahJongType, mahJongType, mahJongType,
                                });

                        //添加到节点
                        node.Parent = nodeAtLayer;

                        continue;
                    }
                }
            }
        }

        #endregion




        //获取最后两层的麻将列表：分别为胡牌为单张牌（只剩一张麻将）、胡牌为多张牌（剩四张麻将）
        List<TreeNode<List<MahJongType>>> singleTileLayerNodes = TreeNode<List<MahJongType>>.GetNodesAtLayer(layerCount);
        List<TreeNode<List<MahJongType>>> mutiTileLayerNodes = TreeNode<List<MahJongType>>.GetNodesAtLayer(layerCount - 1);
        //遍历找到可以听牌的牌型（胡单张牌）
        foreach (TreeNode<List<MahJongType>> singleTileLayerNode in singleTileLayerNodes)
        {
            //获取除去树节点元素之外剩余的元素
            List<MahJongType> remainingMahJongList = mahJongTypeList.ToList();
            //暂存所有癞子牌
            List<List<MahJongType>> wildMahJongTypeList = new List<List<MahJongType>>();
            //遍历当前树枝
            TreeNode<List<MahJongType>> exceptNode = singleTileLayerNode;
            for (int j = 0; j < layerCount; j++)
            {
                //去除元素
                exceptNode.Data.ForEach(a => remainingMahJongList.Remove(a));
                //添加到癞子牌中
                wildMahJongTypeList.Add(exceptNode.Data);
                exceptNode = exceptNode.Parent;
            }

            //没有癞子
            if (HasWildTile(eatMahJongTypeList, singleTileLayerNode, remainingMahJongList) == false) continue;


            //判断剩余牌数是否为1[*不重要代码]
            if (remainingMahJongList.Count != 1)
            {
                Console.WriteLine("只胡一张牌，剩余牌数应等于1");
            }

            Console.WriteLine($"胡牌为单张，胡牌为{remainingMahJongList[0]}");

            //设置胡牌数据（刮大风）
            foreach (MahJongType mahJongType in GetTripletList(eatMahJongTypeList, wildMahJongTypeList))
            {
                listeningTilesData.listenItemList.Add(new ListenItem
                {
                    listenType = ListenType.StrongWind,
                    wildMahJongTypeList = wildMahJongTypeList,
                    listenTiles = new List<MahJongType> { mahJongType, mahJongType, mahJongType },
                    otherTiles = new List<MahJongType>(),
                    winTiles = new List<MahJongType> { mahJongType },
                });
            }



            //设置胡牌数据
            listeningTilesData.listenItemList.Add(new ListenItem
            {
                listenType = ListenType.SingleTile,
                wildMahJongTypeList = wildMahJongTypeList,
                listenTiles = new List<MahJongType> { remainingMahJongList[0] },
                otherTiles = new List<MahJongType>(),
                winTiles = new List<MahJongType> { remainingMahJongList[0] },
            });
        }


        //遍历找到可以听牌的牌型（胡多张牌）
        foreach (TreeNode<List<MahJongType>> mutiTileLayerNode in mutiTileLayerNodes)
        {

            //获取除去树节点元素之外剩余的元素
            List<MahJongType> remainingMahJongList = mahJongTypeList.ToList();
            //暂存所有癞子牌
            List<List<MahJongType>> wildMahJongTypeList = new List<List<MahJongType>>();
            //遍历当前树枝
            TreeNode<List<MahJongType>> exceptNode = mutiTileLayerNode;
            //遍历树节点
            for (int j = 0; j < layerCount - 1; j++)
            {
                //去除元素
                exceptNode.Data.ForEach(a => remainingMahJongList.Remove(a));
                //添加到癞子牌中
                wildMahJongTypeList.Add(exceptNode.Data);
                exceptNode = exceptNode.Parent;
            }


            //没有癞子
            if (HasWildTile(eatMahJongTypeList, mutiTileLayerNode, remainingMahJongList) == false) continue;

            //判断
            //先排序
            remainingMahJongList.Sort();
            if (remainingMahJongList.ToHashSet().Count == 1 //四张相同的牌
                || remainingMahJongList.ToHashSet().Count == 4 //四张不同的牌
                )
            {
                //不能胡
                continue;
            }
            //判断1：两个对
            if (remainingMahJongList.Count(p => p == remainingMahJongList[0]) == 2 && remainingMahJongList.Count(p => p == remainingMahJongList[2]) == 2)
            {
                //设置胡牌数据
                listeningTilesData.listenItemList.Add(new ListenItem
                {
                    listenType = ListenType.TwoDoubleTile,
                    wildMahJongTypeList = wildMahJongTypeList,
                    listenTiles = new List<MahJongType> { remainingMahJongList[0], remainingMahJongList[1] },
                    otherTiles = new List<MahJongType> { remainingMahJongList[2], remainingMahJongList[3] },
                    winTiles = new List<MahJongType> { remainingMahJongList[0], remainingMahJongList[2] },
                });
                //设置胡牌数据（刮大风）
                foreach (MahJongType mahJongType in GetTripletList(eatMahJongTypeList, wildMahJongTypeList))
                {
                    listeningTilesData.listenItemList.Add(new ListenItem
                    {
                        listenType = ListenType.StrongWind,
                        wildMahJongTypeList = wildMahJongTypeList,
                        listenTiles = new List<MahJongType> { mahJongType, mahJongType, mahJongType },
                        otherTiles = new List<MahJongType>(),
                        winTiles = new List<MahJongType> { mahJongType },
                    });
                }
            }
            //判断2：一个对，一个连
            List<MahJongType> suitMahJongTypeList = new List<MahJongType>();
            //获取能胡牌的牌
            foreach (MahJongType mahJongType in remainingMahJongList)
            {
                if (remainingMahJongList.Count(p => p == mahJongType) == 1)
                {
                    suitMahJongTypeList.Add(mahJongType);
                }
            }
            //先排序
            suitMahJongTypeList.Sort();
            if (suitMahJongTypeList.Count == 1)//两个连牌中有一个牌和对牌的牌一样，例：四万、四万、四万、五万，胡三万、六万、五万
            {
                //三个一样的牌（刻子），例：四万
                MahJongType tripletMahJongType = remainingMahJongList.Except(suitMahJongTypeList).ToList()[0];
                //单张牌，例：五万
                MahJongType suitMahJongType = suitMahJongTypeList[0];

                if (int.TryParse(Enum.GetName(typeof(MahJongType), tripletMahJongType).Last().ToString(), out int number1)
                        && int.TryParse(Enum.GetName(typeof(MahJongType), suitMahJongType).Last().ToString(), out int number2))//是数字牌
                {
                    //顺子
                    if ((Math.Abs(tripletMahJongType - suitMahJongType) == 1 && Math.Abs(number1 - number2) == 1))
                    {
                        if (Math.Min(number1, number2) == 1 || Math.Max(number1, number2) == 9)//胡一张（夹），例1：一万、二万，胡：三万；例2：八万、九万，胡：七万。
                        {
                            //设置胡牌数据（夹）
                            listeningTilesData.listenItemList.Add(new ListenItem
                            {
                                listenType = ListenType.SandwichWay,
                                wildMahJongTypeList = wildMahJongTypeList,
                                listenTiles = new List<MahJongType> { tripletMahJongType, suitMahJongType },
                                otherTiles = new List<MahJongType> { tripletMahJongType, tripletMahJongType },
                                winTiles = Math.Min(number1, number2) == 1 ?
                                new List<MahJongType> { (MahJongType)(Math.Max((int)tripletMahJongType, (int)suitMahJongType) + 1) } :
                                new List<MahJongType> { (MahJongType)(Math.Min((int)tripletMahJongType, (int)suitMahJongType) - 1) }
                                ,
                            });
                        }
                        else//胡两张（边）,例：六万、七万，胡：五万、八万。
                        {
                            listeningTilesData.listenItemList.Add(new ListenItem
                            {
                                listenType = ListenType.SideWay,
                                wildMahJongTypeList = wildMahJongTypeList,
                                listenTiles = new List<MahJongType> { tripletMahJongType, suitMahJongType },
                                otherTiles = new List<MahJongType> { tripletMahJongType, tripletMahJongType },
                                winTiles = new List<MahJongType> {
                                       (MahJongType)(Math.Max((int)tripletMahJongType, (int)suitMahJongType) + 1),                  (MahJongType)(Math.Min((int)tripletMahJongType, (int)suitMahJongType) - 1)
                                    },
                            });
                        }
                        //设置胡牌数据（刮大风）
                        foreach (MahJongType mahJongType in GetTripletList(eatMahJongTypeList, wildMahJongTypeList))
                        {
                            listeningTilesData.listenItemList.Add(new ListenItem
                            {
                                listenType = ListenType.StrongWind,
                                wildMahJongTypeList = wildMahJongTypeList,
                                listenTiles = new List<MahJongType> { mahJongType, mahJongType, mahJongType },
                                otherTiles = new List<MahJongType>(),
                                winTiles = new List<MahJongType> { mahJongType },
                            });
                        }
                    }
                    else if (Math.Abs(tripletMahJongType - suitMahJongType) == 2 && Math.Abs(number1 - number2) == 2)//胡一张（夹），例：四万、六万，胡：五万
                    {
                        //设置胡牌数据
                        listeningTilesData.listenItemList.Add(new ListenItem
                        {
                            listenType = ListenType.SandwichWay,
                            wildMahJongTypeList = wildMahJongTypeList,
                            listenTiles = new List<MahJongType> { tripletMahJongType, suitMahJongType },
                            otherTiles = new List<MahJongType> { tripletMahJongType, tripletMahJongType },
                            winTiles = new List<MahJongType> {
                                       (MahJongType)(((int)tripletMahJongType + (int)suitMahJongType) / 2)
                                    },
                        });
                        //设置胡牌数据（刮大风）
                        foreach (MahJongType mahJongType in GetTripletList(eatMahJongTypeList, wildMahJongTypeList))
                        {
                            listeningTilesData.listenItemList.Add(new ListenItem
                            {
                                listenType = ListenType.StrongWind,
                                wildMahJongTypeList = wildMahJongTypeList,
                                listenTiles = new List<MahJongType> { mahJongType, mahJongType, mahJongType },
                                otherTiles = new List<MahJongType>(),
                                winTiles = new List<MahJongType> { mahJongType },
                            });
                        }
                    }
                }

            }
            else if (suitMahJongTypeList.Count == 2)//两个连牌中没有牌和对牌的牌一样，例：二万、二万、七万、八万，胡六万、九万
            {
                //两个一样的牌（刻子），例：二万
                MahJongType tripletMahJongType = remainingMahJongList.Except(suitMahJongTypeList).ToList()[0];

                MahJongType suitMahJongType1 = suitMahJongTypeList[0];
                MahJongType suitMahJongType2 = suitMahJongTypeList[1];

                if (int.TryParse(Enum.GetName(typeof(MahJongType), suitMahJongType1).Last().ToString(), out int number1)
                    && int.TryParse(Enum.GetName(typeof(MahJongType), suitMahJongType2).Last().ToString(), out int number2))//是数字牌
                {
                    //顺子
                    if ((Math.Abs(suitMahJongType1 - suitMahJongType2) == 1 && Math.Abs(number1 - number2) == 1))
                    {
                        if (number1 == 1 || number2 == 9)//胡一张（夹），例1：一万、二万，胡：三万；例2：八万、九万，胡：七万。
                        {
                            //设置胡牌数据
                            listeningTilesData.listenItemList.Add(new ListenItem
                            {
                                listenType = ListenType.SandwichWay,
                                wildMahJongTypeList = wildMahJongTypeList,
                                listenTiles = new List<MahJongType> { suitMahJongType1, suitMahJongType2 },
                                otherTiles = new List<MahJongType> { tripletMahJongType, tripletMahJongType },
                                winTiles = new List<MahJongType> {
                                        number1 == 1 ? suitMahJongType2 + 1 : suitMahJongType1 - 1
                                    },
                            });
                        }
                        else//胡两张（边）,例：六万、七万，胡：五万、八万。
                        {
                            //设置胡牌数据
                            listeningTilesData.listenItemList.Add(new ListenItem
                            {
                                listenType = ListenType.SideWay,
                                wildMahJongTypeList = wildMahJongTypeList,
                                listenTiles = new List<MahJongType> { suitMahJongType1, suitMahJongType2 },
                                otherTiles = new List<MahJongType> { tripletMahJongType, tripletMahJongType },
                                winTiles = new List<MahJongType> {
                                         suitMahJongType1 - 1, suitMahJongType2 + 1
                                    },
                            });
                        }
                        //设置胡牌数据（刮大风）
                        foreach (MahJongType mahJongType in GetTripletList(eatMahJongTypeList, wildMahJongTypeList))
                        {
                            listeningTilesData.listenItemList.Add(new ListenItem
                            {
                                listenType = ListenType.StrongWind,
                                wildMahJongTypeList = wildMahJongTypeList,
                                listenTiles = new List<MahJongType> { mahJongType, mahJongType, mahJongType },
                                otherTiles = new List<MahJongType>(),
                                winTiles = new List<MahJongType> { mahJongType },
                            });
                        }

                    }
                    else if (Math.Abs(suitMahJongType1 - suitMahJongType2) == 2 && Math.Abs(number1 - number2) == 2)//胡一张（夹），例：四万、六万，胡：五万
                    {
                        //设置胡牌数据
                        listeningTilesData.listenItemList.Add(new ListenItem
                        {
                            listenType = ListenType.SandwichWay,
                            wildMahJongTypeList = wildMahJongTypeList,
                            listenTiles = new List<MahJongType> { suitMahJongType1, suitMahJongType2 },
                            otherTiles = new List<MahJongType> { tripletMahJongType, tripletMahJongType },
                            winTiles = new List<MahJongType> {
                                         (MahJongType)(((int)suitMahJongType1 + (int)suitMahJongType2) / 2)
                                    },
                        });
                        //设置胡牌数据（刮大风）
                        foreach (MahJongType mahJongType in GetTripletList(eatMahJongTypeList, wildMahJongTypeList))
                        {
                            listeningTilesData.listenItemList.Add(new ListenItem
                            {
                                listenType = ListenType.StrongWind,
                                wildMahJongTypeList = wildMahJongTypeList,
                                listenTiles = new List<MahJongType> { mahJongType, mahJongType, mahJongType },
                                otherTiles = new List<MahJongType>(),
                                winTiles = new List<MahJongType> { mahJongType },
                            });
                        }
                    }
                }
            }

        }
        //有胡牌数据
        if (listeningTilesData.listenItemList.Count != 0)
        {
            //可以听牌
            return true;
        }



        return false;
    }

    /// <summary>
    /// 判断是否有懒子（三张一样的牌）
    /// </summary>
    /// <param name="eatTiles">吃的牌</param>
    /// <param name="node">叶子节点，Count一定为3</param>
    /// <returns></returns>
    private static bool HasWildTile(List<List<MahJongType>> eatTiles, TreeNode<List<MahJongType>> node, List<MahJongType> remainingMahJongList)
    {
        //获取全部的牌
        List<List<MahJongType>> wholeMahJongTypeList = eatTiles.ToList();
        TreeNode<List<MahJongType>> checkNode = node;
        while (checkNode.Parent != null)
        {
            wholeMahJongTypeList.Add(checkNode.Data);
            checkNode = checkNode.Parent;
        }
        //添加能胡牌的牌
        wholeMahJongTypeList.Add(remainingMahJongList);

        //判断是否有红中
        if (wholeMahJongTypeList.FirstOrDefault(p => p.Contains(MahJongType.RedDragon)) != default)
        {
            //有红中，可以当癞子
            return true;
        }
        //判断是否有刻子（三个相同的）
        foreach (List<MahJongType> mahJongTypeList in wholeMahJongTypeList)
        {
            if (mahJongTypeList.Count >= 3 && mahJongTypeList.ToHashSet().Count == 1)//三张牌的节点中有刻子
            {
                return true;
            }
        }

        //判断是否有两个对
        if (wholeMahJongTypeList.Last().Count(p1 => wholeMahJongTypeList.Last().Count(p2 => p2 == p1) == 2) == 4)
        {
            //两个对（两张牌的数量为2）
            return true;
        }

        //没有癞子
        return false;
    }

    /// <summary>
    /// 获取手牌和吃牌中的所有刻子牌（三张一样的牌）
    /// </summary>
    /// <param name="eatMahJongTypeList">吃的牌</param>
    /// <param name="wildMahJongTypeList">手牌中的癞子牌</param>
    /// <returns>刻子牌列表（三张一样的牌），返回一万、三万，即代表有两个刻子：一万、一万、一万，三万、三万、三万。</returns>
    private static List<MahJongType> GetTripletList(List<List<MahJongType>> eatMahJongTypeList, List<List<MahJongType>> wildMahJongTypeList)
    {
        //刻子牌列表
        List<MahJongType> tripletList = new List<MahJongType>();
        //判断是否有刻子（三个相同的）
        foreach (List<MahJongType> mahJongTypeList in eatMahJongTypeList.Concat(wildMahJongTypeList))
        {
            if (mahJongTypeList.Count == 3 && mahJongTypeList.ToHashSet().Count == 1)//三张牌的节点中有刻子
            {
                tripletList.Add(mahJongTypeList[0]);
            }
        }
        return tripletList;
    }


    #region 检查是否有可以吃牌并听牌的操作

    /// <summary>委托字典</summary>
    private static Dictionary<EatTileType, EatCheckDelegate> eatTileCheck = new Dictionary<EatTileType, EatCheckDelegate>()
        {
            {EatTileType.LeftEat, EatTileManager.Instance.CheckLeftEat },
            {EatTileType.MiddleEat, EatTileManager.Instance.CheckMiddleEat },
            {EatTileType.RightEat, EatTileManager.Instance.CheckRightEat },
            {EatTileType.Touch, EatTileManager.Instance.CheckTouch },
        };

    /// <summary>吃牌检查委托</summary>
    private delegate bool EatCheckDelegate(MahJongType mahJongType, MahJongTiles mahJongTiles
    , out List<MahJongType> fitEatMahJongTypeList);


    /// <summary>
    /// 判断是否有吃牌并听牌的操作
    /// </summary>
    /// <param name="eatTileType">吃牌方式</param>
    /// <param name="mahJongType">吃的哪张牌</param>
    /// <param name="mahJongTiles">玩家的麻将列表</param>
    /// <param name="fitEatMahJongTypeList">配合吃牌的牌</param>
    /// <param name="listeningMagJongTypeList">打出哪张牌可以听牌</param>
    /// <returns>是否可以吃&听</returns>
    public static bool CheckEatAndListening(EatTileType eatTileType, MahJongType mahJongType, MahJongTiles mahJongTiles
        , out List<MahJongType> fitEatMahJongTypeList, out List<MahJongType> listeningMagJongTypeList)
    {
        //新建返回数据
        listeningMagJongTypeList = new List<MahJongType>();
        //判断是否有对应类型的吃牌
        if (eatTileCheck[eatTileType].Invoke(mahJongType, mahJongTiles, out fitEatMahJongTypeList))
        {
            //需要排除的麻将列表
            List<MahJongType> exceptMahJongTypeList = fitEatMahJongTypeList.ToList();
            //手牌
            List<MahJongType> playTiles = mahJongTiles.tiles.ToList();
            //排除手牌
            exceptMahJongTypeList.ForEach(a => playTiles.Remove(a));
            //加上一组吃牌
            List<List<MahJongType>> eatTiles = mahJongTiles.eatTiles.ToList();
            exceptMahJongTypeList.Add(mahJongType);
            eatTiles.Add(exceptMahJongTypeList);
            //检查听牌
            foreach (MahJongType checkMahJongType in playTiles.ToList())
            {
                //先移除判断是否可以听牌
                playTiles.Remove(checkMahJongType);
                if (CheckListening(playTiles, eatTiles, out _))
                {
                    //打出checkMahJongType可以听牌
                    listeningMagJongTypeList.Add(checkMahJongType);
                }
                //再加回来
                playTiles.Add(checkMahJongType);
            }
        }
        //返回是否有吃&听的操作
        return listeningMagJongTypeList.Count == 0 ? false : true;
    }

    #endregion

}
