using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NodeManager : MonoBehaviour {

    #region Fields
    private static NodeManager instance;
    private HashSet<Node> spawnedNodes;
    private GameObject nodePrefab;
    #endregion

    #region Properties
    public HashSet<Node> SpawnedNodes
    {
        get { return spawnedNodes; }
        set { spawnedNodes = value; }
    }
    public GameObject NodePrefab
    {
        get { return nodePrefab; }
        set { nodePrefab = value; }
    }
    public static NodeManager Instance
    {
        get { return NodeManager.instance; }
        set { NodeManager.instance = value; }
    }
    #endregion

    void Awake()
    {
        Instance = this;
        Instance.SpawnedNodes = new HashSet<Node>();
        Instance.NodePrefab = Resources.Load("Node") as GameObject;
    }

    /// <summary>
    /// Find endpoints (leaves) and set them as such by IsEndpoint-boolean.
    /// </summary>
    public void SetEndpoints()
    {
        foreach(Cluster cluster in ClusterManager.Instance.SpawnedClusters)
        {
            HashSet<Node> clusterNodes = cluster.GetNodes(true);
            
            // If cluster have 2 or less contacts, there can't be endpoints (leaves)
            if(clusterNodes.Count > 2)
            {
                foreach (Node node in clusterNodes)
                {
                    int connectionAmount = node.ConnectedNodes.Count;

                    if (connectionAmount < 2)
                    {
                        node.IsEndpoint = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// "Remove" all lines by setting their local position to 0.
    /// </summary>
    public void RemoveLines()
    {
        foreach(Node node in SpawnedNodes)
        {
            LineRenderer lineRenderer = node.GetComponent<LineRenderer>();

            for (int i = 0; i < lineRenderer.numPositions; i++)
            {
                lineRenderer.SetPosition(i, Vector3.zero);
            }
        }
    }

    /// <summary>
    /// Loop through all nodes and set connected Nodes based on Node names.
    /// </summary>
    public void SetConnectedNodes()
    {
        foreach(Node node in SpawnedNodes)
        {
            HashSet<string> connections = node.GetConnections();
            
            // Look for connections from all spawned nodes
            foreach(Node result in SpawnedNodes.Where(item => connections.Contains(item.NodeName)))
            {
                node.ConnectedNodes.Add(result);
            }
        }
    }

    /// <summary>
    /// Initialize line renderers for nodes.
    /// </summary>
    /// <remarks>
    /// Line width is also set in here. Value for line width is taken from Settings-class. 
    /// </remarks>
    public void InitLinerenderers()
    {
        foreach(Node node in SpawnedNodes)
        {
            LineRenderer lineRenderer = node.GetComponent<LineRenderer>();
            lineRenderer.numPositions = (node.ConnectedNodes.Count * 2) + 1; // + 1 for contacts own position
            for(int i = 0; i < lineRenderer.numPositions; i++)
            {
                lineRenderer.SetPosition(i, Vector3.zero);
            }
            lineRenderer.startWidth = Settings.ConnectionLineWidth;
            lineRenderer.endWidth = Settings.ConnectionLineWidth;
            node.CLineRenderer = lineRenderer;
        }
    }

    /// <summary>
    /// Creates the node-gameobject based on prefab. Sets the name of the gameobject and nodeName inside Node class.
    /// Adds created node to the list of spawned nodes.
    /// </summary>
    /// <param name="nodeName">Name of the node (email)</param>
    /// <returns>Returns created node or node that already exists with same name. If nodeName is empty, return null.</returns>
    private Node CreateNode(string nodeName)
    {
        // Check if the nodes name (email) is empty
        if (nodeName.Trim().Length == 0)
            return null;

        // Check if this node already exists
        Node foundNode = Instance.SpawnedNodes.FirstOrDefault(item => item.NodeName == nodeName);
        if (foundNode != null)
            return foundNode;

        GameObject instance = Instantiate(NodePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        instance.name = "Node_" + nodeName;

        Node node = instance.GetComponent<Node>();
        node.NodeName = nodeName;

        Instance.SpawnedNodes.Add(node);

        return node;
    }

    /// <summary>
    /// Spawn nodes from single email (nodes for sender and receivers). Add connections between these nodes.
    /// </summary>
    /// <param name="connections">List of sender and receivers. First item in the list is the sender.</param>
    public void SpawnNodes(List<string> connections)
    {
        for(int i = 0; i < connections.Count; i++)
        {
            Node spawnedNode = CreateNode(connections[i]);
            if (spawnedNode != null)
            {
                if (i == 0)
                {
                    for (int j = 0; j < connections.Count; j++)
                    {
                        // Add receivers to senders list of connections
                        if (i != j)
                            spawnedNode.Connections.Add(connections[j]);
                    }
                }
                else
                {
                    // Add sender to receivers list of connections
                    spawnedNode.Connections.Add(connections[0]);
                }
            }
        }
    }
}
