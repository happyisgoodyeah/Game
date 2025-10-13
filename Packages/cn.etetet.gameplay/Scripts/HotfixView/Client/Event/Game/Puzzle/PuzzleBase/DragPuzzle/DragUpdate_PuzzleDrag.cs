using DG.Tweening;
using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    [FriendOf(typeof(Grid))]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(PuzzleView))]
    public class DragUpdate_PuzzleDrag : AEvent<Scene, DragUpdateEvent>
    {
        protected override async ETTask Run(Scene scene, DragUpdateEvent data)
        {
            //是Puzzle拖拽事件
            if (data.Entity is PuzzleView puzzleView)
            {
                Puzzle puzzle = puzzleView.GetParent<Puzzle>();

                if (puzzle.moveMode == PuzzleMoveModeType.Normal)
                {
                    if (puzzleView.endPos != data.CurrentPosition)
                    {
                        puzzleView.startPos = puzzleView.endPos;
                        puzzleView.endPos = data.CurrentPosition;

                        if (puzzleView.tweener == null)
                        {
                            puzzleView.tweener = DOTween.To(() => puzzleView.transform.position,
                                        pos => { puzzleView.transform.position = pos; },
                                        puzzleView.endPos,
                                        0.1f)
                                    .SetEase(Ease.Linear)
                                    .OnUpdate(() =>
                                    {
                                        puzzleView.tweener.ChangeEndValue(puzzleView.endPos, 0.1f, true).Play();
                                        if (Vector2.Distance(puzzleView.transform.position, puzzleView.endPos) <= 0.01f)
                                        {
                                            EventSystem.Instance.Publish(scene, new PuzzleMoveEndEvent() { puzzle = puzzle });
                                        }
                                    });
                        }
                    }
                }
                else if (puzzle.moveMode == PuzzleMoveModeType.Adsorption) //吸附模式
                {
                    //原点slot
                    var slot = puzzle.slots[0].Entity;
                    var slotView = slot.GetComponent<SlotView>();
                    var grid = puzzle.GetParent<Grid>();

                    //吸附模式优先判断拼图是否在grid内 若拼图不在grid内 则鼠标位置不能超过 当前拼图对应的操作范围 否则取消吸附模式 进入普通移动模式
                    if (!puzzle.isInGrid)
                    {
                        var range = puzzle.GetPuzzleXYRange();
                        var gridSize = grid.gridSize;
                        var gridSizeX = gridSize.X * grid.cellSize / 2f;
                        var gridSizeY = gridSize.Y * grid.cellSize / 2f;

                        //此时的rangeX.X代表minX rangeX.Y代表maxY
                        var leftRange = -gridSizeX - (range.xRange.Y / 2f + 0.5f) * grid.cellSize;
                        var rightRange = gridSizeX + (range.xRange.X /2f + 0.5f) * grid.cellSize;
                        var topRange = gridSizeY + (range.yRange.X / 2f + 0.5f) * grid.cellSize;
                        var downRange = -gridSizeY - (range.yRange.Y / 2f + 0.5f) * grid.cellSize;

                        var xCan = data.CurrentPosition.x >= leftRange && data.CurrentPosition.x <= rightRange;
                        var yCan = data.CurrentPosition.y >= downRange && data.CurrentPosition.y <= topRange;

                        //如果不满足则直接转换为普通移动模式 退出
                        if (!xCan || !yCan)
                        {
                            puzzleView.tweener?.Kill();
                            puzzleView.tweener = null;
                            puzzle.ChangeMoveMode(PuzzleMoveModeType.Normal);
                            return;
                        }
                    }
                    
                    //如果鼠标位置超出了当前圆点slot坐标slot半径 则向对应方向整体移动
                    var r = slotView.GetComponent<SpriteRenderComponent>().SpriteSize.x;

                    var puzzleOffset = puzzleView.endPos - puzzleView.transform.position;
                    //var distance = Vector2.Distance(data.CurrentPosition, slotView.transform.position + puzzleOffset);
                    // var dis = Mathf.Sqrt(r / 2 * r / 2 + r / 2 * r / 2);
                    
                    if (!Utility.GetOnePointInPointRange(puzzleView.endPos , r / 2 , r / 2, data.CurrentPosition))
                    {
                        //获取对应方向
                        var dir = Utility.GetSnapDirection(puzzleView.endPos, data.CurrentPosition , r / 2, true);
                        Log.Error("方向" + dir);
                        if (dir == SnapDirection.UpLeft || dir == SnapDirection.DownLeft) dir = SnapDirection.Left;
                        if (dir == SnapDirection.UpRight || dir == SnapDirection.DownRight) dir = SnapDirection.Right;
                        var dirOffset = slot.GetDirOffset(dir);

                        //目标pos
                        var targetPos = new Vector3(puzzleView.endPos.x + dirOffset.x * r, puzzleView.endPos.y + dirOffset.y * r, 0);

                        Log.Error("当前endpos" + puzzleView.endPos);
                        Log.Error("当前targetPos" + targetPos);
                        Log.Error("--------------------");

                        //目标pos不等于当前移动目标 需要更新
                        if (targetPos != puzzleView.endPos)
                        {
                            //更新startPos
                            puzzleView.startPos = puzzleView.endPos;
                            //更新endPos
                            puzzleView.endPos = targetPos;

                            if (puzzleView.tweener == null)
                            {
                                puzzleView.tweener = DOTween.To(() => puzzleView.transform.position,
                                            pos => { puzzleView.transform.position = pos; },
                                            puzzleView.endPos,
                                            0.1f)
                                        .SetEase(Ease.Linear)
                                        .OnUpdate(() =>
                                        {
                                            puzzleView.tweener.ChangeEndValue(puzzleView.endPos, 0.1f, true).Play();
                                            if (Vector2.Distance(puzzleView.transform.position, puzzleView.endPos) <= 0.01f)
                                            {
                                                Debug.LogError("移动完成一次");
                                                EventSystem.Instance.Publish(scene, new PuzzleMoveEndEvent() { puzzle = puzzle });
                                            }
                                        });
                            }
                        }
                    }
                }
            }

            await ETTask.CompletedTask;
        }
    }
}