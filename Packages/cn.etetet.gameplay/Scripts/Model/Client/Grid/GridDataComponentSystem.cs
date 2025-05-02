namespace ET
{
    [EntitySystemOf(typeof(GridDataComponent))]
    public static partial class GridDataComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.GridDataComponent self, int x, int y, int cellSize)
        {
            // self.gridSize = new IntVector2(x, y);
            // self.cellSize = cellSize;
        }
    }
}
