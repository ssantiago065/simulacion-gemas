using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Vector2Int currentGridPosition; // Posición actual en la matriz
    public Renderer targetRenderer; // Renderer del área de juego
    public int columns = 8; // Número de columnas en la matriz
    public int rows = 8; // Número de filas en la matriz

    void Start()
    {
        if (targetRenderer == null)
        {
            targetRenderer = RendererHelper.GetTargetRenderer();
            if (targetRenderer == null)
            {
                Debug.LogError("No se pudo asignar el targetRenderer en MovementController.");
            }
        }

        // Obtener la posición inicial del robot en la matriz
        currentGridPosition = GridManager.WorldToGridPosition(transform.position, targetRenderer.bounds, columns, rows);
        GridManager.SetObjectAt(currentGridPosition, gameObject);
    }

    void Update()
    {
        // Detectar entrada del teclado
        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector2Int.right;

        if (direction != Vector2Int.zero)
        {
            Move(direction);
        }
    }

    private void Move(Vector2Int direction)
    {
        Vector2Int newGridPosition = currentGridPosition + direction;

        // Verificar si la nueva posición está dentro de los límites de la matriz
        if (newGridPosition.x >= 0 && newGridPosition.x < columns &&
            newGridPosition.y >= 0 && newGridPosition.y < rows &&
            GridManager.GetObjectAt(newGridPosition) == null) // Verificar si la celda está vacía
        {
            // Actualizar la matriz
            GridManager.ClearObjectAt(currentGridPosition);
            GridManager.SetObjectAt(newGridPosition, gameObject);

            // Actualizar la posición en el mundo
            Vector3 newWorldPosition = GridToWorldPosition(newGridPosition);
            transform.position = newWorldPosition;

            // Actualizar la posición actual
            currentGridPosition = newGridPosition;

            // Imprimir la matriz en la consola
            PrintGrid();
        }
    }

    private Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        Bounds b = targetRenderer.bounds;
        float cellWidth = b.size.x / columns;
        float cellHeight = b.size.z / rows;

        return new Vector3(
            b.min.x + cellWidth * (gridPosition.x + 0.5f),
            transform.position.y,
            b.min.z + cellHeight * (gridPosition.y + 0.5f)
        );
    }

    private void PrintGrid()
    {
        GameObject[,] grid = GridManager.GetGrid();
        string gridText = "";

        for (int y = rows - 1; y >= 0; y--) // Imprimir desde la fila superior
        {
            for (int x = 0; x < columns; x++)
            {
                if (grid[x, y] == null)
                {
                    gridText += ". "; // Celda vacía
                }
                else if (grid[x, y].CompareTag("RobotAzul"))
                {
                    gridText += "A "; // Robot azul
                }
                else if (grid[x, y].CompareTag("RobotRojo"))
                {
                    gridText += "R "; // Robot rojo
                }
                else if (grid[x, y].CompareTag("RobotVerde"))
                {
                    gridText += "V "; // Robot verde
                }
                else if (grid[x, y].CompareTag("GemaAzul"))
                {
                    gridText += "GA "; // Gema azul
                }
                else if (grid[x, y].CompareTag("GemaRoja"))
                {
                    gridText += "GR "; // Gema roja
                }
                else if (grid[x, y].CompareTag("GemaVerde"))
                {
                    gridText += "GV "; // Gema verde
                }
                else
                {
                    gridText += "? "; // Objeto desconocido
                }
            }
            gridText += "\n";
        }

        Debug.Log(gridText);
    }
}