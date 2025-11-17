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
            
            if (slot.GetParent<Puzzle>() != null)
            {
                //生成预制体
                var bundleObj = await ResourcesLoaderHelper.LoadAssetPrefabAsync<GameObject>(scene, slot.Config().PrefabPath);
                await ETTaskHelperExtend.WaitUntil(slot , () => slot.GetParent<Puzzle>().GetComponent<PuzzleView>() != null);
                var Obj = UnityEngine.Object.Instantiate(bundleObj , slot.GetParent<Puzzle>().GetComponent<PuzzleView>().transform);
            
                //生成View
                var slotView = slot.AddComponent<SlotView,Transform>(Obj.transform);

                Obj.GetComponent<GameObjectEntityRef>().Entity = slotView;
            }
            
            await ETTask.CompletedTask;
        }
    }
}
