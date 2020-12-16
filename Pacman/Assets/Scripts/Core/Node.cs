using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node // each node represents a cell
{
    public List<Node> neighbors; // represents all the cells juxtaposed to this cell
    public enum tile
    {
        nothing,
        dot,
        energizer,
        tpOne,
        tpTwo,
        tpThree,
        tpFour
    }
    public tile has; // tells what is on the cell represented by this node
    public Vector2Int coord; // coordinates of the Node
    public Node(Vector2Int coord)
    {
        neighbors = new List<Node>();
        has = tile.nothing;
        this.coord = coord;
    }
}
