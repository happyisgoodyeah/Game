namespace ET
{
    [EntitySystemOf(typeof(Grid))]
    public static partial class GridSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Grid self, int x, int y, int cellSize)
        {
            self.InitComponent(x,y,cellSize);
            self.SpawnSlot();
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
                    self.GetComponent<SlotSpawnComponent>().SpawnSlot(new IntVector2(i, j));
                }
            }
        }

        /// <summary>
        /// 生成puzzle
        /// </summary>
        /// <param name="self"></param>
        public static void SpawnPuzzle(this Grid self)
        {
            self.GetComponent<PuzzleSpawnComponent>().SpawnPuzzle(new IntVector2(0,0) , 1);
            self.GetComponent<PuzzleSpawnComponent>().SpawnPuzzle(new IntVector2(0,0) , 1);
            self.GetComponent<PuzzleSpawnComponent>().SpawnPuzzle(new IntVector2(0,0) , 1);
        }
    }
}
