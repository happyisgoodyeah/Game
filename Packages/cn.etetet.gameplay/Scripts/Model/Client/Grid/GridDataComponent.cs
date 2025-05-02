using System.Numerics;
namespace ET
{
    [ComponentOf(typeof(Grid))]
    public partial class GridDataComponent : Entity , IAwake<int,int,int>
    {
        // public IntVector2 gridSize;
        // public int cellSize;
    }
}
