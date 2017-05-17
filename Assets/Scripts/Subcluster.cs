using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subcluster : MonoBehaviour {

    private HashSet<Node> clusterNodes;

    public HashSet<Node> ClusterNodes
    {
        get { return clusterNodes; }
        set { clusterNodes = value; }
    }

    public Subcluster()
    {
        ClusterNodes = new HashSet<Node>();
    }

    public Node GetCentralNode()
    {
        Node returnedNode = null;

        foreach (Node node in ClusterNodes)
        {
            if (node.IsEndpoint)
            {
                returnedNode = node;
            }
        }

        return returnedNode;
    }
}
