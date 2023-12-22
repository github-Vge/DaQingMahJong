using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class NetMessageBase
{
    /// <summary>
    /// ������Ϣ�����ڼ�⵱ǰ�ͻ����Ƿ���������
    /// </summary>
    public class PingPong
    {

    }

    /// <summary>
    /// ������Ϣ�Ĵ�����
    /// </summary>
    public class PingPong_Handler : NetMessageHandlerBase<PingPong>
    {
        public static void SendMessage()
        {
            PingPong netMessage = new PingPong();
            SendMessage(netMessage);
        }

        public override void OnMessage(PingPong message)
        {
            //��¼��һ��Pong��ʱ��
            GameClient.Instance.lastPongTime = Time.time;
        }

    }
}


