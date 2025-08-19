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

            self.InitComponent(self.Config().X, self.Config().Y, 2);
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
        public static void SpawnSlot(this Grid self)
        {
            for (int i = 0; i < self.gridSize.X; i++)
            {
                for (int j = 0; j < self.gridSize.Y; j++)
                {
                    var slot = self.GetComponent<SlotSpawnComponent>().GridSpawnSlot(1001, new IntVector2(i, j));
                    self.slotDic.TryAdd(new IntVector2(i, j), slot);
                }
            }
        }

        /// <summary>
        /// 生成puzzle
        /// </summary>
        /// <param name="self"></param>
        public static void SpawnPuzzle(this Grid self)
        {
            for (int i = 0; i < self.Config().PuzzleCount; i++)
            {
                var puzzle = self.GetComponent<PuzzleSpawnComponent>().SpawnPuzzle(self.Config().PuzzleList[i], i);
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
                if (slot.puzzleRef.Entity != null)
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
            float halfWidth = (self.gridSize.Y * self.cellSize) / 2f;
            float halfHeight = (self.gridSize.X * self.cellSize) / 2f;

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

            return new IntVector2((int)Math.Round((totalHeight / 2 - worldPos.Y - self.cellSize / 2f) / self.cellSize), // 行索引
                (int)Math.Round((worldPos.X + totalWidth / 2 - self.cellSize / 2f) / self.cellSize) // 列索引
            );
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
                //todo 网格边界比一定是矩形判断逻辑需要修改
                // 检查是否超出网格边界
                if (pos.X < 0 || pos.X >= self.gridSize.X || pos.Y < 0 || pos.Y >= self.gridSize.Y)
                    return false;

                Slot slot = self.GetSlot(pos);
                if (slot == null || slot.puzzleRef.Entity != null)
                    return false;
            }

            return true;
        }
    }
}