using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    /// <summary>����</summary>
    public static CameraManager Instance {  get; private set; }

    [Header("������ڶ�����λ�ã����ӽǣ�")]
    public Transform topCameraTransform;
    /// <summary>�ĸ���Ҿ�ͷ�ĳ�ʼλ��</summary>
    private Vector3[] cameraInitPosition = new Vector3[4];
    /// <summary>�ĸ���Ҿ�ͷ�ĳ�ʼ��ת</summary>
    private readonly Quaternion[] cameraInitRotation = new Quaternion[4];


    private void Awake()
    {
        //��ʼ������
        Instance = this;
        //��ʼ���ĸ���ҵĳ�ʼ����ͷ��λ��
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
    /// �ƶ��������ָ��Id��ҵ�λ��
    /// </summary>
    /// <param name="playerId">��ҵ�Id��1-4</param>
    public void MoveCameraToPlayer(int playerId)
    {
        //ֹͣ��������ƶ�
        Camera.main.transform.DOPause();
        //λ��
        Camera.main.transform.position = cameraInitPosition[playerId - 1];
        //��ת
        Camera.main.transform.rotation = cameraInitRotation[playerId - 1];
    }


    private void Update()
    {
        //������
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
    /// ������������Ƶ����������ӽǣ�
    /// </summary>
    public void MoveToTop()
    {
        //λ���ƶ�������
        Camera.main.transform.DOMove(topCameraTransform.position, 1.5f);
        //��ȡ��ǰ����ת
        Vector3 rotation = Camera.main.transform.eulerAngles;
        //ֻ��x��ת��90f�������᲻��
        Camera.main.transform.DORotate(new Vector3(90f, rotation.y, rotation.z), 1.5f);
    }



}
