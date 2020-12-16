using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    // Tilemaps
    [SerializeField] private Tilemap tilemapWalkable, tilemapEnergizer, tilemapPlayerSpawn;
    [SerializeField] private Tile dotTile, energizerTile, nothingTile;

    // Characters
    [SerializeField] private PlayerController playerPrefab;
    private PlayerController playerInstance;
    [SerializeField] private EnemyController[] enemyPrefabs;
    private List<EnemyController> enemyInstances;

    // Dots
    private int dotsCount = 0;
    private int eatenDots = 0;

    // Graphs
    private Graph mazeGraph;

    // Core
    private float clock = 0f;
    public int clockRate = 4; // capped at the refresh rate of the game
    private int enemyExitManagerState = 1;

    // Teleportation
    private List<Node> tpList = new List<Node>();
    private Vector3[] tpTargets = new Vector3[5];

    // Nodes
    private Node playerNode;
    private Dictionary<EnemyController, Node> enemyList;

    // InSpawn enemies behavior
    public Vector3 enemySpawnOne, enemySpawnTwo, spawnExit;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayerPrefs.SetInt("lastLevel", SceneManager.GetActiveScene().buildIndex); // Saves the current scene in the PlayerPrefs so you can reload it in the GameOver scene if you press the play again button

        mazeGraph = new Graph(tilemapWalkable);
        InstantiateNodes();
        InstantiateEnemies();
    }


    void Update()
    {
        if (clock >= 1)
        {
            ClockUpdate();
        }
        clock += Time.deltaTime * clockRate;
    }

    /// <summary>
    /// ClockUpdate is called every 1/clockRate seconds.
    /// In the GameManager script, calls the ClockUpdate(); of the other scripts so they are all syncronized
    /// </summary>
    void ClockUpdate()
    {
        playerInstance.ClockUpdate(); // calls the PlayerController script's ClockUpdate();

        Dictionary<EnemyController, Node> enemyListLast = enemyList; // saves the enemyList Dictionary from the previous ClockUpdate(); call in the enemyListLast Dictionary
        enemyList = new Dictionary<EnemyController, Node>();
        foreach (EnemyController enemy in enemyInstances) // calls the EnemyController script's ClockUpdate(); of each enemy in the scene and add that enemy in the enemyList Dictionary
        {
            enemy.ClockUpdate();
            if (HasNode(enemy.transform.position)) enemyList.Add(enemy, GetNode(enemy.transform.position));
        }

        Node playerLastNode = playerNode; // savec the playerNode Node from the previous ClockUpdate(); call in the playerLastNode Node
        playerNode = GetNode(playerInstance.transform.position);

        foreach (Node tp in tpList) // checks if there is an enemy or the player on any tp and teleports it if it's the case
        {
            foreach (EnemyController enemy in enemyList.Keys)
            {
                if (enemyList[enemy] == tp)
                {
                    Teleport(enemy.transform, tp);
                    enemy.ClockUpdate(); // The ClockUpdate needs to be called again in case of tp
                }
            }
            if (playerNode == tp)
            {
                Teleport(playerInstance.transform, tp);
                playerInstance.ClockUpdate(); // The ClockUpdate needs to be called again in case of tp
            }
        }

        foreach (EnemyController enemy in enemyList.Keys) // Loads the game over scene if the player's Node is equal to the enemy's Node or if their last Nodes are equals (in the case where they cross themselves in between two tiles)
        {
            if
            (
                playerNode == enemyList[enemy]
                || (enemyListLast.ContainsKey(enemy) ? // I check if the enemy got a last Node before trying to access it because when the enemies just left their spawn they don't have a last Node yet
                    (playerNode == enemyListLast[enemy] && playerLastNode == enemyList[enemy])
                    : false)
            )
            {
                SceneManager.LoadScene(4);
            }
        }

        PlayerInteraction(playerNode);

        EnemyExitManager();

        clock -= clock;
    }
    /// <summary>
    /// Sets the "has" of each node in the mazeGraph dictionary and spawns the player
    /// </summary>
    private void InstantiateNodes()
    {
        foreach (Vector2Int coord in mazeGraph.mazeDic.Keys)
        {

            Vector3 spawnPos = CoordToPos(coord);

            if (tilemapPlayerSpawn.HasTile((Vector3Int)coord))
            {
                playerInstance = Instantiate(playerPrefab, spawnPos, transform.rotation);
            }
            else if (tilemapEnergizer.HasTile((Vector3Int)coord))
            {
                mazeGraph.mazeDic[coord].has = Node.tile.energizer;
                dotsCount += 1;
            }
            else if (tilemapWalkable.cellBounds.xMin == coord.x)
            {
                mazeGraph.mazeDic[coord].has = Node.tile.tpOne;
                tpTargets[1] = CoordToPos(coord);
                tpList.Add(mazeGraph.mazeDic[coord]);
            }
            else if (tilemapWalkable.cellBounds.xMax == coord.x + 1)
            {
                mazeGraph.mazeDic[coord].has = Node.tile.tpTwo;
                tpTargets[2] = CoordToPos(coord);
                tpList.Add(mazeGraph.mazeDic[coord]);
            }
            else if (tilemapWalkable.cellBounds.yMin == coord.y)
            {
                mazeGraph.mazeDic[coord].has = Node.tile.tpThree;
                tpTargets[3] = CoordToPos(coord);
                tpList.Add(mazeGraph.mazeDic[coord]);
            }
            else if (tilemapWalkable.cellBounds.yMax == coord.y + 1)
            {
                mazeGraph.mazeDic[coord].has = Node.tile.tpFour;
                tpTargets[4] = CoordToPos(coord);
                tpList.Add(mazeGraph.mazeDic[coord]);
            }
            else
            {
                mazeGraph.mazeDic[coord].has = Node.tile.dot;
                dotsCount += 1;
            }
        }
        InstantiateTiles();
    }

    /// <summary>
    /// Sets the tiles at the coordinates of each node with a "has" equal to "dot" to a dot tile 
    /// and sets the tiles at the coordinates of each node with a "has" equal to "energizer" to an energizer tile
    /// </summary>
    private void InstantiateTiles()
    {
        foreach (Vector2Int coord in mazeGraph.mazeDic.Keys)
        {
            if (mazeGraph.mazeDic[coord].has == Node.tile.dot)
            {
                tilemapWalkable.SetTile((Vector3Int)coord, dotTile);
            }
            else if (mazeGraph.mazeDic[coord].has == Node.tile.energizer)
            {
                tilemapWalkable.SetTile((Vector3Int)coord, energizerTile);
            }
        }
    }

    /// <summary>
    /// Instantiate all the enemies in the enemyPrefabs list between 2 position at regular intervals
    /// And adds their instances in the enemyInstance list
    /// </summary>
    private void InstantiateEnemies()
    {
        enemyInstances = new List<EnemyController>();
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            Vector3 spawn = Vector3.Lerp(enemySpawnOne, enemySpawnTwo, i / (float)(enemyPrefabs.Length - 1));
            EnemyController enemy = Instantiate(enemyPrefabs[i], spawn, transform.rotation);
            enemy.spawnExit = spawnExit; //The enemy prefabs stay the same from a scene to an other but not the GameManager. By setting enemy.spawnExist in the GameManager I can have different levels with different spawn shapes and locations
            enemyInstances.Add(enemy);
        }
    }

    /// <summary>
    /// Calls HasNode(); in the Graph script
    /// </summary>
    /// <param name="coord">Coordinates of a tile</param>
    /// <returns>true if there is a node at "coord" in "mazeDic" else false</returns>
    public bool HasNode(Vector2Int coord)
    {
        return mazeGraph.HasNode(coord);
    }

    /// <summary>
    /// Calls HasNode(); in the Graph script
    /// </summary>
    /// <param name="worldPos">Position of a tile</param>
    /// <returns>true if there is a node at "worldPos" in "mazeDic" else false</returns>
    public bool HasNode(Vector3 worldPos)
    {
        return mazeGraph.HasNode(PosToCoord(worldPos));
    }

    /// <summary>
    /// Calls GetNode(); in the Graph script
    /// </summary>
    /// <param name="coord">Coordinates of a tile</param>
    /// <returns>the node representing the cell with "coord" coordinates</returns>
    public Node GetNode(Vector2Int coord)
    {
        return mazeGraph.GetNode(coord);
    }

    /// <summary>
    /// Calls GetNode(); in the Graph script
    /// </summary>
    /// <param name="worldPos">World position of a tile</param>
    /// <returns>the node representing the cell at "worldPos" position</returns>
    public Node GetNode(Vector3 worldPos)
    {
        return mazeGraph.GetNode(PosToCoord(worldPos));
    }

    /// <param name="position">World position of a tile</param>
    /// <returns>Coordinates of a tile</returns>
    public Vector2Int PosToCoord(Vector3 position)
    {
        Vector3Int coordV3 = tilemapWalkable.WorldToCell(position);
        return (Vector2Int)coordV3;
    }
    /// <param name="coord">coordinates of a tile</param>
    /// <returns>World position of a tile</returns>
    public Vector3 CoordToPos(Vector2Int coord)
    {
        return tilemapWalkable.GetCellCenterWorld((Vector3Int)coord);
    }

    /// <summary>
    /// Teleports an enemy or the player to the other side of a teleporter
    /// </summary>
    /// <param name="instance">the Transform of something you want to teleport</param>
    /// <param name="tp">a teleporter's Node</param>
    private void Teleport(Transform instance, Node tp)
    {
        switch (tp.has)
        {
            case Node.tile.tpOne:
                instance.position = tpTargets[2];
                break;
            case Node.tile.tpTwo:
                instance.position = tpTargets[1];
                break;
            case Node.tile.tpThree:
                instance.position = tpTargets[4];
                break;
            case Node.tile.tpFour:
                instance.position = tpTargets[3];
                break;
        }
    }

    /// <summary>
    /// When the player arrives on a node, checks if there is a dot or an energizer on it
    /// If that is the case calls EatDot(node);
    /// </summary>
    /// <param name="node">The player's current Node</param>
    private void PlayerInteraction(Node node)
    {
        switch (node.has)
        {
            case Node.tile.dot:
                EatDot(node);
                break;
            case Node.tile.energizer:
                EatDot(node);
                break;
        }
    }

    /// <summary>
    /// changes the "has" of a Node to "nothing"
    /// changes the tile at the Node coordinates by an empty ground tile
    /// increments the "eatenDots" by 1
    /// if all dots an energizers are eaten, launch the victory sequence
    /// </summary>
    /// <param name="node">a node :)</param>
    private void EatDot(Node node)
    {
        node.has = Node.tile.nothing;
        tilemapWalkable.SetTile((Vector3Int)node.coord, nothingTile);
        eatenDots += 1;
        if (eatenDots == dotsCount) SceneManager.LoadScene(4);
    }

    /// <summary>
    /// Makes the enemies leave their spawn one by one according to the percentage of eaten dots
    /// </summary>
    private void EnemyExitManager()
    {
        float eatenRatio = eatenDots / (float)dotsCount;
        switch (enemyExitManagerState)
        {
            case 1:
                enemyInstances[0].state++;
                enemyExitManagerState++;
                break;
            case 2:
                if (eatenRatio >= 0.2)
                {
                    enemyInstances[1].state++;
                    enemyExitManagerState++;
                }
                break;
            case 3:
                if (eatenRatio >= 0.4)
                {
                    enemyInstances[2].state++;
                    enemyExitManagerState++;
                }
                break;
            case 4:
                if (eatenRatio >= 0.6)
                {
                    enemyInstances[3].state++;
                    enemyExitManagerState++;
                }
                break;
        }
    }
}
