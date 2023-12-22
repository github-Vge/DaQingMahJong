using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 每个麻将都有这个组件
/// </summary>
public class MahjongTile : MonoBehaviour
{
    /// <summary>麻将的类型</summary>
    public MahJongType MahJongType {  get; set; }
    /// <summary>麻将是否是宝</summary>
    public bool IsTreasure { get; set; }

}
