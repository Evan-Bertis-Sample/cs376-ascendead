using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class GridGizmoDrawer : MonoBehaviour
{
    public int GridWidth = 10; // Number of cells in the horizontal direction
    public int GridHeight = 30; // Number of cells in the vertical direction
    public Color GridColor = Color.red;

    public float CellWidth = 30f; // Width of a single cell
    public float CellHeight = 17f; // Height of a single cell
    public Vector3 CellOffset = new Vector3(0, 0, 0); // Offset of the grid

    private void OnDrawGizmos()
    {
        DrawGrid(GridWidth, GridHeight, CellWidth, CellHeight, GridColor);
    }

    private void DrawGrid(int width, int height, float cellWidth, float cellHeight, Color color)
    {
        Gizmos.color = color;

        // Calculate the total size of the grid
        Vector3 totalSize = new Vector3(width * cellWidth, height * cellHeight, 0);

        // Draw the vertical lines
        for (int x = 0; x <= width; x++)
        {
            Vector3 startX = transform.position + new Vector3(x * cellWidth, 0, 0) + CellOffset;
            Vector3 endX = startX + new Vector3(0, totalSize.y, 0);
            Gizmos.DrawLine(startX, endX);
        }

        // Draw the horizontal lines
        for (int y = 0; y <= height; y++)
        {
            Vector3 startY = transform.position + new Vector3(0, y * cellHeight, 0) + CellOffset;
            Vector3 endY = startY + new Vector3(totalSize.x, 0, 0);
            Gizmos.DrawLine(startY, endY);
        }
    }
}
