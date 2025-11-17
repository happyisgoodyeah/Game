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
            Grid grid = slot.GetParent<Grid>();
            
            //生成预制体
            var bundleObj = await ResourcesLoaderHelper.LoadAssetPrefabAsync<GameObject>(scene, slot.Config().PrefabPath);
            
            if (grid != null)
            {
                var Obj = UnityEngine.Object.Instantiate(bundleObj, grid.GetComponent<GridView>().slotTransform);
                
                // Obj.name = slot.Config().Id.ToString();
                //非编辑器模式下 加入slot到grid后需要强制调用slotTransform上的mono脚本进行布局刷新
                Obj.transform.SetAsLastSibling();
                
                //todo 改为全部加载完成后刷新一次
                grid.GetComponent<GridView>().slotTransform.GetComponent<CenteredGridLayout>().UpdateLayout();
            
                //生成View
                var slotView = slot.AddComponent<SlotView,Transform>(Obj.transform);    
                
                Obj.GetComponent<GameObjectEntityRef>().Entity = slotView;
            }
            
            await ETTask.CompletedTask;
        }
    }
}
