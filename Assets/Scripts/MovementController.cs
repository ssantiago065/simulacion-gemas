using UnityEngine;

public class MovementController : MonoBehaviour
{
    public Movement movement;   // referencia al script Movement

    
    private int currentCol = 0;
    private int currentRow = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentRow += 1;
            movement.MoveToCell(currentCol, currentRow);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentRow -= 1;
            movement.MoveToCell(currentCol, currentRow);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentCol -= 1;
            movement.MoveToCell(currentCol, currentRow);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentCol += 1;
            movement.MoveToCell(currentCol, currentRow);
        }
    }
}