using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  Behappy
    /// Date    2025.11.26
    /// Desc
    /// </summary>
    [FriendOf(typeof(SelectLevelViewComponent))]
    public static partial class SelectLevelViewComponentSystem
    {
        [EntitySystem]
        private static async ETTask DynamicEvent(this ET.Client.SelectLevelViewComponent self, ET.SelectLevelView_LevelSlotGoGrid dynamicEvent)
        {
            self.UIView.Close();
            await YIUIMgrComponent.Inst.Root.OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum
                    .GameView);
            await ETTask.CompletedTask;
        }

        [EntitySystem]
        private static void YIUIInitialize(this SelectLevelViewComponent self)
        {
            self.nowPage = 1;
            self.maxPage = GridConfigCategory.Instance.DataList.Count / 12 + 1;
            self.levelSlotComponents = new List<EntityRef<LevelSlotComponent>>();
        }

        [EntitySystem]
        private static void Destroy(this SelectLevelViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this SelectLevelViewComponent self)
        {
            self.Refresh();

            await ETTask.CompletedTask;
            return true;
        }

        /// <summary>
        /// 刷新ui
        /// </summary>
        /// <param name="self"></param>
        public static void Refresh(this SelectLevelViewComponent self)
        {
            self.RefreshBtn();
            self.u_ComU_TitleText.SetText($"第{Utility.ConvertToChinese(self.nowPage)}章");
            self.RefreshSlots();
        }

        /// <summary>
        /// 刷新按钮
        /// </summary>
        public static void RefreshBtn(this SelectLevelViewComponent self)
        {
            self.u_ComU_LeftBtn.gameObject.SetActive(self.nowPage - 1 != 0);
            self.u_ComU_RightBtn.gameObject.SetActive(self.nowPage != self.maxPage);
        }

        /// <summary>
        /// 刷新ui 3*4 12格grid layout group
        /// </summary>
        /// <param name="self"></param>
        /// <param name="page">页码</param>
        public static void RefreshSlots(this SelectLevelViewComponent self)
        {
            PlayerDataComponent playerData = self.Root().GetComponent<SaveManagerComponent>().GetPlayerData();

            var start = 12 * (self.nowPage - 1) + 1;

            foreach (var slot in self.levelSlotComponents)
            {
                slot.Entity.UIBase.OwnerGameObject.SetActive(false);
            }

            for (int i = 0; i < 12; i++)
            {
                var now = i + start;

                if (now > GridConfigCategory.Instance.DataList.Count)
                {
                    return;
                }

                var gridConfig = GridConfigCategory.Instance.Get(1000 + now);

                LevelSlotComponent levelSlotComponent;

                if (i < self.levelSlotComponents.Count)
                {
                    levelSlotComponent = self.levelSlotComponents[i];
                }
                else
                {
                    //生成Slot并且判断关卡状态
                    levelSlotComponent = YIUIFactory.Instantiate<LevelSlotComponent>(self, self.u_ComU_LevelContent);
                    self.levelSlotComponents.Add(levelSlotComponent);
                }

                levelSlotComponent.UIBase.OwnerGameObject.SetActive(true);
                //levelSlotComponent.UpdateData(gridConfig);
                levelSlotComponent.SetPassState(playerData.CheckLevelPass(gridConfig.Id));
                levelSlotComponent.SetUnlockState(playerData.CheckLevelUnlock(gridConfig.Id));
            }
        }

        #region YIUIEvent开始

        [YIUIInvoke(SelectLevelViewComponent.OnEventU_LeftBtnClickInvoke)]
        private static void OnEventU_LeftBtnClickInvoke(this SelectLevelViewComponent self)
        {
            self.nowPage--;
            self.Refresh();
        }

        [YIUIInvoke(SelectLevelViewComponent.OnEventBackMenuBtnClickInvoke)]
        private static async ETTask OnEventBackMenuBtnClickInvoke(this SelectLevelViewComponent self)
        {
            await YIUIMgrComponent.Inst.Root.OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum
                    .Main1View);
            await ETTask.CompletedTask;
        }

        [YIUIInvoke(SelectLevelViewComponent.OnEventU_RightBtnClickInvoke)]
        private static void OnEventU_RightBtnClickInvoke(this SelectLevelViewComponent self)
        {
            self.nowPage++;
            self.Refresh();
        }

        #endregion YIUIEvent结束
    }
}