﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TowerTemplate : ScriptableObject
{
    public GameObject towerPrefab; // 타워 생성을 위한 프리팹
    public Weapon[] weapon; // 래벨별 타워(무기) 정보

    [Serializable]
    public struct Weapon
    {
        [SerializeField]
        private Sprite sprite; // 보여지는 타워 이미지 (UI)
        [SerializeField]
        private float damage; // 공격력
        [SerializeField]
        private float rate; // 공격 속도
        [SerializeField]
        private float range; // 공격 범위
        [SerializeField]
        private int cost; //필요 골드 (0래벨 :건설, 1~래벨 업그레이드)
        [SerializeField]
        private int sell; //타워 판매 시 획득 골드

        public Sprite Sprite => sprite;
        public float Damage => damage;
        public float Rate => rate;
        public float Range => range;
        public int Cost => cost;
        public int Sell => sell;
    }

}