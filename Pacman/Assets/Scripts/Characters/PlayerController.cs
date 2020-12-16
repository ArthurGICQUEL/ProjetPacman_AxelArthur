using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveTime = 0.2f;
    private Vector2Int inputDirection, nextCoord;
    private float timer = 0;
    int xDirection = 0;
    int yDirection = 0;
    private GameManager gm;
    int rotation = 0;
    float speed = 1f;
    Vector2Int selfCoord
    {
        get
        {
            return gm.PosToCoord(transform.position);
        }
    }

    private void Start()
    {
        gm = GameManager.Instance;
        speed = gm.clockRate; // so the movements of from tiles to tiles are syncronized with the ClockUpdate();
        nextCoord = selfCoord;
    }
    void Update()
    {
        Move();
        Debug.Log(inputDirection);
    }

    /// <summary>
    /// ClockUpdate is called every 1/clockRate seconds 
    /// </summary>
    public void ClockUpdate()
    {
        if (gm.HasNode(selfCoord + inputDirection)) nextCoord = selfCoord + inputDirection;
        Rotate();
    }

    /// <summary>
    /// Calls InputManager(); and MoveLerp
    /// </summary>
    private void Move()
    {
        InputManager(); // Sets the inputDirection variable

        MoveLerp(nextCoord);
    }

    /// <summary>
    /// Sets the inputDirection variable
    /// </summary>
    private void InputManager()
    {
        //I am rounding xDirection and yDirection to 1, 0 or -1 because my cells size is 1 by 1.
        int inputHori = Mathf.RoundToInt(Input.GetAxis("Horizontal"));
        int inputVert = Mathf.RoundToInt(Input.GetAxis("Vertical"));

        if (inputHori == 0 || inputVert == 0)
        {
            xDirection = inputHori;
            yDirection = inputVert;
        }//So the player can't go in diagonal

        Vector2Int input = new Vector2Int(xDirection, yDirection);

        if (input != Vector2Int.zero && gm.HasNode(nextCoord + input))//if I put "gm.HasNode(selfCoord + input)", there is a frame where I can turn on a wall
        {
            inputDirection = input;
        }//So the player keeps its direction even if the key is released or if you press a key leading to a wall
    }

    /// <summary>
    /// Moves the player to a target
    /// </summary>
    /// <param name="lerpTargetCoord">target coordinates</param>
    private void MoveLerp(Vector2Int lerpTargetCoord)
    {
        Vector3 lerpTarget = gm.CoordToPos(lerpTargetCoord);
        transform.position = Vector3.Lerp(transform.position, lerpTarget, Time.deltaTime * speed / Vector3.Distance(transform.position, lerpTarget));
    }

    /// <summary>
    /// Rotates the player so he is facing the tile where he wants to go
    /// </summary>
    private void Rotate()
    {
        if (inputDirection == new Vector2Int(-1, 0)) rotation = 0;
        else if (inputDirection == new Vector2Int(1, 0)) rotation = 180;
        else if (inputDirection == new Vector2Int(0, -1)) rotation = 90;
        else if (inputDirection == new Vector2Int(0, 1)) rotation = 270;
        transform.eulerAngles = new Vector3Int(0, 0, rotation);
    }
}
