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
            
            self.InitComponent(self.Config().X, self.Config().Y, 1);
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
                    var slot = self.GetComponent<SlotSpawnComponent>().GridSpawnSlot(1001 , new IntVector2(i, j));
                    self.slotDic.TryAdd(slot.InstanceId, slot);
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
                var puzzle = self.GetComponent<PuzzleSpawnComponent>().SpawnPuzzle(self.Config().PuzzleList[i] , i);
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
    }
}