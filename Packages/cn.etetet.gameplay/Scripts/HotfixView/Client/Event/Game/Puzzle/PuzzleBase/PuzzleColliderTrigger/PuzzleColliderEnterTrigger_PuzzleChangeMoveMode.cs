using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(GridView))]
    [FriendOf(typeof(SlotView))]
    public class PuzzleColliderEnterTrigger_PuzzleChangeMoveMode : AEvent<Scene , ColliderTriggerEnterEventMono>
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
                    var collider = data.collider;
                    
                    //碰撞点 gridClosePosition grid的碰撞点
                    Vector3 gridClosePosition = collider.ClosestPoint(puzzleView.transform.position);
                    //拼图触发slot
                    var puzzleSlotView = puzzleView.GetSlotViewByClosePoint(gridClosePosition);
                    //grid触发slot
                    var gridSlotView = gridView.GetSlotViewByClosePoint(gridClosePosition);
                    //gridSlot grid上slot的碰撞点 计算相对位置和对应偏移矢量
                    var gridSlotClosePosition = gridSlotView.transform.GetComponent<BoxCollider2D>().ClosestPoint(puzzleView.transform.position);
                    var r = gridSlotView.GetComponent<SpriteRenderComponent>().SpriteSize.x;
                    var direction = gridSlotView.GetSnapDirection(gridSlotClosePosition , r / 2);
                    var directionOffset = gridSlotView.GetParent<Slot>().GetDirOffset(direction);
                    
                    var targetPosition = new Vector3(gridSlotView.transform.position.x + r * directionOffset.x, gridSlotView.transform.position.y + r * directionOffset.y , 0);

                    //拼图偏移量
                    var puzzleSlotOffset = targetPosition - puzzleSlotView.transform.position;
                    //todo dotwwen位移 暂时直接位移
                    //完成吸附模式前的复位
                    puzzleView.transform.position += puzzleSlotOffset;
                    //切换为侧边吸附模式
                    puzzle.ChangeMoveMode(PuzzleMoveModeType.Adsorption);
                }
            }
            await ETTask.CompletedTask;
        }
    }    
}