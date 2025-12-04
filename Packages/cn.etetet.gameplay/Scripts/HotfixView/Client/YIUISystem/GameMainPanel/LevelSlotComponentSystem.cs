using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  Behappy
    /// Date    2025.11.28
    /// Desc
    /// </summary>
    [FriendOf(typeof(LevelSlotComponent))]
    public static partial class LevelSlotComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LevelSlotComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LevelSlotComponent self)
        {
        }

        #region YIUIEvent开始

        [YIUIInvoke(LevelSlotComponent.OnEventClickLevelInvoke)]
        private static async ETTask OnEventClickLevelInvoke(this LevelSlotComponent self)
        {
            var grid = self.Root().AddChild<Grid, int>(1001);
            await EventSystem.Instance.PublishAsync(self.Root(), new AfterCreateGrid() { grid = grid });
            await grid.SpawnSlot();
            grid.SpawnPuzzle();

            await self.DynamicEvent(self.Root(), new SelectLevelView_LevelSlotGoGrid());
        }

        #endregion YIUIEvent结束

        /// <summary>
        /// 刷新ui相关
        /// 设置状态应该放在上层生成LevelSlot的时候判断
        /// </summary>
        /// <param name="self"></param>
        /// <param name="gridConfig"></param>
        public static void UpdateData(this LevelSlotComponent self, GridConfig gridConfig)
        {
            var playerData = self.Root().GetComponent<SaveManagerComponent>().GetPlayerData();

            //关卡
            self.u_ComU_LevelCountText.SetText((gridConfig.Id - 1000).ToString());

            //是否解锁
            var unlock = playerData.CheckLevelUnlock(gridConfig.Id);
            self.u_ComU_MaskRect.gameObject.SetActive(!unlock);
            self.u_ComU_LevelSlotBtn.enabled = unlock;

            //通过
            var pass = playerData.CheckLevelPass(gridConfig.Id);
            self.u_ComU_IsPassRect.gameObject.SetActive(pass);
        }

        public static void SetUnlockState(this LevelSlotComponent self, bool isUnlock)
        {
            self.u_ComU_MaskRect.gameObject.SetActive(!isUnlock);
            self.u_ComU_LevelSlotBtn.enabled = isUnlock;
        }

        public static void SetPassState(this LevelSlotComponent self, bool isPass)
        {
            self.u_ComU_IsPassRect.gameObject.SetActive(isPass);
        }
    }
}