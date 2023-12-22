using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePlayModeUI : MonoBehaviour
{
    /// <summary>����</summary>
    public static ChoosePlayModeUI Instance { get; private set; }


    [Header("ѡ����Ϸģʽ��壨��ǰ��壩")]
    public GameObject choosePlayModePanel;
    [Header("�ȴ�������")]
    public GameObject waitingAnotherPlayerPanel;
    [Header("������Ϸ ��ť")]
    [SerializeField]
    private Button singlePlayerButton;
    [Header("������Ϸ ��ť")]
    [SerializeField]
    private Button multiPlayerButton;



    private void Awake()
    {
        //��ʼ������
        Instance = this;
        //�󶨰�ť�¼�
        singlePlayerButton.onClick.AddListener(OnSinglePlayerButtonClick);
        multiPlayerButton.onClick.AddListener(OnMultiPlayerButtonClick);

    }
    /// <summary>
    /// ������Ϸ ��ť����ʱ����
    /// </summary>
    private void OnSinglePlayerButtonClick()
    {
        //���ص�ǰ���
        choosePlayModePanel.SetActive(false);
        //��ʼ��Ϸ
        GameManager.Instance.StartGame();
    }
    /// <summary>
    /// ������Ϸ ��ť����ʱ����
    /// </summary>
    private void OnMultiPlayerButtonClick()
    {
        //�������ӷ����
        GameClient.Instance.Connect();
        //����ѡ��ģʽ���
        choosePlayModePanel.SetActive(false);
        //���뷿��
        NetMessage.JoinRoom_Handler.SendMessage();
        //��ʾ�ȴ�������
        waitingAnotherPlayerPanel.SetActive(true);
    }
}
