using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 选择麻将的管理类，即点击鼠标时，判断鼠标点的是哪张牌。
/// </summary>
public class ChooseMahJongManager : MonoBehaviour
{
    /// <summary>单例</summary>
    public static ChooseMahJongManager Instance;


    private void Awake()
    {
        //初始化单例
        Instance = this;
    }

    /// <summary>当前选中的物体</summary>
    private Transform mCurrentObject;


    private void Update()
    {
        //安卓平台
        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // 创建一条点击位置为光标位置的射线
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            //创建一个RayCast变量用于存储返回信息
            RaycastHit hit;
            //将创建的射线投射出去并将反馈信息存储到hit中
            if (Physics.Raycast(ray, out hit))
            {
                //获取被射线碰到的对象transfrom变量
                mCurrentObject = hit.transform;
            }

            if (hit.transform != null
                && MainPlayer.Instance.State == PlayerState.Playing // 玩家只能在出牌回合才能出牌
                && MahJongManager.Instance.mPlayerGameObjects[MainPlayer.Instance.PlayerId].tiles.Contains(mCurrentObject.gameObject) // 是玩家的手牌
                )
            {
                //获取麻将类型
                MahJongType mahJongType = MahJongManager.Instance.GetMahJongType(mCurrentObject.gameObject);
                //玩家打出牌[!重要代码]
                MahJongTilesManager.Instance.PlayTile(MainPlayer.Instance.PlayerId, mahJongType);
            }

            return;
        }
        //PC平台
        {
            // 创建一条点击位置为光标位置的射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //将射线以黄色的表示出来
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
            //创建一个RayCast变量用于存储返回信息
            RaycastHit hit;
            //将创建的射线投射出去并将反馈信息存储到hit中
            if (Physics.Raycast(ray, out hit))
            {
                //获取被射线碰到的对象transfrom变量
                mCurrentObject = hit.transform;
            }

            if (Input.GetMouseButtonDown(0) && hit.transform != null //点击鼠标左键
                && MainPlayer.Instance.State == PlayerState.Playing // 玩家只能在出牌回合才能出牌
                && MahJongManager.Instance.mPlayerGameObjects[MainPlayer.Instance.PlayerId].tiles.Contains(mCurrentObject.gameObject) // 是玩家的手牌
                )
            {
                //获取麻将类型
                MahJongType mahJongType = MahJongManager.Instance.GetMahJongType(mCurrentObject.gameObject);
                //玩家打出牌[!重要代码]
                MahJongTilesManager.Instance.PlayTile(MainPlayer.Instance.PlayerId, mahJongType);

            }
        }



        
    }




}
