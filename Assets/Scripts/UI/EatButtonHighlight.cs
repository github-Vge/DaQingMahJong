using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���Ƶ�ʱ������Ƶ����Ƶİ�ť��ʱ����ʾ����Щ��ȥ�ԣ��Ե������ơ��������Ǹ�����õ�
/// </summary>
public class EatButtonHighlight : MonoBehaviour
{

    /// <summary>��Ҫ����������</summary>
    private List<HighlightableObject> highlightObjects = new List<HighlightableObject>();

    /// <summary>
    /// ע����Ҫ����������
    /// </summary>
    /// <param name="highlightGameObjects">��Ҫ����������</param>
    public void RegisterHighlightGameObject(List<GameObject> highlightGameObjects)
    {
        foreach (GameObject gameObject in highlightGameObjects)
        {
            highlightObjects.Add(gameObject.GetComponent<HighlightableObject>());
        }
    }

    /// <summary>
    /// ��������밴ťʱ����
    /// </summary>
    public void OnPointEnter()
    {
        //����
        foreach(HighlightableObject gameObject in highlightObjects)
        {
            gameObject.ConstantOn(new Color(1f, 0f, 0f));
        }
    }
    /// <summary>
    /// ������Ƴ���ťʱ����
    /// </summary>
    public void OnPointExit()
    {
        //ȡ������
        foreach (HighlightableObject gameObject in highlightObjects)
        {
            gameObject.ConstantOff();
        }
    }

    private void OnDestroy()
    {
        //ȡ������[*����Ҫ����]
        foreach (HighlightableObject gameObject in highlightObjects)
        {
            gameObject.ConstantOff();
        }
    }



}
