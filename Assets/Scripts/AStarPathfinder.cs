using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Required for OrderBy

// Make sure the BloodVesselGenerator.Node class is accessible.
// If it's a private nested class, you might need to make it public or internal,
// or define a similar public Node structure here for the A* algorithm to use.
// For this task, assume BloodVesselGenerator.Node is accessible.

public static class AStarPathfinder
{
    public static List<BloodVesselGenerator.Node> FindPath(BloodVesselGenerator.Node startNode, BloodVesselGenerator.Node goalNode)
    {
        List<BloodVesselGenerator.Node> openSet = new List<BloodVesselGenerator.Node>();
        HashSet<BloodVesselGenerator.Node> closedSet = new HashSet<BloodVesselGenerator.Node>();
        openSet.Add(startNode);

        startNode.gCost = 0;
        startNode.hCost = Vector3.Distance(startNode.position, goalNode.position);

        while (openSet.Count > 0)
        {
            BloodVesselGenerator.Node currentNode = openSet.OrderBy(node => node.gCost + node.hCost).First();

            if (currentNode == goalNode)
            {
                return ReconstructPath(goalNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (var neighborEntry in currentNode.neighbors)
            {
                BloodVesselGenerator.Node neighbor = neighborEntry.Key;
                float tentativeGCost = currentNode.gCost + neighborEntry.Value; // neighborEntry.Value is the distance/cost

                if (closedSet.Contains(neighbor))
                    continue;

                if (tentativeGCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = Vector3.Distance(neighbor.position, goalNode.position);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // No path found
        return null;
    }

    private static List<BloodVesselGenerator.Node> ReconstructPath(BloodVesselGenerator.Node currentNode)
    {
        List<BloodVesselGenerator.Node> path = new List<BloodVesselGenerator.Node>();
        while (currentNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }
}
