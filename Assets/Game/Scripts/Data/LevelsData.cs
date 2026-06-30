using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelsData : Singleton<LevelsData>
{
    public LinkedList<MonsterWave> level1Waves = new LinkedList<MonsterWave>(); //第一关波次
    public LinkedList<MonsterWave> level2Waves = new LinkedList<MonsterWave>(); //第二关波次
    public LinkedList<MonsterWave> level3Waves = new LinkedList<MonsterWave>(); //第三关波次
    public LinkedList<MonsterWave> level4Waves = new LinkedList<MonsterWave>(); //第四关波次

    public void OnInitLevels()
    {

    }

    public void OnInitLevel1()
    {
        MonsterWave wave = new MonsterWave();
    }

}
// 怪物波次信息
public class MonsterWave
{
    public int monsterCount;      // 怪物数量
    public int monsterType;       // 怪物类型
    public MonseterAttr monseterAttrs; // 怪物属性
    public float interval;        // 与上一波的间隔时间
}
public class MonseterAttr
{
    int hp = 1;
    float flySpeed = 2;
}