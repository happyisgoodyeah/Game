using System.Collections.Generic;
using DG.Tweening;
using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    [FriendOf(typeof(Slot))]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(PuzzleView))]
    public class Click_PuzzleClick : AEvent<Scene, ClickRotateEvent>
    {
        protected override async ETTask Run(Scene scene, ClickRotateEvent data)
        {
            //拖拽的时候进行旋转操作
            if (data.Entity is PuzzleView puzzleView)
            {
                Puzzle puzzle = puzzleView.GetParent<Puzzle>();
                // puzzleView.Rotate(data.Angle);

                if (puzzle.bindSlots.Count == 0)
                {
                    puzzle.RotatePuzzle(90);

                    EventSystem.Instance.Publish(scene,
                        new ClickPuzzleRotateEvent() { puzzle = puzzleView.GetParent<Puzzle>(), angle = data.Angle });
                }
                else
                {
                    //puzzle已经在Grid上执行旋转得条件判断

                    Grid grid = puzzle.GetParent<Grid>();
                    GridView gridView = grid.GetComponent<GridView>();

                    puzzleView.tweener?.Kill();
                    puzzleView.tweener = null;
                    //点击旋转一定是位于Grid中，所以不判断是否在Grid之中
                    //先执行旋转逻辑，判断是否合法，不合法再转回去
                    puzzle.RotatePuzzleData(90);

                    //拖拽结束的位置
                    var worldPosition = puzzleView.transform.position;
                    FloatVector2 position = new FloatVector2(worldPosition.x, worldPosition.y);

                    IntVector2 originPosition =
                            grid.WorldToGridPosition(position);
                    //能够放置拼图
                    if (grid.CanPlacePuzzle(puzzle, originPosition))
                    {
                        puzzle.ResetBindSlots();

                        List<IntVector2> positionList = grid.GetCoveredPositions(puzzle, originPosition);
                        foreach (IntVector2 slotPosition in positionList)
                        {
                            Slot slot = grid.GetSlot(slotPosition);
                            slot.SetPuzzle(puzzle);
                            puzzle.bindSlots.Add(slot);
                        }

                        puzzleView.transform.position = grid.GetSlot(originPosition).GetComponent<SlotView>().transform.position;
                        puzzle.RotatePuzzleView();
                    }
                    else
                    {
                        //不能放置拼图，执行shake puzzle效果
                        //先将数据层转回去
                        puzzle.RotatePuzzleData(-90);
                        Log.Info("rotate illegality");

                        puzzle.ShakePuzzleView();
                    }
                }
            }

            await ETTask.CompletedTask;
        }
    }
}