using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public abstract class NetMessageHandlerBase<TNetMessage>
{
    /// <summary>消息分发的列表</summary>
    private static Dictionary<string, Delegate> netMessageDict = new Dictionary<string, Delegate>();

    /// <summary>
    /// 开始消息派遣
    /// </summary>
    public static void StartNetMessageHandle()
    {
        //获取当前程序集
        Assembly assembly = Assembly.GetExecutingAssembly();
        //找到所有继承自NetMessageHandlerBase<TNetMessage>的类，即所有NetMessage.***_Handler类
        foreach (Type type in assembly.GetTypes())
        {
            if (type.BaseType.Name == nameof(NetMessageHandlerBase<TNetMessage>) + "`1")//如果继承自NetMessageBase<TNetMessage>，"`1"指有一个泛型
            {
                //实例化一个对象
                object obj = Activator.CreateInstance(type);
                //获取这个类的OnMessage方法
                MethodInfo methodInfo = type.GetMethod(nameof(OnMessage));
                //获取这个类的基类中的泛类，即NetMessage.***类
                Type genericType = type.BaseType.GetGenericArguments()[0];
                //创建一个Action委托，与OnMessage方法具有相同的签名
                Delegate delegateForMethod = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(genericType), obj, methodInfo);
                //将这个对象的委托添加到字典中，供消息派遣使用
                netMessageDict.Add(genericType.Name, delegateForMethod);
            }
        }
    }

    /// <summary>
    /// 向所有客户端发送消息
    /// </summary>
    /// <param name="netMessage"></param>
    /// <param name="negativeClient">指定哪个客户端不需要广播</param>
    protected static void SendMessage(TNetMessage netMessage)
    {
        //构建Json对象
        JObject jObject = new JObject();
        //序列化消息
        jObject[netMessage.GetType().Name] = JsonConvert.SerializeObject(netMessage);
        //发送消息
        Network.SendMessage(jObject);
    }

    /// <summary>
    /// 重写此方法后，在收到当前类型的消息时就会，此方法就会被调用
    /// </summary>
    public virtual void OnMessage(TNetMessage netMessageBase)
    {

    }

    /// <summary>
    /// 消息派遣
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="netMessage"></param>
    public static void Dispatch(string message)
    {
        //解析消息
        JObject jObject = JObject.Parse(message);
        //获取消息对象名称
        string messageObjectName = jObject.Children<JProperty>().First().Name;

        if (!netMessageDict.ContainsKey(messageObjectName))//没有对应消息
        {
            Debug.Log($"错误！没有消息：{messageObjectName}");
            return;
        }

        //获取当前程序集
        Assembly assembly = Assembly.GetExecutingAssembly();
        //反序列化的类型
        Type deserializeType = null;
        //尝试获取NetMessageBase类型
        deserializeType = Type.GetType(typeof(NetMessageBase).FullName + "+" + messageObjectName);
        //不是NetMessageBase类型
        if (deserializeType == null)
        {
            //找到所有继承自NetMessageBase的类
            foreach (Type type in assembly.GetTypes())
            {
                //如果继承自NetMessageBase
                if (type.BaseType.Name == nameof(NetMessageBase))
                {
                    //尝试获取类型（指定assembly.GetName()可跨程序集）
                    deserializeType = Type.GetType(type.FullName + "+" + messageObjectName);
                    if (deserializeType != null) 
                        break;
                }
            }
        }
        if (deserializeType == null)
        {
            Debug.Log("deserializeType异常！");
        }

        //反序列化字符串得到TNetMessage类型的对象
        object netMessage = JsonConvert.DeserializeObject(jObject[messageObjectName].ToString(), deserializeType);
        //通知消息
        netMessageDict[messageObjectName].DynamicInvoke(netMessage);

    }

}
