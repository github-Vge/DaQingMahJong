using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameClient : MonoBehaviour
{
    /// <summary>����</summary>
    public static GameClient Instance;
    [Header("IP��ַ")]
    public string ip;
    [Header("�˿ں�")]
    public int port;
    /// <summary>�Ƿ����ӵ��˷����</summary>
    private bool isConnect;
    /// <summary>�Ƿ����ӵ��˷����</summary>
    public static bool IsConnect
    {
        get { return Instance == null ? false : Instance.isConnect; }
    }
    /// <summary>��¼��һ�η���Ping��Ϣ��ʱ��</summary>
    private float lastPingTime;
    /// <summary>��һ�ν��յ�Pong��Ϣ��ʲôʱ��</summary>
    [HideInInspector]
    public float lastPongTime;

    private void Awake()
    {
        //��ʼ������
        Instance = this;
        //��ʼ���ϴ�Ping��ʱ��
        lastPingTime = Time.time;
        lastPongTime = Time.time;
    }

    private void Start()
    {

    }

    /// <summary>
    /// ���ӷ����
    /// </summary>
    public void Connect()
    {
        //���ӵ�������
        ip = ip == string.Empty ? "127.0.0.1" : ip;
        port = port == 0 ? 8888 : port;
        if (Network.Connect(ip, port))//���ӳɹ�
        {
            //��ʼ��Ϣ����
            NetMessageHandlerBase<NetMessageBase.PingPong>.StartNetMessageHandle();
            //���͵�¼��Ϣ
            NetMessage.Login_Handler.SendMessage();

            isConnect = true;
            //����Ping��Ϣ
            lastPingTime = Time.time;
            NetMessageBase.PingPong_Handler.SendMessage();
        }
        else//����ʧ��
        {

        }
    }


    private void Update()
    {
        if(isConnect)//������ӵ��������
        {
            //������Ϣ
            lock (Network.NetMessageQueue)
            {
                while (Network.NetMessageQueue.Count > 0)
                {
                    string message = Network.NetMessageQueue.Dequeue();
                    //���ڵ���
                    Debug.Log("���յ�����˵���Ϣ��" + message);
                    //��ǲ��Ϣ
                    NetMessageHandlerBase<NetMessageBase.PingPong>.Dispatch(message);

                }
            }
            //����������Ϣ��ÿ��10���ӷ���һ�Σ�
            if (Time.time - lastPingTime > 10f)
            {
                //��¼���ping��ʱ��
                lastPingTime = Time.time;
                //����������Ϣ
                NetMessageBase.PingPong_Handler.SendMessage();
                //���Pong��Ϣ���û�յ��ˣ��ɷ���˷��͹����ģ�
                if ((Time.time - lastPongTime) > 30f)//������30��
                {
                    //�Ͽ������˵�����
                    Network.CloesSocket();
                }
            }
        }
    }



    private void OnDestroy()
    {
        if (isConnect)
        {
            Network.CloesSocket();
        }
    }

}
