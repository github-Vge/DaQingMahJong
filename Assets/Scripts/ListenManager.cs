using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ���ƵĹ����࣬�����������йص����в���
/// </summary>
public static class ListenManager
{
    /// <summary>
    /// �����ж�
    /// </summary>
    /// <param name="mahJongTypeList">����</param>
    /// <param name="eatMahJongTypeList">�Թ�����</param>
    /// <param name="listeningTilesData">�������ݣ����ֵ��</param>
    /// <returns></returns>
    public static bool CheckListening(List<MahJongType> mahJongTypeList, List<List<MahJongType>> eatMahJongTypeList, out ListeningTilesData listeningTilesData)
    {
        //�����
        TreeNode<List<MahJongType>>.allTreeNodeList.Clear();

        //�½������жϵ����ṹ
        TreeNode<List<MahJongType>> listeningTreeNode = new TreeNode<List<MahJongType>>(new List<MahJongType> { MahJongType.None });


        //�½���������
        listeningTilesData = new ListeningTilesData();
        //���ó��˵����б�
        listeningTilesData.eatMahJongTypeList = eatMahJongTypeList;
        listeningTilesData.listenItemList = new List<ListenItem>();


        if (eatMahJongTypeList.Count == 0)//û�г��ƣ�û���ţ�
        {
            //��������
            return false;
        }

        #region �ж��Ƿ����۾�
        //��ȡȫ�����ƣ�����+���ƣ�
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
            //û���۾�
            return false;
        }


        #endregion

        //���ܹ��Ĳ��������������� - 1��/ 3��
        int layerCount = (mahJongTypeList.Count - 1) / 3;

        if (mahJongTypeList.Count != 1
            && mahJongTypeList.Count != 4
            && mahJongTypeList.Count != 7
            && mahJongTypeList.Count != 10
            && mahJongTypeList.Count != 13
            )
        {
            Console.WriteLine("�����ж���ʱ���齫����������");
        }

        #region �ж�˳�ӻ�����Ƿ�����

