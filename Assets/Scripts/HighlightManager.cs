using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// ����ƶ�ʱ���������λ�����ڵ��ơ�
/// </summary>
public class HighlightManager : MonoBehaviour
{
    /// <summary>������ɫ��ĿǰΪ��ɫ</summary>
    private Color highlightColor = new Color(1f, 0.5f, 0f);

    /// <summary>��ǰ���ڸ���������</summary>
    private GameObject currentHighlightGameObject;

    private void Start()
    {
        
    }


    private void Update()
    {
        //������屻���٣�����Ϊnull[*����Ҫ����]
        if (currentHighlightGameObject.IsDestroyed())
        {
            currentHighlightGameObject = null;
        }

        //����Ļ��������
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //��ȡ����������������
            GameObject target = hit.collider.gameObject;

            if (target != currentHighlightGameObject)//�л�����Ҫ����������
            {
                //ȡ����һ������ĸ�����ʾ
                currentHighlightGameObject?.GetComponent<HighlightableObject>()?.ConstantOff();
                //��¼��ǰ����������
                currentHighlightGameObject = target;
                //��������[!��Ҫ����]
                target.GetComponent<HighlightableObject>()?.ConstantOn(highlightColor);
            }

        }
        else
        {
            //ȡ����һ������ĸ�����ʾ
            currentHighlightGameObject?.GetComponent<HighlightableObject>()?.ConstantOff();
            //��һ������Ϊ��
            currentHighlightGameObject = null;
        }
    }





}

