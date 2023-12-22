using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class NetMessageBase
{
    /// <summary>
    /// 心跳消息，用于检测当前客户端是否还在连接中
    /// </summary>
    public class PingPong
    {

    }

    /// <summary>
    /// 心跳消息的处理类
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
            //记录上一次Pong的时间
            GameClient.Instance.lastPongTime = Time.time;
        }

    }
}


