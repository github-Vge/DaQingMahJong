using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 鼠标移动时，高亮鼠标位置所在的牌。
/// </summary>
public class HighlightManager : MonoBehaviour
{
    /// <summary>高亮颜色，目前为橙色</summary>
    private Color highlightColor = new Color(1f, 0.5f, 0f);

    /// <summary>当前正在高亮的物体</summary>
    private GameObject currentHighlightGameObject;

    private void Start()
    {
        
    }


    private void Update()
    {
        //如果物体被销毁，则置为null[*不重要代码]
        if (currentHighlightGameObject.IsDestroyed())
        {
            currentHighlightGameObject = null;
        }

        //向屏幕发射射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //获取玩家鼠标点击到的物体
            GameObject target = hit.collider.gameObject;

            if (target != currentHighlightGameObject)//切换了需要高亮的物体
            {
                //取消上一个物体的高亮显示
                currentHighlightGameObject?.GetComponent<HighlightableObject>()?.ConstantOff();
                //记录当前高亮的物体
                currentHighlightGameObject = target;
                //高亮物体[!重要代码]
                target.GetComponent<HighlightableObject>()?.ConstantOn(highlightColor);
            }

        }
        else
        {
            //取消上一个物体的高亮显示
            currentHighlightGameObject?.GetComponent<HighlightableObject>()?.ConstantOff();
            //上一个物体为空
            currentHighlightGameObject = null;
        }
    }





}

