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
                EntityRef<Slot> slotRef = slot;
                EntityRef<Puzzle> puzzleRef = slot.GetParent<Puzzle>();
                EntityRef<TimerComponent> timerRef = scene.Root().GetComponent<TimerComponent>();
                //生成预制体
                var bundleObj = await ResourcesLoaderHelper.LoadAssetPrefabAsync<GameObject>(scene, slot.Config().PrefabPath);
                Puzzle puzzle = puzzleRef;
                slot = slotRef;
                if (slot == null || puzzle == null)
                {
                    return;
                }
                while (puzzle.GetComponent<PuzzleView>() == null)
                {
                    TimerComponent timer = timerRef;
                    if (timer == null)
                    {
                        return;
                    }
                    await timer.WaitFrameAsync();
                    slot = slotRef;
                    puzzle = puzzleRef;
                    if (slot == null || puzzle == null)
                    {
                        return;
                    }
                }
                var Obj = UnityEngine.Object.Instantiate(bundleObj , puzzle.GetComponent<PuzzleView>().transform);
            
                //生成View
                var slotView = slot.AddComponent<SlotView,Transform>(Obj.transform);
                var gameObjectComponent = slot.AddComponent<GameObjectComponent>();
                gameObjectComponent.GameObject = Obj;

                Obj.GetComponent<GameObjectEntityRef>().Entity = slotView;
            }
            
            await ETTask.CompletedTask;
        }
    }
}
