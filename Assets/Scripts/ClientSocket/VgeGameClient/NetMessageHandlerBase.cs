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
    /// <summary>��Ϣ�ַ����б�</summary>
    private static Dictionary<string, Delegate> netMessageDict = new Dictionary<string, Delegate>();

    /// <summary>
    /// ��ʼ��Ϣ��ǲ
    /// </summary>
    public static void StartNetMessageHandle()
    {
        //��ȡ��ǰ����
        Assembly assembly = Assembly.GetExecutingAssembly();
        //�ҵ����м̳���NetMessageHandlerBase<TNetMessage>���࣬������NetMessage.***_Handler��
        foreach (Type type in assembly.GetTypes())
        {
            if (type.BaseType.Name == nameof(NetMessageHandlerBase<TNetMessage>) + "`1")//����̳���NetMessageBase<TNetMessage>��"`1"ָ��һ������
            {
                //ʵ����һ������
                object obj = Activator.CreateInstance(type);
                //��ȡ������OnMessage����
                MethodInfo methodInfo = type.GetMethod(nameof(OnMessage));
                //��ȡ�����Ļ����еķ��࣬��NetMessage.***��
                Type genericType = type.BaseType.GetGenericArguments()[0];
                //����һ��Actionί�У���OnMessage����������ͬ��ǩ��
                Delegate delegateForMethod = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(genericType), obj, methodInfo);
                //����������ί����ӵ��ֵ��У�����Ϣ��ǲʹ��
                netMessageDict.Add(genericType.Name, delegateForMethod);
            }
        }
    }

    /// <summary>
    /// �����пͻ��˷�����Ϣ
    /// </summary>
    /// <param name="netMessage"></param>
    /// <param name="negativeClient">ָ���ĸ��ͻ��˲���Ҫ�㲥</param>
    protected static void SendMessage(TNetMessage netMessage)
    {
        //����Json����
        JObject jObject = new JObject();
        //���л���Ϣ
        jObject[netMessage.GetType().Name] = JsonConvert.SerializeObject(netMessage);
        //������Ϣ
        Network.SendMessage(jObject);
    }

    /// <summary>
    /// ��д�˷��������յ���ǰ���͵���Ϣʱ�ͻᣬ�˷����ͻᱻ����
    /// </summary>
    public virtual void OnMessage(TNetMessage netMessageBase)
    {

    }

    /// <summary>
    /// ��Ϣ��ǲ
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="netMessage"></param>
    public static void Dispatch(string message)
    {
        //������Ϣ
        JObject jObject = JObject.Parse(message);
        //��ȡ��Ϣ��������
        string messageObjectName = jObject.Children<JProperty>().First().Name;

        if (!netMessageDict.ContainsKey(messageObjectName))//û�ж�Ӧ��Ϣ
        {
            Debug.Log($"����û����Ϣ��{messageObjectName}");
            return;
        }

        //��ȡ��ǰ����
        Assembly assembly = Assembly.GetExecutingAssembly();
        //�����л�������
        Type deserializeType = null;
        //���Ի�ȡNetMessageBase����
        deserializeType = Type.GetType(typeof(NetMessageBase).FullName + "+" + messageObjectName);
        //����NetMessageBase����
        if (deserializeType == null)
        {
            //�ҵ����м̳���NetMessageBase����
            foreach (Type type in assembly.GetTypes())
            {
                //����̳���NetMessageBase
                if (type.BaseType.Name == nameof(NetMessageBase))
                {
                    //���Ի�ȡ���ͣ�ָ��assembly.GetName()�ɿ���򼯣�
                    deserializeType = Type.GetType(type.FullName + "+" + messageObjectName);
                    if (deserializeType != null) 
                        break;
                }
            }
        }
        if (deserializeType == null)
        {
            Debug.Log("deserializeType�쳣��");
        }

        //�����л��ַ����õ�TNetMessage���͵Ķ���
        object netMessage = JsonConvert.DeserializeObject(jObject[messageObjectName].ToString(), deserializeType);
        //֪ͨ��Ϣ
        netMessageDict[messageObjectName].DynamicInvoke(netMessage);

    }

}
