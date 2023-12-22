using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainUI : MonoBehaviour
{
    /// <summary>����</summary>
    public static MainUI Instance;

    [Header("�����")]
    public GameObject mainPanel;

    [Header("ʣ�������ı�")]
    public TextMeshProUGUI mRemainingTileCountText;
    /// <summary>ʣ������</summary>
    private int remainingTileCount = 112;
    /// <summary>ʣ������������UI��ʾ˫���</summary>
    [HideInInspector]
    public int RemainingTileCount
    {
        get { return remainingTileCount; }
        set { remainingTileCount = value; mRemainingTileCountText.text = $"ʣ��������<b>{remainingTileCount}</b>"; }
    }

    private void Awake()
    {
        //��ʼ������
        Instance = this;
    }






}
