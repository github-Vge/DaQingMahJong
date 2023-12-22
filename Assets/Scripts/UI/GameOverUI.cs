using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    /// <summary>����</summary>
    public static GameOverUI Instance { get; private set; }

    [Header("��Ϸ�������")]
    public GameObject gameOverPanel;
    [Header("��Ϸ������ʾ������")]
    public TextMeshProUGUI gameOverText;


    [Header("����һ�� ��ť")]
    [SerializeField]
    private Button regameButton;



    private void Awake()
    {
        //��ʼ������
        Instance = this;
        //�󶨰�ť�¼�
        regameButton.onClick.AddListener(OnRegameButtonClick);
    }

    /// <summary>
    /// ������Ϸ�������
    /// </summary>
    public void GameOver(GameNetMessage.GameOver gameOver)
    {
        if (gameOver.overType == GameNetMessage.OverType.NoTileLeft)//û������
        {
            gameOverText.text = "û����\n��Ϸ����";
        }
        else//�����Ӯ��
        {
            if (gameOver.winPlayerId == MainPlayer.Instance.PlayerId)//��������Լ�
            {
                gameOverText.text = "��Ӯ�ˣ�";
            }
            else//��Ҳ�����
            {
                gameOverText.text = "�Է�Ӯ��\n��Ϸ����";
            }
        }

        //��ʾ���
        gameOverPanel.SetActive(true);
    }


    /// <summary>
    /// ����һ�ְ�ť ����ʱ����
    /// </summary>
    private void OnRegameButtonClick()
    {
        //���ص�ǰ���
        gameOverPanel.SetActive(false);
        //��ʾ�ȴ���������
        ChoosePlayModeUI.Instance.waitingAnotherPlayerPanel.SetActive(true);
        if(GameClient.IsConnect == true)//������Ϸ
        {
            //��������һ����Ϣ�������
            GameNetMessage.RestartGame_Handler.SendMessage();
        }
        else//������Ϸ
        {
            GameManager.Instance.RestartGame();
        }

    }




}
