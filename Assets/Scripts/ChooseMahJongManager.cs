using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ѡ���齫�Ĺ����࣬��������ʱ���ж�������������ơ�
/// </summary>
public class ChooseMahJongManager : MonoBehaviour
{
    /// <summary>����</summary>
    public static ChooseMahJongManager Instance;


    private void Awake()
    {
        //��ʼ������
        Instance = this;
    }

    /// <summary>��ǰѡ�е�����</summary>
    private Transform mCurrentObject;


    private void Update()
    {
        //��׿ƽ̨
        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // ����һ�����λ��Ϊ���λ�õ�����
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            //����һ��RayCast�������ڴ洢������Ϣ
            RaycastHit hit;
            //������������Ͷ���ȥ����������Ϣ�洢��hit��
            if (Physics.Raycast(ray, out hit))
            {
                //��ȡ�����������Ķ���transfrom����
                mCurrentObject = hit.transform;
            }

            if (hit.transform != null
                && MainPlayer.Instance.State == PlayerState.Playing // ���ֻ���ڳ��ƻغϲ��ܳ���
                && MahJongManager.Instance.mPlayerGameObjects[MainPlayer.Instance.PlayerId].tiles.Contains(mCurrentObject.gameObject) // ����ҵ�����
                )
            {
                //��ȡ�齫����
                MahJongType mahJongType = MahJongManager.Instance.GetMahJongType(mCurrentObject.gameObject);
                //��Ҵ����[!��Ҫ����]
                MahJongTilesManager.Instance.PlayTile(MainPlayer.Instance.PlayerId, mahJongType);
            }

            return;
        }
        //PCƽ̨
        {
            // ����һ�����λ��Ϊ���λ�õ�����
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //�������Ի�ɫ�ı�ʾ����
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
            //����һ��RayCast�������ڴ洢������Ϣ
            RaycastHit hit;
            //������������Ͷ���ȥ����������Ϣ�洢��hit��
            if (Physics.Raycast(ray, out hit))
            {
                //��ȡ�����������Ķ���transfrom����
                mCurrentObject = hit.transform;
            }

            if (Input.GetMouseButtonDown(0) && hit.transform != null //���������
                && MainPlayer.Instance.State == PlayerState.Playing // ���ֻ���ڳ��ƻغϲ��ܳ���
                && MahJongManager.Instance.mPlayerGameObjects[MainPlayer.Instance.PlayerId].tiles.Contains(mCurrentObject.gameObject) // ����ҵ�����
                )
            {
                //��ȡ�齫����
                MahJongType mahJongType = MahJongManager.Instance.GetMahJongType(mCurrentObject.gameObject);
                //��Ҵ����[!��Ҫ����]
                MahJongTilesManager.Instance.PlayTile(MainPlayer.Instance.PlayerId, mahJongType);

            }
        }



        
    }




}
