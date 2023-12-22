using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NewTestScript
{
    private EatTileManager eatTileManager;

    [SetUp]
    public void SetUp()
    {
        eatTileManager = new EatTileManager();
    }

    // A Test behaves as an ordinary method
    [Test]
    public void NewTestScriptSimplePasses()
    {
        //新建输入数据
        List<MahJongType> mahJongTypeList = new List<MahJongType>();
        List<List<MahJongType>> eatMahJongTypeList = new List<List<MahJongType>>();

        //设置输入数据
        mahJongTypeList = new List<MahJongType> { MahJongType.Stick4, MahJongType.Stick6, MahJongType.Stick9, MahJongType.Stick9 };
        eatMahJongTypeList = new List<List<MahJongType>>
        {
            new List<MahJongType> { MahJongType.Thousand1, MahJongType.Thousand1 , MahJongType.Thousand1 },
            new List<MahJongType> { MahJongType.Thousand3, MahJongType.Thousand4 , MahJongType.Thousand5 },
            new List<MahJongType> { MahJongType.Thousand7, MahJongType.Thousand8 , MahJongType.Thousand9 },
        };


        Assert.IsTrue(ListenManager.CheckListening(mahJongTypeList, eatMahJongTypeList, out ListeningTilesData listeningTilesData));

        //设置输入数据
        mahJongTypeList = new List<MahJongType> { MahJongType.Circle9, MahJongType.Stick6, MahJongType.Stick9, MahJongType.Stick9 };

        Assert.IsFalse(ListenManager.CheckListening(mahJongTypeList, eatMahJongTypeList, out listeningTilesData));
        
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
