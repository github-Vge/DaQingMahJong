using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EatTileUI : MonoBehaviour
{
    /// <summary>����</summary>
    public static EatTileUI Instance;

    [Header("���Ʋ������")]
    public GameObject eatTilePanel;

    [Header("���ư�ťԤ����")]
    public GameObject eatButtonPrefab;


    /// <summary>��ǰ�����г��ư�ť</summary>
    private List<GameObject> eatButtonList = new List<GameObject>();


    private void Awake()
    {
        //��ʼ������
        Instance = this;
    }

    /// <summary>
    /// ����������ƿ��Գ�ʱ���ã�һ�γ��ƻغϻᱻ��ε���
    /// </summary>
    /// <param name="eatTileOperation"></param>
    public void OnHaveTileToEatEvent(EatTileOperation eatTileOperation)
    {
        //��ʾ�������
        eatTilePanel.SetActive(true);
        if (eatButtonList.Count == 0)//�����������һ�ε���
        {
            //ʵ������������ť
            GameObject dontEatButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
            //���İ�ť�ı�Ϊ������
            dontEatButton.GetComponentInChildren<TextMeshProUGUI>().text = "��";
            //�󶨰�ť�¼�
            dontEatButton.GetComponent<Button>().onClick.AddListener(() => {
                //���Ƴ��Ʋ���
                EatTileOperation op = eatTileOperation;
                OnEatButtonClick(op, false);
            });
            //��ӵ��б���
            eatButtonList.Add(dontEatButton);
        }
        //���ư�ťʵ��
        {
            //ʵ����һ����ť
            GameObject eatButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
            //���ð�ťƫ��
            eatButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f * eatButtonList.Count, 0f);
            //���İ�ť�ı�
            eatButton.GetComponentInChildren<TextMeshProUGUI>().text = eatTileOperation.eatTileType switch
            {
                EatTileType.LeftEatAndListening => "����",
                EatTileType.MiddleEatAndListening => "����",
                EatTileType.RightEatAndListening => "����",
                EatTileType.TouchAndListening => "����",
                EatTileType.GangAndListening => "����",

                EatTileType.LeftEat => "��",
                EatTileType.MiddleEat => "��",
                EatTileType.RightEat => "��",
                EatTileType.Touch => "��",
                EatTileType.Gang => "��",

                _ => "��",
            };

            //�󶨰�ť�¼�
            eatButton.GetComponent<Button>().onClick.AddListener(() => {             
                //���Ƴ��Ʋ���
                EatTileOperation op = eatTileOperation;
                OnEatButtonClick(op, true);
                });
            //������Ӧ��������
            List<GameObject> highlightGameObjects = new List<GameObject>();
            foreach (MahJongType mahJongType in eatTileOperation.fitEatMahJongTypeList)
            {
                highlightGameObjects.Add(MahJongManager.Instance.mPlayerGameObjects[eatTileOperation.toPlayerId].tiles.First(p => 
                    mahJongType == MahJongManager.Instance.GetMahJongType(p) && !highlightGameObjects.Contains(p)
                ));
            }
            highlightGameObjects.Add(MahJongManager.Instance.mPlayerGameObjects[eatTileOperation.fromPlayerId].playedTiles.Last());
            //ע����Ҫ��������
            eatButton.GetComponent<EatButtonHighlight>().RegisterHighlightGameObject(highlightGameObjects);
            //��ӵ��б���
            eatButtonList.Add(eatButton);
        }



    }


    /// <summary>
    /// �����������ⰴť ����ʱ����
    /// </summary>
    /// <param name="eatTileOperation">���ƵĲ���</param>
    /// <param name="whetherToEat">�Ƿ���ƣ��Ƿ����ˡ�������ť��</param>
    private void OnEatButtonClick(EatTileOperation eatTileOperation, bool whetherToEat)
    {
        //ʣ������ص����������������ƣ�
        for (int i = 0; i < eatButtonList.Count - 2; i++)
        {
            //���һ����������
            EatTileOperation op = new EatTileOperation();
            //���ȼ��ͺ�
            op.eatTilePriority = 100;
            //���������ص�
            if (GameClient.IsConnect == false)//������Ϸ
            {
                EatTileManager.Instance.TotalNumberOfOperationsCurrentlyInProgress.Add(op);
            }
            else//������Ϸ
            {
                //�����˻ص���Ϣ����������Ϣ��
                GameNetMessage.PlayerHaveTileToEat_Handler.SendMessage(op);
            }
        }

        //�������г��ư�ť
        eatButtonList.ForEach(a => Destroy(a));
        //����б�
        eatButtonList.Clear();
        //���س������
        eatTilePanel.SetActive(false);

        //���Ʋ���
        eatTileOperation.whetherToEat = whetherToEat;
        if (GameClient.IsConnect == false)//������Ϸ
        {
            //�ص�����
            EatTileManager.Instance.TotalNumberOfOperationsCurrentlyInProgress.Add(eatTileOperation);
            //����Ǹܣ���Ϊ����ٷ�һ����
            if (eatTileOperation.eatTileType == EatTileType.Gang)
            {
                GameManager.Instance.Players[1].DealATile();
            }
        }
        else//������Ϸ
        {
            //�����˻ص���Ϣ
            GameNetMessage.PlayerHaveTileToEat_Handler.SendMessage(eatTileOperation);
        }
        //��ҳ������ˣ�����ѡ�������
        GameManager.Instance.Players[1].PlayingTile();


    }

    /// <summary>
    /// ������Ʋ�������Ҿ����Ƿ����ƣ�
    /// <param name="mahJongType">�ݴ�Ҫ�������</param>
    /// </summary>
    public void ListeningOperation(MahJongType mahJongType)
    {
        //��ʾ�������
        eatTilePanel.SetActive(true);

        //ʵ������������ť
        GameObject listeningButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
        //���İ�ť�ı�Ϊ������
        listeningButton.GetComponentInChildren<TextMeshProUGUI>().text = "��";
        //���ð�ťλ��
        listeningButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f, 0f);
        //�󶨰�ť�¼�
        listeningButton.GetComponent<Button>().onClick.AddListener(() => OnListeningButtonClick(mahJongType, true));
        //��ӵ��б���
        eatButtonList.Add(listeningButton);

        //ʵ��������������ť
        GameObject dontListeningButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
        //���İ�ť�ı�Ϊ��������
        dontListeningButton.GetComponentInChildren<TextMeshProUGUI>().text = "����";
        //�󶨰�ť�¼�
        dontListeningButton.GetComponent<Button>().onClick.AddListener(() => OnListeningButtonClick(mahJongType, false));
        //��ӵ��б���
        eatButtonList.Add(dontListeningButton);
    }

    /// <summary>
    /// �����������ⰴť ����ʱ����
    /// </summary>
    /// <param name="eatTileOperation">���ƵĲ���</param>
    /// <param name="whetherToEat">�Ƿ���ƣ��Ƿ����ˡ�������ť��</param>
    private void OnListeningButtonClick(MahJongType mahJongType, bool whetherToListening)
    {
        //�������г��ư�ť
        eatButtonList.ForEach(a => Destroy(a));
        //����б�
        eatButtonList.Clear();
        //���س������
        eatTilePanel.SetActive(false);

        if(GameClient.IsConnect == false)//������Ϸ
        {
            //�ص�����
            if (whetherToListening == true)//����
            {
                GameManager.Instance.Players[MainPlayer.Instance.PlayerId].Listening(mahJongType);
            }
            else//������
            {
                GameManager.Instance.PlayerPlayingTile(MainPlayer.Instance.PlayerId, mahJongType);
            }
        }
        else//������Ϸ
        {
            //�����˴��ز���
            GameNetMessage.PlayerListenOperation_Handler.SendMessage(whetherToListening, mahJongType);
        }

    }

    /// <summary>
    /// ��Һ��Ʋ�������Ҿ����Ƿ���ƣ�
    /// </summary>
    public void WinOperation(GameNetMessage.WinOperationData winOperationData)
    {
        //��ʾ�������
        eatTilePanel.SetActive(true);

        //ʵ������������ť
        GameObject winButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
        //���İ�ť�ı�Ϊ������
        winButton.GetComponentInChildren<TextMeshProUGUI>().text = "��";
        //���ð�ťλ��
        winButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f, 0f);
        //�󶨰�ť�¼�
        winButton.GetComponent<Button>().onClick.AddListener(() => OnWinButtonClick(winOperationData, true));
        //������Ӧ��������
        List<GameObject> highlightGameObjects = new List<GameObject>();
        if(winOperationData.winOperationType == GameNetMessage.WinOperationType.SelfTouch_StrongWind)//�δ��ʱ���������ƿ����ڳ�����
        {
            foreach (MahJongType mahJongType in winOperationData.fitTiles)
            {
                highlightGameObjects.Add(MahJongManager.Instance.mPlayerGameObjects[winOperationData.toPlayerId].tiles
                    .Concat(MahJongManager.Instance.mPlayerGameObjects[winOperationData.toPlayerId].playedTiles)//���ܲ��ԣ�������4���Ʒ���winOperationData.fitTiles
                    .First(p =>
                    mahJongType == MahJongManager.Instance.GetMahJongType(p) && !highlightGameObjects.Contains(p)
                ));
            }
        }
        else//��������
        {
            foreach (MahJongType mahJongType in winOperationData.fitTiles)
            {
                highlightGameObjects.Add(MahJongManager.Instance.mPlayerGameObjects[winOperationData.toPlayerId].tiles.First(p =>
                    mahJongType == MahJongManager.Instance.GetMahJongType(p) && !highlightGameObjects.Contains(p)
                ));
            }
        }

        if(winOperationData.winOperationType == GameNetMessage.WinOperationType.OtherPlayed)//��������
        {
            highlightGameObjects.Add(MahJongManager.Instance
                .mPlayerGameObjects[winOperationData.fromPlayerId].playedTiles.Last());
        }
        else//������
        {
            highlightGameObjects.Add(MahJongManager.Instance
                .mPlayerGameObjects[winOperationData.fromPlayerId].tiles.Last());
        }
        winButton.GetComponent<EatButtonHighlight>().RegisterHighlightGameObject(highlightGameObjects);
        //��ӵ��б���
        eatButtonList.Add(winButton);

        //ʵ��������������ť
        GameObject dontWinButton = Instantiate(eatButtonPrefab, eatTilePanel.transform);
        //���İ�ť�ı�Ϊ��������
        dontWinButton.GetComponentInChildren<TextMeshProUGUI>().text = "����";
        //�󶨰�ť�¼�
        dontWinButton.GetComponent<Button>().onClick.AddListener(() => OnWinButtonClick(winOperationData, false));
        //��ӵ��б���
        eatButtonList.Add(dontWinButton);
    }


    /// <summary>
    /// �����������ⰴť ����ʱ����
    /// </summary>
    /// <param name="eatTileOperation">���ƵĲ���</param>
    /// <param name="whetherToEat">�Ƿ���ƣ��Ƿ����ˡ�������ť��</param>
    private void OnWinButtonClick(GameNetMessage.WinOperationData winOperationData, bool whetherToWin)
    {
        //�������г��ư�ť
        eatButtonList.ForEach(a => Destroy(a));
        //����б�
        eatButtonList.Clear();
        //���س������
        eatTilePanel.SetActive(false);


        if(GameClient.IsConnect == false)//������Ϸ
        {
            //�ص�����
            if (whetherToWin == true)//����
            {
                //������Ч
                SoundManager.Instance.PlayEatSound(SoundManager.EatSoundType.Win);
                //��Ϸ����
                GameManager.Instance.GameOver(new GameNetMessage.GameOver
                {
                    overType = winOperationData.winOperationType switch
                    {
                        GameNetMessage.WinOperationType.SelfTouch_RedDragon => GameNetMessage.OverType.SelfTouch,
                        GameNetMessage.WinOperationType.SelfTouch_Treasure => GameNetMessage.OverType.SelfTouch,
                        GameNetMessage.WinOperationType.SelfTouch_EatTile => GameNetMessage.OverType.SelfTouch,
                        GameNetMessage.WinOperationType.OtherPlayed => GameNetMessage.OverType.OtherPlayed,
                        _ => GameNetMessage.OverType.None,
                    },
                    fromPlayerId = winOperationData.fromPlayerId,
                    winPlayerId = winOperationData.toPlayerId,
                    mahJongType = winOperationData.mahJongType,
                });
            }
            else//������
            {
                MainPlayer.Instance.PlayingTile();
                //TODO:
            }
        }
        else//������Ϸ
        {
            //�ص������������˴�����Ϣ
            GameNetMessage.WinOperation_Handler.SendMessage(winOperationData.mahJongType, true);
            
        }

    }




}
