using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]

    public class AfterSlotCreate_CreateSlotViewEvent : AEvent<Scene , AfterCreateSlot>
    {
        protected override async ETTask Run(Scene scene, AfterCreateSlot data)
        {
            Slot slot = data.slot;
            
            //生成预制体
            var bundleObj = await ResourcesLoaderHelper.LoadAssetPrefabAsync<GameObject>(scene, slot.Config().PrefabPath);
            
            var Obj = UnityEngine.Object.Instantiate(bundleObj , slot.GetParent<Grid>().GetComponent<GridView>().slotTransform);
            
            //生成View
            var slotView = slot.AddComponent<SlotView,Transform>(Obj.transform);
            
            await ETTask.CompletedTask;
        }
    }
}
