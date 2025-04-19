using ET.Client;

namespace ET
{
    [Event(SceneType.StateSync)]
    public class StartGameEvent_SceneChange : AEvent<Scene, GameStart>
    {
        protected override async ETTask Run(Scene root, GameStart a)
        {
            //切换场景
            await SceneChangeHelper.SceneChangeTo(root, "Map1", root.InstanceId);
        }
    }
}