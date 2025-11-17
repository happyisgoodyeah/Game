using System;
using System.Collections.Generic;
using System.Drawing;

namespace ET
{
    [EntitySystemOf(typeof(Grid))]
    [FriendOf(typeof(Slot))]
    public static partial class GridSystem
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static GridConfig Config(this Grid self)
        {
            return GridConfigCategory.Instance.Get(self.configId);
        }

        [EntitySystem]
        private static void Awake(this ET.Grid self, int configId)
        {
            self.configId = configId;

            self.InitComponent(self.Config().X, self.Config().Y, 3);
        }

        public static void InitComponent(this ET.Grid self, int x, int y, int cellSize)
        {
            //数据
            self.gridSize = new IntVector2(x, y);
            self.cellSize = cellSize;

            //添加组件
            self.AddComponent<SlotSpawnComponent>();
            self.AddComponent<PuzzleSpawnComponent>();
        }

        /// <summary>
        /// 生成slot
        /// </summary>
        /// <param name="self"></param>
        public static async ETTask SpawnSlot(this Grid self)
        {
            var config = self.Config();
            for (int j = 0; j < self.gridSize.Y; j++)
            {
                for (int i = 0; i < self.gridSize.X; i++)
                {
                    var index = i + j * self.gridSize.Y;
                    var slotConfig = config.SlotList[index];
                    var slot = self.GetComponent<SlotSpawnComponent>().GridSpawnSlot(config.SlotList[index] , new IntVector2(i, j));
                    await self.Root().GetComponent<TimerComponent>().WaitFrameAsync();
                    self.slotDic.TryAdd(new IntVector2(i, j), slot);
                    //最外围一圈判定可吸附
                    if (i == 0 || j == 0 || i == self.gridSize.X - 1 || j == self.gridSize.Y - 1)
                    {
                        self.adsorptionSlots.Add(slot);
                    }

                    if ((i == 0 || i == self.gridSize.X - 1) && (j == 0 || j == self.gridSize.Y - 1))
                    {
                        self.sideSlots.Add(slot);
                    }
                }
            }
        }

        /// <summary>
        /// 生成puzzle
        /// </summary>
        /// <param name="self"></param>
        public static void SpawnPuzzle(this Grid self)
        {
            var config = self.Config();
            for (int i = 0; i < config.PuzzleCount; i++)
            {
                var puzzleConfig = config.PuzzleList[i];
                var puzzle = self.GetComponent<PuzzleSpawnComponent>().SpawnPuzzle(puzzleConfig.Id , new FloatVector2(puzzleConfig.Trans.X , puzzleConfig.Trans.Y));
                self.PuzzleDic.TryAdd(puzzle.InstanceId, puzzle);
            }
        }

        /// <summary>
        /// 获得Grid下Slot的数量
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetSlotCount(this Grid self)
        {
            return self.slotDic.Count;
        }

        /// <summary>
        /// 获得Grid下Puzzle的数量
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetPuzzleCount(this Grid self)
        {
            return self.PuzzleDic.Count;
        }

        /// <summary>
        /// 检测所有slot是否已经有绑定的puzzle
        /// </summary>
        /// <returns></returns>
        public static bool CheckGameOver(this Grid self)
        {
            foreach (var slotRef in self.slotDic.Values)
            {
                var slot = slotRef.Entity;
                if (slot.puzzleRef.Entity == null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检测位置是否在网格范围内
        /// </summary>
        /// <param name="self"></param>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public static bool ContainsPosition(this Grid self, FloatVector2 worldPos)
        {
            float halfWidth = (self.gridSize.X * self.cellSize) / 2f;
            float halfHeight = (self.gridSize.Y * self.cellSize) / 2f;

            return worldPos.X >= -halfWidth &&
                    worldPos.X <= halfWidth &&
                    worldPos.Y >= -halfHeight &&
                    worldPos.Y <= halfHeight;
        }

        /// <summary>
        /// 网格坐标转世界坐标（中心点对齐）
        /// </summary>
        /// <param name="self"></param>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        public static FloatVector2 GridToWorldPosition(this Grid self, IntVector2 gridPos)
        {
            // 计算总尺寸
            float totalWidth = self.gridSize.X * self.cellSize;
            float totalHeight = self.gridSize.Y * self.cellSize;

            // 计算世界坐标
            return new FloatVector2(totalHeight / 2 - gridPos.X * self.cellSize - self.cellSize / 2f, // 行方向：从上到下
                -totalWidth / 2 + gridPos.Y * self.cellSize + self.cellSize / 2f // 列方向：从左到右
            );
        }

        /// <summary>
        /// 世界坐标转网格坐标（中心点对齐）
        /// </summary>
        /// <param name="self"></param>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public static IntVector2 WorldToGridPosition(this Grid self, FloatVector2 worldPos)
        {
            // 计算总尺寸
            float totalWidth = self.gridSize.X * self.cellSize;
            float totalHeight = self.gridSize.Y * self.cellSize;

            return new IntVector2((int)Math.Round((worldPos.X + totalWidth / 2 - self.cellSize / 2f) / self.cellSize),
                (int)Math.Round((totalHeight / 2 - worldPos.Y - self.cellSize / 2f) / self.cellSize));
        }

        /// <summary>
        /// 获取指定位置的格子
        /// </summary>
        /// <param name="self"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Slot GetSlot(this Grid self, IntVector2 pos)
        {
            // 检查边界
            if (pos.X < 0 || pos.X >= self.gridSize.X || pos.Y < 0 || pos.Y >= self.gridSize.Y)
                return null;

            return self.slotDic.TryGetValue(pos, out var slotRef) ? slotRef : null;
        }

        /// <summary>
        /// 获取拼图覆盖的slot坐标list
        /// </summary>
        /// <param name="self"></param>
        /// <param name="puzzle"></param>
        /// <param name="originPosition"></param>
        public static List<IntVector2> GetCoveredPositions(this Grid self, Puzzle puzzle, IntVector2 originPosition)
        {
            return puzzle.GetCoveredPositions(originPosition);
        }

        /// <summary>
        /// 检查是否可以放置拼图
        /// </summary>
        /// <param name="self"></param>
        /// <param name="puzzle"></param>
        /// <param name="originPosition"></param>
        /// <returns></returns>
        public static bool CanPlacePuzzle(this Grid self, Puzzle puzzle, IntVector2 originPosition)
        {
            //获取puzzle的偏移量数组
            var coveredPositions = self.GetCoveredPositions(puzzle, originPosition);

            foreach (var pos in coveredPositions)
            {
                // 检查是否超出网格边界
                if (pos.X < 0 || pos.X >= self.gridSize.X || pos.Y < 0 || pos.Y >= self.gridSize.Y)
                {
                    return false;
                }

                Slot slot = self.GetSlot(pos);
                
                //检测绑定的puzzle不为当前判定的puzzle
                if (slot == null || (slot.puzzleRef.Entity != null && slot.puzzleRef.Entity != puzzle))
                {
                    return false;    
                }
                
                //检测slot是否可以绑定
                if (!slot.GetComponent<SlotStateComponent>().GetCanPlace())
                {
                    return false;
                }
            }

            return true;
        }
    }
}