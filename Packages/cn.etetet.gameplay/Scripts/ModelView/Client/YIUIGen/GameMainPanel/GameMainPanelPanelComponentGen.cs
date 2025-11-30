using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// 当前Panel所有可用view枚举
    /// </summary>
    public enum EGameMainPanelPanelViewEnum
    {
        SelectLevelView = 1,
        Main1View = 2,
    }
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Panel, EPanelLayer.Panel)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class GameMainPanelPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "GameMainPanel";
        public const string ResName = "GameMainPanelPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.UI.Button u_ComSettingBtnButton;
        public UIEventP0 u_EventOpenSetting;
        public UIEventHandleP0 u_EventOpenSettingHandle;
        public const string OnEventOpenSettingInvoke = "GameMainPanelPanelComponent.OnEventOpenSettingInvoke";

    }
}