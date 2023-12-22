using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public partial class NetMessage : NetMessageBase
{
    /// <summary>
    /// ��¼��Ϣ
    /// �ͻ���->����ˣ���ҵ���
    /// �����->�ͻ��ˣ���¼�ɹ�
    /// </summary>
    public class Login
    {

    }

    /// <summary>
    /// ��¼��Ϣ�Ĵ�����
    /// </summary>
    public class Login_Handler : NetMessageHandlerBase<Login>
    {
        public static void SendMessage()
        {
            Login netMessage = new Login();
            SendMessage(netMessage);
        }

        public override void OnMessage(Login message)
        {
            Debug.Log("����Socket�ɹ�");
        }

    }

    /// <summary>
    /// ���뷿����Ϣ
    /// �ͻ���->����ˣ�����һ�����䣬��ָ��һ������ID
    /// �����->�ͻ��ˣ������������
    /// </summary>
    public class JoinRoom
    {
        public int roomID;
        public int playerIndex;
    }

    /// <summary>
    /// ���뷿����Ϣ�Ĵ�����
    /// </summary>
    public class JoinRoom_Handler : NetMessageHandlerBase<JoinRoom>
    {
        /// <summary>
        /// ���������1������ķ���
        /// </summary>
        public static void SendMessage()
        {
            //������Ϣ
            JoinRoom joinRoom = new JoinRoom();
            joinRoom.roomID = 1;
            //������Ϣ
            SendMessage(joinRoom);
        }

        public override void OnMessage(JoinRoom netMessageBase)
        {
            Debug.Log("���뷿��ɹ�");
        }

    }


    /// <summary>
    /// ��ʼ��Ϸ��Ϣ
    /// �����->�ͻ��ˣ�ָ֪ͨ�������������ҿ�ʼ��Ϸ
    /// </summary>
    public class StartGame
    {
        /// <summary>���Id</summary>
        public int playerId;
        /// <summary>�Ƿ�ΪAI���</summary>
        public List<bool> isAI = new List<bool>();
        //public int roomID;
        /// <summary>�Ƿ��Ƿ���</summary>
        public bool isRoomOwner;
    }

    /// <summary>
    /// ��ʼ��Ϸ��Ϣ�Ĵ�����
    /// </summary>
    public class StartGame_Handler : NetMessageHandlerBase<StartGame>
    {
        public override void OnMessage(StartGame netMessage)
        {
            Debug.Log("��ʼ��Ϸ�����Ƿ�����=>" + netMessage.isRoomOwner);
            //���صȴ���������
            ChoosePlayModeUI.Instance.waitingAnotherPlayerPanel.SetActive(false);
            //��ʼ������Ϸ
            GameManager.Instance.StartMultiPlayerGame(netMessage.playerId);
        }

    }


}