        /*
         * �����ж�����
         * ����layerCount:4, �㼶4:{Stick1,Stick2,Stick3,},�㼶3:{Circle7,Circle8,Circle9,},�㼶2:{Circle7,Circle8,Circle9,},�㼶1:{Circle7,Circle8,Circle9,},
         * ����layerCount:3, �㼶2:{Thousand4,Thousand5,Thousand6,},�㼶1:{Stick4,Stick5,Stick6,},��ֻ�����㣩
         */
        for (int i = 0; i < layerCount; i++)
        {
            foreach (TreeNode<List<MahJongType>> nodeAtLayer in TreeNode<List<MahJongType>>.GetNodesAtLayer(i))
            {
                //��ȡ��ȥ���ڵ�Ԫ��֮��ʣ���Ԫ��
                List<MahJongType> remainingMahJongList = mahJongTypeList.ToList();
                //������ǰ��֦
                TreeNode<List<MahJongType>> exceptNode = nodeAtLayer;
                for (int j = 0; j < i; j++)
                {
                    //ȥ��Ԫ��
                    exceptNode.Data.ForEach(a => remainingMahJongList.Remove(a));
                    exceptNode = exceptNode.Parent;
                }

                //ȥ���ظ�Ԫ��
                List<MahJongType> remainingMahJongNoRepeatList = remainingMahJongList.ToHashSet().ToList();
                //��ȡ��СԪ�أ�С�����Ԫ�ص��齫���ٱ�����
                MahJongType minMahJongType = nodeAtLayer.Data.Min();

                foreach (MahJongType mahJongType in remainingMahJongNoRepeatList)
                {
                    //С��minMahJongType���齫���ٱ�����
                    if (mahJongType < minMahJongType) continue;

                    if (int.TryParse(Enum.GetName(typeof(MahJongType), mahJongType).Last().ToString(), out int number))//��������
                    {
                        //˳��
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

                                //��ӵ��ڵ�
                                node.Parent = nodeAtLayer;

                                continue;
                            }
                        }
                    }
                    //����
                    if (remainingMahJongList.Count(p => p == mahJongType) == 3)
                    {
                        TreeNode<List<MahJongType>> node = new TreeNode<List<MahJongType>>(new List<MahJongType>
                                {
                                    mahJongType, mahJongType, mahJongType,
                                });

                        //��ӵ��ڵ�
                        node.Parent = nodeAtLayer;

                        continue;
                    }
                }
            }
        }

        #endregion




        //��ȡ���������齫�б��ֱ�Ϊ����Ϊ�����ƣ�ֻʣһ���齫��������Ϊ�����ƣ�ʣ�����齫��
        List<TreeNode<List<MahJongType>>> singleTileLayerNodes = TreeNode<List<MahJongType>>.GetNodesAtLayer(layerCount);
        List<TreeNode<List<MahJongType>>> mutiTileLayerNodes = TreeNode<List<MahJongType>>.GetNodesAtLayer(layerCount - 1);
        //�����ҵ��������Ƶ����ͣ��������ƣ�
        foreach (TreeNode<List<MahJongType>> singleTileLayerNode in singleTileLayerNodes)
        {
            //��ȡ��ȥ���ڵ�Ԫ��֮��ʣ���Ԫ��
            List<MahJongType> remainingMahJongList = mahJongTypeList.ToList();
            //�ݴ����������
            List<List<MahJongType>> wildMahJongTypeList = new List<List<MahJongType>>();
            //������ǰ��֦
            TreeNode<List<MahJongType>> exceptNode = singleTileLayerNode;
            for (int j = 0; j < layerCount; j++)
            {
                //ȥ��Ԫ��
                exceptNode.Data.ForEach(a => remainingMahJongList.Remove(a));
                //��ӵ��������
                wildMahJongTypeList.Add(exceptNode.Data);
                exceptNode = exceptNode.Parent;
            }

            //û�����
            if (HasWildTile(eatMahJongTypeList, singleTileLayerNode, remainingMahJongList) == false) continue;


            //�ж�ʣ�������Ƿ�Ϊ1[*����Ҫ����]
            if (remainingMahJongList.Count != 1)
            {
                Console.WriteLine("ֻ��һ���ƣ�ʣ������Ӧ����1");
            }

            Console.WriteLine($"����Ϊ���ţ�����Ϊ{remainingMahJongList[0]}");

            //���ú������ݣ��δ�磩
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



            //���ú�������
            listeningTilesData.listenItemList.Add(new ListenItem
            {
                listenType = ListenType.SingleTile,
                wildMahJongTypeList = wildMahJongTypeList,
                listenTiles = new List<MahJongType> { remainingMahJongList[0] },
                otherTiles = new List<MahJongType>(),
                winTiles = new List<MahJongType> { remainingMahJongList[0] },
            });
        }


        //�����ҵ��������Ƶ����ͣ��������ƣ�
        foreach (TreeNode<List<MahJongType>> mutiTileLayerNode in mutiTileLayerNodes)
        {

            //��ȡ��ȥ���ڵ�Ԫ��֮��ʣ���Ԫ��
            List<MahJongType> remainingMahJongList = mahJongTypeList.ToList();
            //�ݴ����������
            List<List<MahJongType>> wildMahJongTypeList = new List<List<MahJongType>>();
            //������ǰ��֦
            TreeNode<List<MahJongType>> exceptNode = mutiTileLayerNode;
            //�������ڵ�
            for (int j = 0; j < layerCount - 1; j++)
            {
                //ȥ��Ԫ��
                exceptNode.Data.ForEach(a => remainingMahJongList.Remove(a));
                //��ӵ��������
                wildMahJongTypeList.Add(exceptNode.Data);
                exceptNode = exceptNode.Parent;
            }


            //û�����
            if (HasWildTile(eatMahJongTypeList, mutiTileLayerNode, remainingMahJongList) == false) continue;

            //�ж�
            //������
            remainingMahJongList.Sort();
            if (remainingMahJongList.ToHashSet().Count == 1 //������ͬ����
                || remainingMahJongList.ToHashSet().Count == 4 //���Ų�ͬ����
                )
            {
                //���ܺ�
                continue;
            }
            //�ж�1��������
            if (remainingMahJongList.Count(p => p == remainingMahJongList[0]) == 2 && remainingMahJongList.Count(p => p == remainingMahJongList[2]) == 2)
            {
                //���ú�������
                listeningTilesData.listenItemList.Add(new ListenItem
                {
                    listenType = ListenType.TwoDoubleTile,
                    wildMahJongTypeList = wildMahJongTypeList,
                    listenTiles = new List<MahJongType> { remainingMahJongList[0], remainingMahJongList[1] },
                    otherTiles = new List<MahJongType> { remainingMahJongList[2], remainingMahJongList[3] },
                    winTiles = new List<MahJongType> { remainingMahJongList[0], remainingMahJongList[2] },
                });
                //���ú������ݣ��δ�磩
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
            //�ж�2��һ���ԣ�һ����
            List<MahJongType> suitMahJongTypeList = new List<MahJongType>();
            //��ȡ�ܺ��Ƶ���
            foreach (MahJongType mahJongType in remainingMahJongList)
            {
                if (remainingMahJongList.Count(p => p == mahJongType) == 1)
                {
                    suitMahJongTypeList.Add(mahJongType);
                }
            }
            //������
            suitMahJongTypeList.Sort();
            if (suitMahJongTypeList.Count == 1)//������������һ���ƺͶ��Ƶ���һ�����������������������򣬺�������������
            {
                //����һ�����ƣ����ӣ�����������
                MahJongType tripletMahJongType = remainingMahJongList.Except(suitMahJongTypeList).ToList()[0];
                //�����ƣ���������
                MahJongType suitMahJongType = suitMahJongTypeList[0];

                if (int.TryParse(Enum.GetName(typeof(MahJongType), tripletMahJongType).Last().ToString(), out int number1)
                        && int.TryParse(Enum.GetName(typeof(MahJongType), suitMahJongType).Last().ToString(), out int number2))//��������
                {
                    //˳��
                    if ((Math.Abs(tripletMahJongType - suitMahJongType) == 1 && Math.Abs(number1 - number2) == 1))
                    {
                        if (Math.Min(number1, number2) == 1 || Math.Max(number1, number2) == 9)//��һ�ţ��У�����1��һ�򡢶��򣬺���������2�����򡢾��򣬺�������
                        {
                            //���ú������ݣ��У�
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
                        else//�����ţ��ߣ�,�����������򣬺������򡢰���
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
                        //���ú������ݣ��δ�磩
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
                    else if (Math.Abs(tripletMahJongType - suitMahJongType) == 2 && Math.Abs(number1 - number2) == 2)//��һ�ţ��У��������������򣬺�������
                    {
                        //���ú�������
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
                        //���ú������ݣ��δ�磩
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
            else if (suitMahJongTypeList.Count == 2)//����������û���ƺͶ��Ƶ���һ�����������򡢶������򡢰��򣬺����򡢾���
            {
                //����һ�����ƣ����ӣ�����������
                MahJongType tripletMahJongType = remainingMahJongList.Except(suitMahJongTypeList).ToList()[0];

                MahJongType suitMahJongType1 = suitMahJongTypeList[0];
                MahJongType suitMahJongType2 = suitMahJongTypeList[1];

                if (int.TryParse(Enum.GetName(typeof(MahJongType), suitMahJongType1).Last().ToString(), out int number1)
                    && int.TryParse(Enum.GetName(typeof(MahJongType), suitMahJongType2).Last().ToString(), out int number2))//��������
                {
                    //˳��
                    if ((Math.Abs(suitMahJongType1 - suitMahJongType2) == 1 && Math.Abs(number1 - number2) == 1))
                    {
                        if (number1 == 1 || number2 == 9)//��һ�ţ��У�����1��һ�򡢶��򣬺���������2�����򡢾��򣬺�������
                        {
                            //���ú�������
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
                        else//�����ţ��ߣ�,�����������򣬺������򡢰���
                        {
                            //���ú�������
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
                        //���ú������ݣ��δ�磩
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
                    else if (Math.Abs(suitMahJongType1 - suitMahJongType2) == 2 && Math.Abs(number1 - number2) == 2)//��һ�ţ��У��������������򣬺�������
                    {
                        //���ú�������
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
                        //���ú������ݣ��δ�磩
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
        //�к�������
        if (listeningTilesData.listenItemList.Count != 0)
        {
            //��������
            return true;
        }



        return false;
    }

    /// <summary>
    /// �ж��Ƿ������ӣ�����һ�����ƣ�
    /// </summary>
    /// <param name="eatTiles">�Ե���</param>
    /// <param name="node">Ҷ�ӽڵ㣬Countһ��Ϊ3</param>
    /// <returns></returns>
    private static bool HasWildTile(List<List<MahJongType>> eatTiles, TreeNode<List<MahJongType>> node, List<MahJongType> remainingMahJongList)
    {
        //��ȡȫ������
        List<List<MahJongType>> wholeMahJongTypeList = eatTiles.ToList();
        TreeNode<List<MahJongType>> checkNode = node;
        while (checkNode.Parent != null)
        {
            wholeMahJongTypeList.Add(checkNode.Data);
            checkNode = checkNode.Parent;
        }
        //����ܺ��Ƶ���
        wholeMahJongTypeList.Add(remainingMahJongList);

        //�ж��Ƿ��к���
        if (wholeMahJongTypeList.FirstOrDefault(p => p.Contains(MahJongType.RedDragon)) != default)
        {
            //�к��У����Ե����
            return true;
        }
        //�ж��Ƿ��п��ӣ�������ͬ�ģ�
        foreach (List<MahJongType> mahJongTypeList in wholeMahJongTypeList)
        {
            if (mahJongTypeList.Count >= 3 && mahJongTypeList.ToHashSet().Count == 1)//�����ƵĽڵ����п���
            {
                return true;
            }
        }

        //�ж��Ƿ���������
        if (wholeMahJongTypeList.Last().Count(p1 => wholeMahJongTypeList.Last().Count(p2 => p2 == p1) == 2) == 4)
        {
            //�����ԣ������Ƶ�����Ϊ2��
            return true;
        }

        //û�����
        return false;
    }

    /// <summary>
    /// ��ȡ���ƺͳ����е����п����ƣ�����һ�����ƣ�
    /// </summary>
    /// <param name="eatMahJongTypeList">�Ե���</param>
    /// <param name="wildMahJongTypeList">�����е������</param>
    /// <returns>�������б�����һ�����ƣ�������һ�����򣬼��������������ӣ�һ��һ��һ��������������</returns>
    private static List<MahJongType> GetTripletList(List<List<MahJongType>> eatMahJongTypeList, List<List<MahJongType>> wildMahJongTypeList)
    {
        //�������б�
        List<MahJongType> tripletList = new List<MahJongType>();
        //�ж��Ƿ��п��ӣ�������ͬ�ģ�
        foreach (List<MahJongType> mahJongTypeList in eatMahJongTypeList.Concat(wildMahJongTypeList))
        {
            if (mahJongTypeList.Count == 3 && mahJongTypeList.ToHashSet().Count == 1)//�����ƵĽڵ����п���
            {
                tripletList.Add(mahJongTypeList[0]);
            }
        }
        return tripletList;
    }


    #region ����Ƿ��п��Գ��Ʋ����ƵĲ���

    /// <summary>ί���ֵ�</summary>
    private static Dictionary<EatTileType, EatCheckDelegate> eatTileCheck = new Dictionary<EatTileType, EatCheckDelegate>()
        {
            {EatTileType.LeftEat, EatTileManager.Instance.CheckLeftEat },
            {EatTileType.MiddleEat, EatTileManager.Instance.CheckMiddleEat },
            {EatTileType.RightEat, EatTileManager.Instance.CheckRightEat },
            {EatTileType.Touch, EatTileManager.Instance.CheckTouch },
        };

    /// <summary>���Ƽ��ί��</summary>
    private delegate bool EatCheckDelegate(MahJongType mahJongType, MahJongTiles mahJongTiles
    , out List<MahJongType> fitEatMahJongTypeList);


    /// <summary>
    /// �ж��Ƿ��г��Ʋ����ƵĲ���
    /// </summary>
    /// <param name="eatTileType">���Ʒ�ʽ</param>
    /// <param name="mahJongType">�Ե�������</param>
    /// <param name="mahJongTiles">��ҵ��齫�б�</param>
    /// <param name="fitEatMahJongTypeList">��ϳ��Ƶ���</param>
    /// <param name="listeningMagJongTypeList">��������ƿ�������</param>
    /// <returns>�Ƿ���Գ�&��</returns>
    public static bool CheckEatAndListening(EatTileType eatTileType, MahJongType mahJongType, MahJongTiles mahJongTiles
        , out List<MahJongType> fitEatMahJongTypeList, out List<MahJongType> listeningMagJongTypeList)
    {
        //�½���������
        listeningMagJongTypeList = new List<MahJongType>();
        //�ж��Ƿ��ж�Ӧ���͵ĳ���
        if (eatTileCheck[eatTileType].Invoke(mahJongType, mahJongTiles, out fitEatMahJongTypeList))
        {
            //��Ҫ�ų����齫�б�
            List<MahJongType> exceptMahJongTypeList = fitEatMahJongTypeList.ToList();
            //����
            List<MahJongType> playTiles = mahJongTiles.tiles.ToList();
            //�ų�����
            exceptMahJongTypeList.ForEach(a => playTiles.Remove(a));
            //����һ�����
            List<List<MahJongType>> eatTiles = mahJongTiles.eatTiles.ToList();
            exceptMahJongTypeList.Add(mahJongType);
            eatTiles.Add(exceptMahJongTypeList);
            //�������
            foreach (MahJongType checkMahJongType in playTiles.ToList())
            {
                //���Ƴ��ж��Ƿ��������
                playTiles.Remove(checkMahJongType);
                if (CheckListening(playTiles, eatTiles, out _))
                {
                    //���checkMahJongType��������
                    listeningMagJongTypeList.Add(checkMahJongType);
                }
                //�ټӻ���
                playTiles.Add(checkMahJongType);
            }
        }
        //�����Ƿ��г�&���Ĳ���
        return listeningMagJongTypeList.Count == 0 ? false : true;
    }

    #endregion

}
