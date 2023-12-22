using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameNetMessage;

/// <summary>
/// ��Ϸ��Ҫ�࣬�ǳ���Ҫ����
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>����</summary>
    public static GameManager Instance { get; private set; }

    [Header("������Ԥ����")]
    public GameObject mainScenePrefab;

    /// <summary>����Ĭ���ǵ�1�����</summary>
    public const int hostId = 1;

    /// <summary>�ĸ���ҵ�ʵ��</summary>
    public Dictionary<int, Player> Players { get; set; }

    /// <summary>������ʵ��</summary>
    private GameObject mainScene;

    private void Awake()
    {
        //��ʼ������
        Instance = this;
    }

    private void Start()
    {
        //ʵ����һ���µ�������
        mainScene = Instantiate(mainScenePrefab);
    }

    private void OnEnable()
    {
        //ע���¼�
        EventHandler.TilesInitedEvent += OnTilesInitedEvent;
    }

    private void OnDisable()
    {
        //ȡ��ע���¼�
        EventHandler.TilesInitedEvent -= OnTilesInitedEvent;
    }

    /// <summary>
    /// ��ʼ������Ϸ
    /// </summary>
    public void StartGame()
    {
        //����ֵ�
        Players = new Dictionary<int, Player>();
        //�½�4�����ʵ��
        {
            Player player = new MainPlayer();
            Players.Add(1, player);
            player.InitPlayer(1, true);
        }
        for (int i = 1; i < 4; i++)
        {
            Player player = new AIPlayer();
            Players.Add(i + 1, player);
            player.InitPlayer(i + 1, false);
        }
        //Ϊÿ����ҷ���
        for (int i = 1; i <= 4; i++)
        {
            MahJongTilesManager.Instance.DealTiles(i, 13);
        }
        //Ϊ�����ٷ�һ����
        MahJongTilesManager.Instance.DealTiles(hostId, 1);
        //��ʾ����
        MahJongManager.Instance.MahJongTilesInited();
        //���ҳ���
        Players[hostId].PlayingTile();
        //�ƶ������
        CameraManager.Instance.MoveCameraToPlayer(1);
    }


    /// <summary>
    /// ��ʼ������Ϸ
    /// </summary>
    /// <param name="playerId">��ǰ��ҵ�Id</param>
    public void StartMultiPlayerGame(int playerId)
    {
        //����ѡ����Ϸģʽ���
        ChoosePlayModeUI.Instance.choosePlayModePanel.SetActive(false);
        //����ֵ�
        Players = new Dictionary<int, Player>();
        //�½�4�����ʵ��
        if (playerId == 1)
        {
            //1
            Player player = new MainPlayer();
            Players.Add(1, player);
            player.InitPlayer(1, true);
            //2
            player = new AIPlayer();
            Players.Add(2, player);
            player.InitPlayer(2, false);
            //3
            player = new OtherPlayer();
            Players.Add(3, player);
            player.InitPlayer(3, false);
            //4
            player = new AIPlayer();
            Players.Add(4, player);
            player.InitPlayer(4, false);
        }
        else if (playerId == 3)
        {
            //1
            Player player = new OtherPlayer();
            Players.Add(1, player);
            player.InitPlayer(1, true);
            //2
            player = new AIPlayer();
            Players.Add(2, player);
            player.InitPlayer(2, false);
            //3
            player = new MainPlayer();
            Players.Add(3, player);
            player.InitPlayer(3, false);
            //4
            player = new AIPlayer();
            Players.Add(4, player);
            player.InitPlayer(4, false);
        }
        //�ƶ������
        CameraManager.Instance.MoveCameraToPlayer(playerId);
    }



    private void OnTilesInitedEvent(Dictionary<int, List<MahJongType>> tiles)
    {
        //Ϊÿ����ҷ���
        for (int i = 1; i <= 4; i++)
        {
            //����ҷ���
            Players[i].PlayerTiles.tiles = tiles[i];
            //���Ƴ����Ƴ�
            tiles[i].ForEach(a => MahJongTilesManager.Instance.mCurrentMahJongList.Remove(a));
        }
        //��ʾ����
        MahJongManager.Instance.MahJongTilesInited();
        //���ҳ���
        Players[hostId].PlayingTile();

    }

    /// <summary>
    /// ��Ҵ�����ƣ���Ҫ���ں��ƺͳ����жϣ�
    /// </summary>
    /// <param name="playerId">���Id</param>
    public void PlayerPlayingTile(int playerId, MahJongType mahJongType)
    {
        //�����ж�
        foreach (Player player in Players.Values)
        {
            //�½����Ʋ�������
            WinOperationData winOperationData = new WinOperationData();
            winOperationData.mahJongType = mahJongType;
            if (player.PlayerId != playerId && player.CheckWin(ref winOperationData) == true)
            {
                //����Ӯ�Ʋ�������
                winOperationData.fromPlayerId = playerId;
                winOperationData.toPlayerId = player.PlayerId;
                //������Һ���
                player.WinOperation(winOperationData);
                return;
            }
        }
        //�����жϡ������������ҳ��ƣ�����ת��������ҵĳ��ƻغ�
        if (EatTileManager.Instance.CheckEat(playerId, mahJongType) == true)
        {
            //������ҳ��ƻغ�
            return;
        }

        //��Ҵ������
        PlayerPlayedTile(playerId, mahJongType);
    }

    /// <summary>
    /// ��Ҵ������
    /// </summary>
    /// <param name="playerId">���Id</param>
    public void PlayerPlayedTile(int playerId, MahJongType mahJongType)
    {
        //��������
        SoundManager.Instance.PlayTileSound(mahJongType);
        //��һ������ж�
        int nextPlayerId = playerId % 4 + 1;
        //����ҷ�һ����
        Players[nextPlayerId].DealATile();

    }

    /// <summary>
    /// û���ˣ���Ϸ����
    /// </summary>
    /// <param name="gameOver">0����û���ˣ�1-4������Ӯ�ң����ִ���Ӯ��Id</param>
    public void GameOver(GameNetMessage.GameOver gameOver)
    {
        //��Ϸ�����������ж�
        Players.Values.ToList().ForEach(a => a.GameOver());
        //�ƶ������������
        CameraManager.Instance.MoveToTop();
        //����ʣ������UI
        MainUI.Instance.mainPanel.SetActive(false);
        if(gameOver.overType != GameNetMessage.OverType.NoTileLeft)
        {
            //չʾ����
            MahJongManager.Instance.LayTreasure(gameOver.winPlayerId);
            //չʾ����
            MahJongManager.Instance.ShowWin(gameOver);
        }

        //������Ϸ����UI
        GameOverUI.Instance.GameOver(gameOver);

        print($"��Ϸ������Ӯ�ң�{gameOver.winPlayerId}");
    }


    /// <summary>
    /// ���¿�ʼһ������Ϸ��������Ϸ��
    /// </summary>
    public void RestartGame()
    {
        //����������
        Destroy(mainScene);
        //�����������
        Players.Values.ToList().ForEach(action => { action.Dispose(); });
        //ʵ����һ���µ�������
        mainScene = Instantiate(mainScenePrefab);
        //����ѡ����Ϸģʽ���
        ChoosePlayModeUI.Instance.choosePlayModePanel.SetActive(false);
        //��ʼ������Ϸ
        StartGame();
    }

    /// <summary>
    /// ���¿�ʼһ������Ϸ��������Ϸ��
    /// </summary>
    public void RestartMultiPlayerGame()
    {
        //����������
        Destroy(mainScene);
        //�����������
        Players.Values.ToList().ForEach(action => { action.Dispose(); });
        //ʵ����һ���µ�������
        mainScene = Instantiate(mainScenePrefab);
        //��ʼ������Ϸ
        StartMultiPlayerGame(MainPlayer.Instance.PlayerId);
    }



}
