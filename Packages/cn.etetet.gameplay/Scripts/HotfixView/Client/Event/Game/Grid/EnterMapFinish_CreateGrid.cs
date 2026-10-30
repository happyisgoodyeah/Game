using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(LevelSessionComponent))]
    public class EnterMapFinish_CreateGrid : AEvent<Scene , EnterMapFinish>
    {
        protected override async ETTask Run(Scene scene, EnterMapFinish a)
        {
            LevelSessionComponent session = LevelSessionComponentSystem.GetOrCreate(scene);
            if (session.CurrentGrid.Entity != null || session.IsLoading || GridConfigCategory.Instance.DataList.Count == 0)
            {
                return;
            }
            await session.EnterLevelAsync(GridConfigCategory.Instance.DataList[0].Id);
        }
    }
}
