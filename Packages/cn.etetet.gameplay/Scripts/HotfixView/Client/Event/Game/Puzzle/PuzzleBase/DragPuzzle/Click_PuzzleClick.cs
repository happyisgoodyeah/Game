using DG.Tweening;
using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Slot))]
    [FriendOf(typeof(Puzzle))]
    public class Click_PuzzleClick : AEvent<Scene, ClickRotateEvent>
    {
        protected override async ETTask Run(Scene scene, ClickRotateEvent data)
        {
            //拖拽的时候进行旋转操作
            if (data.Entity is PuzzleView puzzleView)
            {
                Puzzle puzzle = puzzleView.GetParent<Puzzle>();

                if (puzzle.bindSlots.Count == 0)
                {
                    // 未放置状态：直接旋转
                    puzzle.RotatePuzzle(90);
                    EventSystem.Instance.Publish(scene,
                        new ClickPuzzleRotateEvent() { puzzle = puzzle, angle = data.Angle });
                }
                else
                {
                    // 已放置状态：检查旋转合法性
                    Grid grid = puzzle.GetParent<Grid>();
                    
                    puzzleView.tweener?.Kill();
                    puzzleView.tweener = null;

                    // 获取当前世界坐标位置（HotfixView层可以访问View）
                    var worldPosition = puzzleView.transform.position;
                    FloatVector2 position = new FloatVector2(worldPosition.x, worldPosition.y);

                    if (grid.TryRotatePlacedPuzzle(puzzle, 90, position, out IntVector2 originPosition))
                    {
                        // 旋转成功：更新视图位置并播放旋转动画
                        puzzleView.transform.position = grid.GetSlot(originPosition).GetComponent<SlotView>().transform.position;
                        puzzle.RotatePuzzleView();
                    }
                    else
                    {
                        // 旋转失败：播放摇晃动画
                        Log.Info("rotate illegality");
                        puzzle.ShakePuzzleView();
                    }
                }
            }

            await ETTask.CompletedTask;
        }
    }
}