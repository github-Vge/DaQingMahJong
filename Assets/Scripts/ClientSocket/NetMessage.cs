using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public partial class NetMessage : NetMessageBase
{
    /// <summary>
    /// 登录消息
    /// 客户端->服务端：玩家登入
    /// 服务端->客户端：登录成功
    /// </summary>
    public class Login
    {

    }

    /// <summary>
    /// 登录消息的处理类
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
            Debug.Log("连接Socket成功");
        }

    }

    /// <summary>
    /// 加入房间消息
    /// 客户端->服务端：加入一个房间，并指定一个房间ID
    /// 服务端->客户端：返回玩家索引
    /// </summary>
    public class JoinRoom
    {
        public int roomID;
        public int playerIndex;
    }

    /// <summary>
    /// 加入房间消息的处理类
    /// </summary>
    public class JoinRoom_Handler : NetMessageHandlerBase<JoinRoom>
    {
        /// <summary>
        /// 创建或加入1个随机的房间
        /// </summary>
        public static void SendMessage()
        {
            //构建消息
            JoinRoom joinRoom = new JoinRoom();
            joinRoom.roomID = 1;
            //发送消息
            SendMessage(joinRoom);
        }

        public override void OnMessage(JoinRoom netMessageBase)
        {
            Debug.Log("加入房间成功");
        }

    }


    /// <summary>
    /// 开始游戏消息
    /// 服务端->客户端：通知指定房间的所有玩家开始游戏
    /// </summary>
    public class StartGame
    {
        /// <summary>玩家Id</summary>
        public int playerId;
        /// <summary>是否为AI玩家</summary>
        public List<bool> isAI = new List<bool>();
        //public int roomID;
        /// <summary>是否是房主</summary>
        public bool isRoomOwner;
    }

    /// <summary>
    /// 开始游戏消息的处理类
    /// </summary>
    public class StartGame_Handler : NetMessageHandlerBase<StartGame>
    {
        public override void OnMessage(StartGame netMessage)
        {
            Debug.Log("开始游戏啦！是否房主？=>" + netMessage.isRoomOwner);
            //隐藏等待玩家中面板
            ChoosePlayModeUI.Instance.waitingAnotherPlayerPanel.SetActive(false);
            //开始多人游戏
            GameManager.Instance.StartMultiPlayerGame(netMessage.playerId);
        }

    }


}
