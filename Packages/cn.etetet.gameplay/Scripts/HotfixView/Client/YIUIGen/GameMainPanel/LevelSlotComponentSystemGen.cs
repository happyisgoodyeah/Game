using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [FriendOf(typeof(YIUIChild))]
    [EntitySystemOf(typeof(LevelSlotComponent))]
    public static partial class LevelSlotComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LevelSlotComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this LevelSlotComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this LevelSlotComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();

            self.u_ComU_IsPassRect = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComU_IsPassRect");
            self.u_ComU_MaskRect = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComU_MaskRect");
            self.u_ComU_LevelSlotBtn = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComU_LevelSlotBtn");
            self.u_ComU_LevelCountText = self.UIBase.ComponentTable.FindComponent<TMPro.TextMeshProUGUI>("u_ComU_LevelCountText");
            self.u_EventClickLevel = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventClickLevel");
            self.u_EventClickLevelHandle = self.u_EventClickLevel.Add(self,LevelSlotComponent.OnEventClickLevelInvoke);

        }
    }
}
