using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Common)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class LevelSlotComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize
    {
        public const string PkgName = "GameMainPanel";
        public const string ResName = "LevelSlot";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public UnityEngine.RectTransform u_ComU_IsPassRect;
        public UnityEngine.RectTransform u_ComU_MaskRect;
        public UnityEngine.UI.Button u_ComU_LevelSlotBtn;
        public TMPro.TextMeshProUGUI u_ComU_LevelCountText;
        public UITaskEventP0 u_EventClickLevel;
        public UITaskEventHandleP0 u_EventClickLevelHandle;
        public const string OnEventClickLevelInvoke = "LevelSlotComponent.OnEventClickLevelInvoke";

    }
}