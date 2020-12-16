using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private GameManager gm;
    public Vector3 spawnExit;
    Vector3 target;
    float speed = 2f;
    Node currentNode;
    public int state = 1;
    private Animator animator;

    private void Start()
    {
        gm = GameManager.Instance;
        speed = gm.clockRate;
        currentNode = gm.GetNode(target);
        target = transform.position;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        MoveLerp(target);
    }

    /// <summary>
    /// ClockUpdate is called every 1/clockRate seconds 
    /// </summary>
    public void ClockUpdate()
    {
        switch (state)
        {
            case 1: // In spawn state
                target = transform.position;
                break;
            case 2: // Exit spawn state
                target = spawnExit;
                if (transform.position == spawnExit)
                {
                    state++;
                    ClockUpdate();
                }
                break;
            case 3: // Move state
                Node lastNode = currentNode;
                currentNode = gm.GetNode(transform.position);
                List<Node> nextNodeList = new List<Node>(currentNode.neighbors);
                nextNodeList.Remove(lastNode);
                target = gm.CoordToPos((nextNodeList.Count > 0 ? nextNodeList[Random.Range(0, nextNodeList.Count)] : currentNode.neighbors[0]).coord);
                Rotate();
                break;
        }
    }

    private void MoveLerp(Vector3 lerpTarget)
    {
        transform.position = Vector3.Lerp(transform.position, lerpTarget, Time.deltaTime * speed / Vector3.Distance(transform.position, lerpTarget));
    }

    private void Rotate()
    {
        if (target.x < transform.position.x) animator.SetInteger("EnemyDir", 0);
        else if (target.x > transform.position.x) animator.SetInteger("EnemyDir", 1);
        else if (target.y > transform.position.y) animator.SetInteger("EnemyDir", 2);
        else animator.SetInteger("EnemyDir", 3);
    }
}
