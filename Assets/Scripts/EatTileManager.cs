using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// �����㷨��
/// </summary>
public class EatTileManager : MonoBehaviour
{
    /// <summary>����</summary>
    public static EatTileManager Instance;

    /// <summary>���п��Գ�����ҵĲ����ص���������whetherToEatΪtrue��������Ϊfalse</summary>
    public List<EatTileOperation> TotalNumberOfOperationsCurrentlyInProgress { get; set; } = new List<EatTileOperation>();

    private void Awake()
    {
        //��ʼ������
        Instance = this;
    }

    /// <summary>
    /// �����������Ƿ�Ҫ����
    /// </summary>
    /// <param name="playerId">����Ƶ����</param>
    /// <param name="mahJongType"></param>
    /// <returns>false����û�����Ҫ���ƣ�true���������Ҫ����</returns>
    public bool CheckEat(int playerId, MahJongType mahJongType)
    {
        //������Ľڵ�
        TreeNode<List<MahJongType>>.allTreeNodeList.Clear();

        //����ʱ����Ҫ�жϣ����Ƽ�����Ѿ�������ƣ��������ƽ׶�ʱ���������û�г��Ʋ�������None���͵��ƣ������ƣ�
        if (mahJongType == MahJongType.None) return false;

        //��Ҫ��������б�
        List<int> checkPlayerIdList = new List<int>();
        //����4�����
        for (int i = 1; i <= 4; i++)
        {
            //����ǳ������ ���� ����Ѿ����ƣ�����Ҫ�����ж�
            if (i == playerId
                || GameManager.Instance.Players[i].IsListening == true
                )
            {
                continue;
            }
            checkPlayerIdList.Add(i);
        }




        //��¼�������ȼ���1��ߣ������ȣ�
        int eatTilePriority = 1;
        //ÿ����ҿ��ԳԵ��Ƶ��ֵ䣬key:���������value:���п��ԳԵ����Լ��������ȼ�
        List<EatTileOperation> eatTileOperationList = new List<EatTileOperation>();



        //���Ʋ����� �� ���Ʋ����� �ж�
        foreach (int checkPlayerId in checkPlayerIdList)
        {
            List<MahJongType> listeningMagJongTypeList = new List<MahJongType>();
            //�����ҳ��Ʋ�����
            if (CheckRightEatAndListening(checkPlayerId, mahJongType, out List<MahJongType> fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //�����ҳ���
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
                //���ȼ���1
                eatTilePriority++;
            }
            //�����г��Ʋ�����
            if (CheckMiddleEatAndListening(checkPlayerId, mahJongType, out fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //�����г���
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
                //���ȼ���1
                eatTilePriority++;
            }

            //��������Ʋ�����
            if (CheckLeftEatAndListening(checkPlayerId, mahJongType, out fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //���������
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
                //���ȼ���1
                eatTilePriority++;
            }
            //�������Ʋ�����
            if (CheckTouchAndListening(checkPlayerId, mahJongType, out fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //��������
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
                //���ȼ���1
                eatTilePriority++;
            }

        }



        //�����ж�
        foreach (int checkPlayerId in checkPlayerIdList)
        {
            if (CheckGang(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out List<MahJongType> fitEatMahJongTypeList))//��������
            {
                //��������
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
                //���ȼ���1
                eatTilePriority++;
            }

        }

        //�����ж�
        foreach (int checkPlayerId in checkPlayerIdList)
        {
            if (CheckTouch(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out List<MahJongType> fitEatMahJongTypeList))//��������
            {
                //��������
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
                //���ȼ���1
                eatTilePriority++;
            }


        }

        //�����ж�
        foreach (int checkPlayerId in checkPlayerIdList)
        {
            if (checkPlayerId == playerId % 4 + 1) //����һ�����
            {

                if (CheckRightEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out List<MahJongType> fitEatMahJongTypeList))//�����ҳ���
                {
                    //�����ҳ���
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
                    //���ȼ���1
                    eatTilePriority++;
                }
                if (CheckMiddleEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))//�����г���
                {
                    //�����г���
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
                    //���ȼ���1
                    eatTilePriority++;
                }

                if (CheckLeftEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))//���������
                {
                    //���������
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
                    //���ȼ���1
                    eatTilePriority++;
                }
            }


        }
        if(eatTileOperationList.Count == 0)//û����Ҫ���Ƶ����
        {
            return false;
        }

        //��ճ��Ʋ����ص���
        TotalNumberOfOperationsCurrentlyInProgress.Clear();

        //��ÿ����Ҫ�Ե��ƽ��в������Ƿ�Ҫ�ԣ���
        foreach (EatTileOperation eatTileOperation in eatTileOperationList)
        {
            //�����¼������Ƴ���[!��Ҫ����]
            EventHandler.CallHaveTileToEat(eatTileOperation);         
        }

        //��ʼ�������Ʋ���
        StartCoroutine(EatTileRoundCallbackCoroutine(eatTilePriority - 1));

        return true;

    }

    #region ����Ӧ���������Ƿ��п��ԳԵ���

    /// <summary>
    /// ����Ƿ�������Ʋ�����
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <param name="fitEatMahJongTypeList"></param>
    /// <param name="listeningMagJongTypeList">��������ƿ�������</param>
    /// <returns></returns>
    private bool CheckLeftEatAndListening(int checkPlayerId, MahJongType mahJongType, out List<MahJongType> fitEatMahJongTypeList, out List<MahJongType> listeningMagJongTypeList)
    {
        //�½���������
        listeningMagJongTypeList = new List<MahJongType>();
        //���ҳ���
        if (CheckLeftEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))
        {
            //��Ҫ�ų����齫�б�
            List<MahJongType> exceptMahJongTypeList = fitEatMahJongTypeList.ToList();
            //����
            List<MahJongType> playTiles = MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].tiles.ToList();
            //�ų�����
            exceptMahJongTypeList.ForEach(a => playTiles.Remove(a));
            //�������
            foreach (MahJongType checkMahJongType in playTiles.ToList())
            {
                //���Ƴ��ж��Ƿ��������
                playTiles.Remove(checkMahJongType);
                if (ListenManager.CheckListening(playTiles, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].eatTiles, out _))
                {
                    //���checkMahJongType��������
                    listeningMagJongTypeList.Add(checkMahJongType);
                }
                //�ټӻ���
                playTiles.Add(checkMahJongType);
            }
        }
        if (listeningMagJongTypeList.Count != 0)
        {
            string listeningLog = "{";
            listeningMagJongTypeList.ForEach(a => listeningLog += a.ToString() + ",");
            listeningLog += "}";
            print($"���{checkPlayerId}������Ʋ��������ƣ�����Ϊ{Enum.GetName(typeof(MahJongType), mahJongType)}������{listeningLog}��������");
        }


        return listeningMagJongTypeList.Count == 0 ? false : true;
    }

    /// <summary>
    /// ����Ƿ����г��Ʋ�����
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <param name="fitEatMahJongTypeList"></param>
    /// <param name="listeningMagJongTypeList">��������ƿ�������</param>
    /// <returns></returns>
    private bool CheckMiddleEatAndListening(int checkPlayerId, MahJongType mahJongType, out List<MahJongType> fitEatMahJongTypeList, out List<MahJongType> listeningMagJongTypeList)
    {
        //�½���������
        listeningMagJongTypeList = new List<MahJongType>();
        //���ҳ���
        if (CheckMiddleEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))
        {
            //��Ҫ�ų����齫�б�
            List<MahJongType> exceptMahJongTypeList = fitEatMahJongTypeList.ToList();
            //����
            List<MahJongType> playTiles = MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].tiles.ToList();
            //�ų�����
            exceptMahJongTypeList.ForEach(a => playTiles.Remove(a));
            //�������
            foreach (MahJongType checkMahJongType in playTiles.ToList())
            {
                //���Ƴ��ж��Ƿ��������
                playTiles.Remove(checkMahJongType);
                if (ListenManager.CheckListening(playTiles, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].eatTiles, out _))
                {
                    //���checkMahJongType��������
                    listeningMagJongTypeList.Add(checkMahJongType);
                }
                //�ټӻ���
                playTiles.Add(checkMahJongType);
            }
        }

        if (listeningMagJongTypeList.Count != 0)
        {
            string listeningLog = "{";
            listeningMagJongTypeList.ForEach(a => listeningLog += a.ToString() + ",");
            listeningLog += "}";
            print($"���{checkPlayerId}���г��Ʋ��������ƣ�����Ϊ{Enum.GetName(typeof(MahJongType), mahJongType)}������{listeningLog}��������");
        }

        return listeningMagJongTypeList.Count == 0 ? false : true;
    }

    /// <summary>
    /// ����Ƿ����ҳ��Ʋ�����
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <param name="fitEatMahJongTypeList"></param>
    /// <param name="listeningMagJongTypeList">��������ƿ�������</param>
    /// <returns></returns>
    private bool CheckRightEatAndListening(int checkPlayerId, MahJongType mahJongType, out List<MahJongType> fitEatMahJongTypeList, out List<MahJongType> listeningMagJongTypeList)
    {
        //�½���������
        listeningMagJongTypeList = new List<MahJongType>();
        //���ҳ���
        if (CheckRightEat(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))
        {
            //��Ҫ�ų����齫�б�
            List<MahJongType> exceptMahJongTypeList = fitEatMahJongTypeList.ToList();
            //����
            List<MahJongType> playTiles = MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].tiles.ToList();
            //�ų�����
            exceptMahJongTypeList.ForEach(a => playTiles.Remove(a));
            //�������
            foreach (MahJongType checkMahJongType in playTiles.ToList())
            {
                //���Ƴ��ж��Ƿ��������
                playTiles.Remove(checkMahJongType);
                if (ListenManager.CheckListening(playTiles, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].eatTiles, out _))
                {
                    //���checkMahJongType��������
                    listeningMagJongTypeList.Add(checkMahJongType);
                }
                //�ټӻ���
                playTiles.Add(checkMahJongType);
            }
        }

        if (listeningMagJongTypeList.Count != 0)
        {
            string listeningLog = "{";
            listeningMagJongTypeList.ForEach(a => listeningLog += a.ToString() + ",");
            listeningLog += "}";
            print($"���{checkPlayerId}���ҳ��Ʋ��������ƣ�����Ϊ{Enum.GetName(typeof(MahJongType), mahJongType)}������{listeningLog}��������");
        }

        return listeningMagJongTypeList.Count == 0 ? false : true;
    }

    /// <summary>
    /// ����Ƿ������Ʋ�����
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <param name="fitEatMahJongTypeList"></param>
    /// <param name="listeningMagJongTypeList">��������ƿ�������</param>
    /// <returns></returns>
    private bool CheckTouchAndListening(int checkPlayerId, MahJongType mahJongType, out List<MahJongType> fitEatMahJongTypeList, out List<MahJongType> listeningMagJongTypeList)
    {
        //�½���������
        listeningMagJongTypeList = new List<MahJongType>();
        //������
        if (CheckTouch(mahJongType, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId], out fitEatMahJongTypeList))
        {
            //��Ҫ�ų����齫�б�
            List<MahJongType> exceptMahJongTypeList = fitEatMahJongTypeList.ToList();
            //����
            List<MahJongType> playTiles = MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].tiles.ToList();
            //�ų�����
            exceptMahJongTypeList.ForEach(a => playTiles.Remove(a));
            //����
            List<List<MahJongType>> eatTiles = MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].eatTiles.ToList();
            //���һ�����
            eatTiles.Add(fitEatMahJongTypeList.Concat(new List<MahJongType> { mahJongType }).ToList());
            //�������
            foreach (MahJongType checkMahJongType in playTiles.ToList())
            {
                //���Ƴ��ж��Ƿ��������
                playTiles.Remove(checkMahJongType);
                if (ListenManager.CheckListening(playTiles, MahJongTilesManager.Instance.mPlayerTiles[checkPlayerId].eatTiles, out _))
                {
                    //���checkMahJongType��������
                    listeningMagJongTypeList.Add(checkMahJongType);
                }
                //�ټӻ���
                playTiles.Add(checkMahJongType);
            }
        }

        if (listeningMagJongTypeList.Count != 0)
        {
            string listeningLog = "{";
            listeningMagJongTypeList.ForEach(a => listeningLog += a.ToString() + ",");
            listeningLog += "}";
            print($"���{checkPlayerId}�����Ʋ��������ƣ�����Ϊ{Enum.GetName(typeof(MahJongType), mahJongType)}������{listeningLog}��������");
        }

        return listeningMagJongTypeList.Count == 0 ? false : true;
    }



    /// <summary>
    /// ����Ƿ�������ƣ���1�����������򣬳�������2���ж�������������һ����
    /// </summary>
    /// <param name="checkPlayerId">Ҫ���Ƶ����Id</param>
    /// <param name="mahJongType">�Ե�������</param>
    /// <returns></returns>
    public bool CheckLeftEat(MahJongType mahJongType, MahJongTiles mahJongTiles , out List<MahJongType> fitEatMahJongTypeList)
    {
        //��ʼ����ϳ��Ƶ����б�
        fitEatMahJongTypeList = new List<MahJongType>();
        if (int.TryParse(Enum.GetName(typeof(MahJongType), mahJongType).Last().ToString(), out int number))//��������
        {
            if (number < 8)
            {
                MahJongType mahJongType1 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType + 1);
                MahJongType mahJongType2 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType + 2);
                if (mahJongType1 != default && mahJongType2 != default)
                {
                    //Console.WriteLine($"���{checkPlayerId}������ƣ�����Ϊ{Enum.GetName(typeof(MahJongType), mahJongType)}");
                    //������ϳ��Ƶ���
                    fitEatMahJongTypeList.Add(mahJongType1);
                    fitEatMahJongTypeList.Add(mahJongType2);
                    //���������
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// ����Ƿ����г���
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType">�Ե�������</param>
    /// <returns></returns>
    public bool CheckMiddleEat(MahJongType mahJongType, MahJongTiles mahJongTiles, out List<MahJongType> fitEatMahJongTypeList)
    {
        //��ʼ����ϳ��Ƶ����б�
        fitEatMahJongTypeList = new List<MahJongType>();
        if (int.TryParse(Enum.GetName(typeof(MahJongType), mahJongType).Last().ToString(), out int number))//��������
        {
            if (number > 1 && number < 9)
            {
                MahJongType mahJongType1 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType - 1);
                MahJongType mahJongType2 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType + 1);
                if (mahJongType1 != default && mahJongType2 != default)
                {
                    //Console.WriteLine($"���{checkPlayerId}���г��ƣ�����Ϊ{Enum.GetName(typeof(MahJongType), mahJongType)}");
                    //������ϳ��Ƶ���
                    fitEatMahJongTypeList.Add(mahJongType1);
                    fitEatMahJongTypeList.Add(mahJongType2);
                    //�����г���
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// ����Ƿ����ҳ���
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <param name="fitEatMahJongTypeList">��ϳ��Ƶ����б�</param>
    /// <returns></returns>
    public bool CheckRightEat(MahJongType mahJongType, MahJongTiles mahJongTiles, out List<MahJongType> fitEatMahJongTypeList)
    {
        //��ʼ����ϳ��Ƶ����б�
        fitEatMahJongTypeList = new List<MahJongType>();
        if (int.TryParse(Enum.GetName(typeof(MahJongType), mahJongType).Last().ToString(), out int number))//��������
        {
            if (number > 2)
            {
                MahJongType mahJongType1 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType - 1);
                MahJongType mahJongType2 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType - 2);
                if (mahJongType1 != default && mahJongType2 != default)
                {
                    //Console.WriteLine($"���{checkPlayerId}���ҳ��ƣ�����Ϊ{Enum.GetName(typeof(MahJongType), mahJongType)}");
                    //������ϳ��Ƶ���
                    fitEatMahJongTypeList.Add(mahJongType1);
                    fitEatMahJongTypeList.Add(mahJongType2);
                    //�����ҳ���
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// ����Ƿ�������
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <returns></returns>
    public bool CheckTouch(MahJongType mahJongType, MahJongTiles mahJongTiles, out List<MahJongType> fitEatMahJongTypeList)
    {
        //��ʼ����ϳ��Ƶ����б�
        fitEatMahJongTypeList = new List<MahJongType>();
        if (mahJongTiles.tiles.Count(p => p == mahJongType) == 2)
        {
            //Console.WriteLine($"���{checkPlayerId}�����ƣ�����Ϊ{Enum.GetName(typeof(MahJongType), mahJongType)}");
            //������ϳ��Ƶ���
            fitEatMahJongTypeList.Add(mahJongType);
            fitEatMahJongTypeList.Add(mahJongType);
            //��������
            return true;
        }

        return false;
    }

    /// <summary>
    /// ����Ƿ��и���
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <returns></returns>
    private bool CheckGang(MahJongType mahJongType, MahJongTiles mahJongTiles, out List<MahJongType> fitEatMahJongTypeList)
    {
        //��ʼ����ϳ��Ƶ����б�
        fitEatMahJongTypeList = new List<MahJongType>();
        if (mahJongTiles.tiles.Count(p => p == mahJongType) == 3)
        {
            //Console.WriteLine($"���{checkPlayerId}�и��ƣ�����Ϊ{Enum.GetName(typeof(MahJongType), mahJongType)}");
            //������ϳ��Ƶ���
            fitEatMahJongTypeList.Add(mahJongType);
            fitEatMahJongTypeList.Add(mahJongType);
            fitEatMahJongTypeList.Add(mahJongType);
            //���Ը���
            return true;
        }

        return false;
    }

    /// <summary>
    /// �����ã����������������
    /// </summary>
    /// <param name="layerCount"></param>
    public void PrintTree(int playerId)
    {
        string log = string.Empty;
        log += $"���{playerId}-----\n";
        bool needLog = false;
        //����������������ã�
        for (int i = 1; i < 5; i++)
        {
            foreach (TreeNode<List<MahJongType>> node in TreeNode<List<MahJongType>>.GetNodesAtLayer(i))
            {
                string tiles = string.Empty;
                //��ȡ�ڵ�����֦�ϵ������齫
                TreeNode<List<MahJongType>> exceptNode = node;
                for (int j = 0; j < i; j++)
                {
                    tiles += $"�㼶{i - j}:{{";
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
    /// ���ƻغϵĻص�������һֱ����ֱ��������ҽ������ƻغ�
    /// </summary>
    /// <param name="totalOperationCount">���Ʋ����ļ���</param>
    /// <returns>Э��</returns>
    private IEnumerator EatTileRoundCallbackCoroutine(int totalOperationCount)
    {
        while (totalOperationCount != TotalNumberOfOperationsCurrentlyInProgress.Count)
        {
            //�ȴ��������
            yield return null;
        }

        List<EatTileOperation> sortedOperations = TotalNumberOfOperationsCurrentlyInProgress.OrderBy(p=>p.eatTilePriority).ToList();

        foreach (EatTileOperation op in sortedOperations)
        {
            if (op.whetherToEat == true)
            {
                //��Ӧ��ҳ��Ʋ���
                MahJongTilesManager.Instance.EatTile(op);

                print($"���{op.toPlayerId}�������{op.fromPlayerId}���ƣ����Ʒ�ʽ��{op.eatTileType}���ƣ�{op.mahJongType}");

                //��Ҽ�������
                GameManager.Instance.Players[op.toPlayerId].PlayingTile();

                yield break;
            }
        }

        //������Ҷ�������
        GameManager.Instance.PlayerPlayedTile(sortedOperations[0].fromPlayerId, sortedOperations[0].mahJongType);

        yield break;
    }



}

public struct EatTileOperation
{
    /// <summary>���Ƶ����</summary>
    public int fromPlayerId;
    /// <summary>���Ƶ����</summary>
    public int toPlayerId;
    /// <summary>�����齫��</summary>
    public MahJongType mahJongType;
    /// <summary>��ϳ��Ƶ��ƣ������Զ�������ϳ��Ƶ��ƾ���һ������������ơ�</summary>
    public List<MahJongType> fitEatMahJongTypeList;
    /// <summary>���Ƶķ�ʽ</summary>
    public EatTileType eatTileType;
    /// <summary>�������ȼ�</summary>
    public int eatTilePriority;

    /// <summary>�Ƿ���ƣ�������Ҵ��ش���ʱ����</summary>
    public bool whetherToEat;
}


public enum EatTileType
{
    None,
    /// <summary>����ƣ������Լ���8��9�򣬱��˴����7��</summary>
    LeftEat,
    /// <summary>�г��ƣ������Լ���4��6�򣬱��˴����5��</summary>
    MiddleEat,
    /// <summary>�ҳ��ƣ������Լ���1��2�򣬱��˴����3��</summary>
    RightEat,
    /// <summary>����</summary>
    Touch,
    /// <summary>����</summary>
    Gang,

    /// <summary>����ƣ�����������</summary>
    LeftEatAndListening,
    /// <summary>�г��ƣ�����������</summary>
    MiddleEatAndListening,           
    /// <summary>�ҳ��ƣ�����������</summary>
    RightEatAndListening,
    /// <summary>���ƣ�����������</summary>
    TouchAndListening,
    /// <summary>���ƣ�����������</summary>
    GangAndListening,


}

/// <summary>
/// ������Һ����жϵ�������
/// </summary>
public class WinTileCheckData
{
    /// <summary>��������б�</summary>
    public List<WinTileItem> winTileItemList = new List<WinTileItem>();

    public struct WinTileItem
    {
        /// <summary>����������</summary>
        public MahJongType mahJongType;
        /// <summary>���Ƶı�����������</summary>
        public int factor;
    }
}



/// <summary>
/// ���Ƶ��齫�����ṹ
/// </summary>
/// <typeparam name="T"></typeparam>
public class TreeNode<T>
{
    public T Data { get; set; }

    /// <summary>��ǰ�ڵ�ĸ��ڵ�</summary>
    public TreeNode<T> Parent { get; set; }
    //public List<TreeNode<T>> Children { get; set; }

    public TreeNode(T data)
    {
        this.Data = data;
        allTreeNodeList.Add(this);
    }

    /// <summary>
    /// ��ȡ��ǰ�ڵ�Ĳ���
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


    /// <summary>�������нڵ���б�</summary>
    public static List<TreeNode<T>> allTreeNodeList = new List<TreeNode<T>>();

    /// <summary>
    /// ��ȡ����ĳһ������нڵ�
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