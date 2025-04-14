using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;
using YIUIFramework;
using YIUIFramework.Editor;

namespace YIUILuban.Editor
{
    public class LubanToolModule : BaseYIUIToolModule
    {
        [ShowInInspector]
        [HideLabel]
        [HideReferenceObjectPicker]
        public LubanToolRoot ToolRoot;

        private void RefreshAllLuban()
        {
            ToolRoot = new LubanToolRoot();
            ToolRoot.RefreshAllLuban(this.AutoTool, this.Tree);
        }

        public override void Initialize()
        {
            this.RefreshAllLuban();
        }

        public override void OnDestroy()
        {
        }
    }
}