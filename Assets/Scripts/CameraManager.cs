using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    /// <summary>单例</summary>
    public static CameraManager Instance {  get; private set; }

    [Header("摄像机在顶部的位置（俯视角）")]
    public Transform topCameraTransform;
    /// <summary>四个玩家镜头的初始位置</summary>
    private Vector3[] cameraInitPosition = new Vector3[4];
    /// <summary>四个玩家镜头的初始旋转</summary>
    private readonly Quaternion[] cameraInitRotation = new Quaternion[4];


    private void Awake()
    {
        //初始化单例
        Instance = this;
        //初始化四个玩家的初始摄像头的位置
        cameraInitPosition[0] = new Vector3 (-0.5f, 4.5f, 10f);
        cameraInitRotation[0] = Quaternion.Euler(30f, 180f, 0f);
        cameraInitPosition[1] = new Vector3 (10f, 4.5f, 0.5f);
        cameraInitRotation[1] = Quaternion.Euler(30f, 270f, 0f);
        cameraInitPosition[2] = new Vector3 (0.5f, 4.5f, -10f);
        cameraInitRotation[2] = Quaternion.Euler(30f, 0f, 0f);
        cameraInitPosition[3] = new Vector3(-10f, 4.5f, -0.5f);
        cameraInitRotation[3] = Quaternion.Euler(30f, 90f, 0f);

    }

    /// <summary>
    /// 移动摄像机到指定Id玩家的位置
    /// </summary>
    /// <param name="playerId">玩家的Id，1-4</param>
    public void MoveCameraToPlayer(int playerId)
    {
        //停止摄像机的移动
        Camera.main.transform.DOPause();
        //位置
        Camera.main.transform.position = cameraInitPosition[playerId - 1];
        //旋转
        Camera.main.transform.rotation = cameraInitRotation[playerId - 1];
    }


    private void Update()
    {
        //测试用
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Camera.main.transform.position = cameraInitPosition[0];
            Camera.main.transform.rotation = cameraInitRotation[0];
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Camera.main.transform.position = cameraInitPosition[1];
            Camera.main.transform.rotation = cameraInitRotation[1];
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Camera.main.transform.position = cameraInitPosition[2];
            Camera.main.transform.rotation = cameraInitRotation[2];
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Camera.main.transform.position = cameraInitPosition[3];
            Camera.main.transform.rotation = cameraInitRotation[3];
        }

    }

    /// <summary>
    /// 将摄像机渐渐移到顶部（俯视角）
    /// </summary>
    public void MoveToTop()
    {
        //位置移动到顶部
        Camera.main.transform.DOMove(topCameraTransform.position, 1.5f);
        //获取当前的旋转
        Vector3 rotation = Camera.main.transform.eulerAngles;
        //只让x轴转到90f，其它轴不变
        Camera.main.transform.DORotate(new Vector3(90f, rotation.y, rotation.z), 1.5f);
    }



}
