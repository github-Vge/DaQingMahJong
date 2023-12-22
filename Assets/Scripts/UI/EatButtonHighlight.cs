using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 吃牌的时候，鼠标移到吃牌的按钮上时会显示用哪些牌去吃，吃的哪张牌。这个类就是干这个用的
/// </summary>
public class EatButtonHighlight : MonoBehaviour
{

    /// <summary>需要高亮的物体</summary>
    private List<HighlightableObject> highlightObjects = new List<HighlightableObject>();

    /// <summary>
    /// 注册需要高亮的物体
    /// </summary>
    /// <param name="highlightGameObjects">需要高亮的物体</param>
    public void RegisterHighlightGameObject(List<GameObject> highlightGameObjects)
    {
        foreach (GameObject gameObject in highlightGameObjects)
        {
            highlightObjects.Add(gameObject.GetComponent<HighlightableObject>());
        }
    }

    /// <summary>
    /// 当鼠标移入按钮时调用
    /// </summary>
    public void OnPointEnter()
    {
        //高亮
        foreach(HighlightableObject gameObject in highlightObjects)
        {
            gameObject.ConstantOn(new Color(1f, 0f, 0f));
        }
    }
    /// <summary>
    /// 当鼠标移出按钮时调用
    /// </summary>
    public void OnPointExit()
    {
        //取消高亮
        foreach (HighlightableObject gameObject in highlightObjects)
        {
            gameObject.ConstantOff();
        }
    }

    private void OnDestroy()
    {
        //取消高亮[*不重要代码]
        foreach (HighlightableObject gameObject in highlightObjects)
        {
            gameObject.ConstantOff();
        }
    }



}
