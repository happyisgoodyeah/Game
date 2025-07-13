using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]

    public class AfterSlotCreate_GridCreateSlotViewEvent : AEvent<Scene , AfterCreateSlot>
    {
        protected override async ETTask Run(Scene scene, AfterCreateSlot data)
        {
            Slot slot = data.slot;
            
            //生成预制体
            var bundleObj = await ResourcesLoaderHelper.LoadAssetPrefabAsync<GameObject>(scene, slot.Config().PrefabPath);

            if (slot.GetParent<Grid>() != null)
            {
                var Obj = slot.GetParent<Grid>().GetComponent<GridView>().slotTransform.GetChild(data.count);
            
                //生成View
                var slotView = slot.AddComponent<SlotView,Transform>(Obj.transform);    
                
                Obj.GetComponent<GameObjectEntityRef>().Entity = slotView;
            }
            
            await ETTask.CompletedTask;
        }
    }
}
