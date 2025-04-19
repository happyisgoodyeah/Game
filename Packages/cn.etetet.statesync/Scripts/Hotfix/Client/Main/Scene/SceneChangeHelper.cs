namespace ET.Client
{
    public static partial class SceneChangeHelper
    {
        // 场景切换协程
        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="root">场景</param>
        /// <param name="sceneName">场景名</param>
        /// <param name="sceneInstanceId">InstanceID</param>
        public static async ETTask SceneChangeTo(Scene root, string sceneName, long sceneInstanceId)
        {
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            // 删除之前的CurrentScene，创建新的
            currentScenesComponent.Scene?.Dispose();
            //创建新的scene并绑定
            Scene currentScene = CurrentSceneFactory.Create(sceneInstanceId, sceneName, currentScenesComponent);
            
            // 可以订阅这个事件中创建Loading界面
            EventSystem.Instance.Publish(root, new SceneChangeStart());
            EventSystem.Instance.Publish(currentScene, new SceneChangeFinish());

            await root.GetComponent<TimerComponent>().WaitAsync(1);
            
            // 通知等待场景切换的协程
            root.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
            await ETTask.CompletedTask;
            
            // 等待CreateMyUnit的消息
            //UnitComponent unitComponent = currentScene.AddComponent<UnitComponent>();
            //Wait_CreateMyUnit waitCreateMyUnit = await root.GetComponent<ObjectWait>().Wait<Wait_CreateMyUnit>();
            //M2C_CreateMyUnit m2CCreateMyUnit = waitCreateMyUnit.Message;
            //Unit unit = UnitFactory.Create(currentScene, m2CCreateMyUnit.Unit);
            //unitComponent.Add(unit);
        }
    }
}