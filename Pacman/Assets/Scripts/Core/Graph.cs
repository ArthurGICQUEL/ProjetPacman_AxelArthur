using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Graph
{
    public Dictionary<Vector2Int, Node> mazeDic;

    private Tilemap tilemap;

    public Graph(Tilemap tilemap)
    {
        this.tilemap = tilemap;
        mazeDic = new Dictionary<Vector2Int, Node>();
        InitializeGraph();
    }

    private void InitializeGraph()
    {
        tilemap.CompressBounds(); //When you draw a tilemap, its size can only grows, by using "CompressBounds" you allow it to shrink to its "real" size
        BoundsInt bounds = tilemap.cellBounds;

        //Creates a dictionary with a node for each tile in "tilemap" wich has for key the coordinates of the tile
        for (int x = bounds.xMin; x < bounds.xMax; x += 1)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y += 1)
            {
                if (tilemap.HasTile(new Vector3Int(x, y, 0)))
                {
                    mazeDic[new Vector2Int(x, y)] = new Node(new Vector2Int(x, y));
                }
            }
        }
        //Puts all the neighbor nodes of each node in its "neighbors" list
        foreach (Vector2Int coord1 in mazeDic.Keys)
        {
            foreach (Vector2Int coord2 in mazeDic.Keys)
            {
                if (coord1 != coord2
                    && (((coord1.x - 1 == coord2.x || coord1.x + 1 == coord2.x) && coord1.y == coord2.y)
                    || ((coord1.y - 1 == coord2.y || coord1.y + 1 == coord2.y) && coord1.x == coord2.x)))
                {
                    mazeDic[coord1].neighbors.Add(mazeDic[coord2]);
                }
            }
        }
    }
    /// <param name="coord">Coordinates of a tile</param>
    /// <returns>true if there is a node at "coord" in "mazeDic" else false</returns>
    public bool HasNode(Vector2Int coord)
    {
        return mazeDic.ContainsKey(coord);
    }

    /// <param name="coord">Coordinates of a tile</param>
    /// <returns>the node representing the cell with "coord" coordinates</returns>
    public Node GetNode(Vector2Int coord)
    {
        if (HasNode(coord))
        {
            return mazeDic[coord];
        }
        return null;
    }
}
