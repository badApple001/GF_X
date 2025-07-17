using GameFramework;
using GameFramework.Event;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;


public class MapEntity : EntityBase
{
    private Transform playerSpawnPoint;
    public bool IsAllReady { get; private set; }

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        playerSpawnPoint = transform.Find("PlayerSpawnPoint");

    }

    protected override async void OnShow(object userData)
    {
        base.OnShow(userData);
        IsAllReady = false;

        var combatUnitTb = GF.DataTable.GetDataTable<CombatUnitTable>();
        var playerRow = combatUnitTb.GetDataRow(0);
        var characterParams = EntityParams.Create(playerSpawnPoint.position, playerSpawnPoint.eulerAngles);
        characterParams.Set(PlayerEntity.P_DataTableRow, playerRow);
        characterParams.Set<VarInt32>(PlayerEntity.P_CombatFlag, (int)CombatUnitEntity.CombatFlag.Player);
        // characterParams.Set<VarAction>(PlayerEntity.P_OnBeKilled, (Action)OnPlayerBeKilled);
        var characterEntity = await GF.Entity.ShowEntityAwait<CharacterEntity>("Characters/Character_0", Const.EntityGroup.Player, characterParams) as CharacterEntity;
        Debug.Assert(characterEntity != null, "实例化 CharacterEntity 失败...");

        IsAllReady = true;
    }

    public void StartGame()
    {
        // m_PlayerEntity.Ctrlable = true;
        Debug.Log("游戏开始");
    }
}
