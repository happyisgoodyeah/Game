using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CenteredGridLayout : MonoBehaviour
{
    public enum Axis { Horizontal, Vertical }
    public enum Constraint { Flexible, FixedColumnCount, FixedRowCount }
    
    [Header("起始轴方向")]
    public Axis startAxis = Axis.Horizontal;
    
    [Header("网格约束类型")]
    public Constraint constraint = Constraint.Flexible;
    
    [Header("约束数量 (行/列)")]
    [Min(1)] public int constraintCount = 1;
    
    [Header("单元格大小")]
    public Vector2 cellSize = new Vector2(1f, 1f);
    
    [Header("单元格间距")]
    public Vector2 spacing = new Vector2(0.1f, 0.1f);
    
    [Header("在编辑模式下自动更新")]
    public bool autoUpdateInEditor = true;
    
    [Header("忽略未激活的子物体")]
    public bool ignoreInactive = true;
    
    [Header("保持子物体原始旋转")]
    public bool preserveRotation = true;
    
    [Header("保持子物体原始缩放")]
    public bool preserveScale = true;
    
    [Header("显示布局Gizmos")]
    public bool showGizmos = true;

    // 缓存上次参数值
    private Axis _lastAxis;
    private Constraint _lastConstraint;
    private int _lastConstraintCount;
    private Vector2 _lastCellSize;
    private Vector2 _lastSpacing;
    private int _lastChildCount;

    private void OnValidate()
    {
        if (autoUpdateInEditor && !Application.isPlaying)
        {
            UpdateLayout();
        }
    }

    private void Update()
    {
        // 在编辑模式下检测变化并自动更新
        if (autoUpdateInEditor && !Application.isPlaying)
        {
            bool needsUpdate = false;
            
            // 检查参数是否变化
            if (_lastAxis != startAxis ||
                _lastConstraint != constraint ||
                _lastConstraintCount != constraintCount ||
                _lastCellSize != cellSize ||
                _lastSpacing != spacing)
            {
                needsUpdate = true;
            }
            
            // 检查子物体数量变化
            int childCount = GetActiveChildren().Count;
            if (_lastChildCount != childCount)
            {
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                UpdateLayout();
            }
        }
    }

    [ContextMenu("Update Layout")]
    public void UpdateLayout()
    {
        List<Transform> children = GetActiveChildren();
        
        if (children.Count == 0) return;
        
        // 更新缓存值
        _lastAxis = startAxis;
        _lastConstraint = constraint;
        _lastConstraintCount = constraintCount;
        _lastCellSize = cellSize;
        _lastSpacing = spacing;
        _lastChildCount = children.Count;

        // 计算网格尺寸
        int rows = 1;
        int columns = 1;
        
        if (constraint == Constraint.FixedColumnCount)
        {
            columns = constraintCount;
            rows = Mathf.CeilToInt(children.Count / (float)columns);
        }
        else if (constraint == Constraint.FixedRowCount)
        {
            rows = constraintCount;
            columns = Mathf.CeilToInt(children.Count / (float)rows);
        }
        else // Flexible
        {
            if (startAxis == Axis.Horizontal)
            {
                columns = Mathf.CeilToInt(Mathf.Sqrt(children.Count));
                rows = Mathf.CeilToInt(children.Count / (float)columns);
            }
            else
            {
                rows = Mathf.CeilToInt(Mathf.Sqrt(children.Count));
                columns = Mathf.CeilToInt(children.Count / (float)rows);
            }
        }

        // 计算网格总尺寸
        float totalWidth = columns * cellSize.x + Mathf.Max(0, columns - 1) * spacing.x;
        float totalHeight = rows * cellSize.y + Mathf.Max(0, rows - 1) * spacing.y;
        
        // 计算起始位置（从中心开始）
        float startX = -totalWidth * 0.5f + cellSize.x * 0.5f;
        float startY = totalHeight * 0.5f - cellSize.y * 0.5f;
        
        // 布局子物体
        for (int i = 0; i < children.Count; i++)
        {
            int row, col;
            
            // 计算网格坐标
            if (startAxis == Axis.Horizontal)
            {
                row = i / columns;
                col = i % columns;
            }
            else
            {
                row = i % rows;
                col = i / rows;
            }
            
            // 计算位置（从左上角开始计算）
            float xPos = startX + col * (cellSize.x + spacing.x);
            float yPos = startY - row * (cellSize.y + spacing.y);
            
            // 应用位置
            children[i].localPosition = new Vector3(xPos, yPos, 0);
            
            // 保持原始旋转
            if (preserveRotation)
            {
                children[i].localRotation = Quaternion.identity;
            }
            
            // 保持原始缩放
            if (preserveScale)
            {
                children[i].localScale = Vector3.one;
            }
        }
    }

    private List<Transform> GetActiveChildren()
    {
        List<Transform> children = new List<Transform>();
        
        foreach (Transform child in transform)
        {
            if (!ignoreInactive || child.gameObject.activeSelf)
            {
                children.Add(child);
            }
        }
        
        return children;
    }

    // 在编辑器中绘制Gizmos
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || !autoUpdateInEditor) return;
        
        List<Transform> children = GetActiveChildren();
        if (children.Count == 0) return;

        // 计算网格尺寸
        int rows = 1;
        int columns = 1;
        
        if (constraint == Constraint.FixedColumnCount)
        {
            columns = constraintCount;
            rows = Mathf.CeilToInt(children.Count / (float)columns);
        }
        else if (constraint == Constraint.FixedRowCount)
        {
            rows = constraintCount;
            columns = Mathf.CeilToInt(children.Count / (float)rows);
        }
        else // Flexible
        {
            if (startAxis == Axis.Horizontal)
            {
                columns = Mathf.CeilToInt(Mathf.Sqrt(children.Count));
                rows = Mathf.CeilToInt(children.Count / (float)columns);
            }
            else
            {
                rows = Mathf.CeilToInt(Mathf.Sqrt(children.Count));
                columns = Mathf.CeilToInt(children.Count / (float)rows);
            }
        }

        // 计算网格总尺寸
        float totalWidth = columns * cellSize.x + Mathf.Max(0, columns - 1) * spacing.x;
        float totalHeight = rows * cellSize.y + Mathf.Max(0, rows - 1) * spacing.y;
        
        // 计算起始位置（与子物体位置计算保持一致）
        float startX = -totalWidth * 0.5f + cellSize.x * 0.5f;
        float startY = totalHeight * 0.5f - cellSize.y * 0.5f;
        
        // 绘制网格边框
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.7f);
        Vector3 size = new Vector3(totalWidth, totalHeight, 0.01f);
        Vector3 center = transform.position;
        Gizmos.DrawWireCube(center, size);
        
        // 绘制中心点
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.1f);
        Gizmos.DrawLine(transform.position - transform.right * 0.15f, transform.position + transform.right * 0.15f);
        Gizmos.DrawLine(transform.position - transform.up * 0.15f, transform.position + transform.up * 0.15f);
        
        // 绘制单元格
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // 计算单元格位置（与子物体位置计算一致）
                float xPos = startX + col * (cellSize.x + spacing.x);
                float yPos = startY - row * (cellSize.y + spacing.y);
                
                Vector3 cellCenter = transform.position + new Vector3(xPos, yPos, 0);
                Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize.x, cellSize.y, 0.01f));
            }
        }
    }
}