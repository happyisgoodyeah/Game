using System.Collections.Generic;
using DG.Tweening;
using ET.Client;
using ET.Server;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(GridView))]
    [FriendOf(typeof(SlotView))]
    [FriendOf(typeof(PuzzleView))]
    [FriendOfAttribute(typeof(ET.Grid))]
    [FriendOfAttribute(typeof(ET.Slot))]
    public class PuzzleColliderEnterTrigger_PuzzleChangeMoveMode : AEvent<Scene, ColliderTriggerEnterEventMono>
    {
        protected override async ETTask Run(Scene scene, ColliderTriggerEnterEventMono data)
        {
            var bindEntity = data.bindEntity;
            var triggerEntity = data.triggerEntity;
            if (bindEntity is PuzzleView puzzleView && triggerEntity is GridView gridView)
            {
                Puzzle puzzle = puzzleView.GetParent<Puzzle>();
                Grid grid = gridView.GetParent<Grid>();

                //改为吸附模式 先对齐对应的slot
                if (puzzle.moveMode == PuzzleMoveModeType.Normal)
                {
                    puzzleView.tweener?.Kill();

                    var collider = data.collider;

                    //碰撞点 gridClosePosition grid的碰撞点
                    Vector3 gridClosePosition = collider.ClosestPoint(puzzleView.transform.position);
                    //拼图触发slot
                    var puzzleSlotView = puzzleView.GetSlotViewByClosePoint(gridClosePosition);
                    //grid触发slot
                    var gridSlotView = gridView.GetSlotViewByClosePoint(gridClosePosition);
                    var gridSlot = gridSlotView.GetParent<Slot>();
                    
                    //gridSlot半径
                    var r = gridSlotView.GetComponent<SpriteRenderComponent>().SpriteSize.x;
                    
                    //gridSlot PuzzleView的碰撞点
                    var pointPosition = puzzleView.transform.GetComponent<PolygonCollider2D>().ClosestPoint(gridSlotView.transform.position);

                    //计算方向
                    var allowDir = Utility.GetAllowDirection(gridSlot.position.X, gridSlot.position.Y, grid.gridSize.X, grid.gridSize.Y);
                    var direction = Utility.GetSnapDirection(gridSlotView.transform.position, pointPosition, !grid.sideSlots.Contains(gridSlot), new List<SnapDirection>(){allowDir});
                    var directionOffset = gridSlotView.GetParent<Slot>().GetDirOffset(direction);

                    //目标位置
                    var targetPosition = new Vector3(gridSlotView.transform.position.x + r * directionOffset.x, gridSlotView.transform.position.y + r * directionOffset.y, 0);

                    //拼图偏移量
                    var puzzleSlotOffset = targetPosition - puzzleSlotView.transform.position;
                    
                    //todo dotwwen位移 暂时直接位移
                    //完成吸附模式前的复位
                    puzzleView.transform.position += puzzleSlotOffset;
                    puzzleView.endPos = puzzleView.transform.position;
                    
                    //切换为吸附模式
                    puzzle.ChangeMoveMode(PuzzleMoveModeType.Adsorption);
                }
            }
            await ETTask.CompletedTask;
        }
    }
}