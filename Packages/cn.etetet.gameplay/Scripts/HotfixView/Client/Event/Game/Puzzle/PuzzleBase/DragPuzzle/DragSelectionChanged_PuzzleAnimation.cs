using PuzzleGame;

namespace ET
{
    /// <summary>选中反馈与拖拽判定分离，短按旋转和长按拖拽都能播放拎起动画。</summary>
    [Event(SceneType.StateSync)]
    public class DragSelectionChanged_PuzzleAnimation : AEvent<Scene, DragSelectionChangedEvent>
    {
        protected override async ETTask Run(Scene scene, DragSelectionChangedEvent data)
        {
            if (data.Entity is PuzzleView view && !view.IsDisposed && view.transform != null)
                view.transform.GetComponent<PuzzleIdleAnimation>()?.SetPickedUp(data.Selected);
            await ETTask.CompletedTask;
        }
    }
}
