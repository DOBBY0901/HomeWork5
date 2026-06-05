using UnityEngine;

public class BoardGridRenderer : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Vector3Int boardOrigin = new Vector3Int(-4, -6, 0);
    [SerializeField] private int width = 11;
    [SerializeField] private int height = 11;

    [Header("Line Settings")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = Color.black;
    [SerializeField] private float lineWidth = 0.03f;
    [SerializeField] private int sortingOrder = 20;

    private void Start()
    {
        DrawGrid();
    }

    [ContextMenu("격자 다시 그리기")]
    private void DrawGrid()
    {
        ClearOldLines();

        // 세로줄 12개
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = new Vector3(boardOrigin.x + x, boardOrigin.y, 0);
            Vector3 end = new Vector3(boardOrigin.x + x, boardOrigin.y + height, 0);

            CreateLine($"Vertical_{x}", start, end);
        }

        // 가로줄 12개
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = new Vector3(boardOrigin.x, boardOrigin.y + y, 0);
            Vector3 end = new Vector3(boardOrigin.x + width, boardOrigin.y + y, 0);

            CreateLine($"Horizontal_{y}", start, end);
        }
    }

    private void CreateLine(string lineName, Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject(lineName);
        lineObj.transform.SetParent(transform);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.startColor = lineColor;
        line.endColor = lineColor;

        line.useWorldSpace = true;

        if (lineMaterial != null)
            line.material = lineMaterial;

        line.sortingOrder = sortingOrder;
    }

    private void ClearOldLines()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}