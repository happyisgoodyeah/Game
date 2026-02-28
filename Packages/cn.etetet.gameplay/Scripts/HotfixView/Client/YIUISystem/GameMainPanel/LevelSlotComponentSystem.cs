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
            var grid = self.Root().AddChild<Grid, int>(self.gridConfigId);
            
            await EventSystem.Instance.PublishAsync(self.Root(), new AfterCreateGrid() { grid = grid });
            await grid.SpawnSlot();
            grid.SpawnPuzzle();

            await self.DynamicEvent(self.Root(), new SelectLevelView_LevelSlotGoGrid());
        }

        #endregion YIUIEvent结束
        
        /// <summary>
        /// 设置slot相关信息
        /// </summary>
        /// <param name="self"></param>
        /// <param name="isUnlock"></param>
        /// <param name="isPass"></param>
        /// <param name="id"></param>
        public static void SetSlotState(this LevelSlotComponent self, bool isUnlock, bool isPass, int id)
        {
            //是否解锁
            self.u_ComU_MaskRect.gameObject.SetActive(!isUnlock);
            self.u_ComU_LevelSlotBtn.enabled = isUnlock;
            self.u_ComU_LevelSlotBtn.interactable = isUnlock;

            //是否通过
            self.u_ComU_IsPassRect.gameObject.SetActive(isPass);

            //关卡
            self.u_ComU_LevelCountText.SetText((id - 1000).ToString());
            self.gridConfigId = id;
        }
    }
}