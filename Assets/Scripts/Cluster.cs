using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Cluster : MonoBehaviour {

    #region Fields
    private HashSet<Node> clusterNodes; // only nodes inside this cluster. No subcluster nodes included
    private List<Node> allClusterNodes; // subclusters nodes included
    private HashSet<Subcluster> subclusters;
    #endregion

    #region Properties
    public HashSet<Node> ClusterNodes
    {
        get { return clusterNodes; }
        set { clusterNodes = value; }
    }
    public List<Node> AllClusterNodes
    {
        get { return allClusterNodes; }
        set { allClusterNodes = value; }
    }
    public HashSet<Subcluster> Subclusters
    {
        get { return subclusters; }
        set { subclusters = value; }
    }
    #endregion

    #region Constructor
    public Cluster()
    {

        ClusterNodes = new HashSet<Node>();
        AllClusterNodes = new List<Node>();
        Subclusters = new HashSet<Subcluster>();
    }
    #endregion

    /// <summary>
    /// Finds the central node of the cluster based on connections.
    /// </summary>
    /// <returns>Node with most connections</returns>
    public Node GetCentralNode()
    {
        Node returnedNode = null;
        int largestAmount = 0;

        foreach(Node node in GetNodes(true))
        {
            int currentAmount = node.ConnectedNodes.Count;

            if (currentAmount >= largestAmount)
            {
                largestAmount = currentAmount;
                returnedNode = node;
            }
        }

        return returnedNode;
    }

    /// <summary>
    /// Get contacts of this cluster with or without endpoints (leaves).
    /// </summary>
    /// <param name="includeEndpoints"></param>
    /// <returns>Found nodes</returns>
    public HashSet<Node> GetNodes(bool includeEndpoints)
    {
        HashSet<Node> connectedNodes = new HashSet<Node>(ClusterNodes);

        foreach(Subcluster subcluster in Subclusters)
        {
            if(includeEndpoints)
            {
                connectedNodes.UnionWith(subcluster.ClusterNodes);
            }
            else
            {
                Node subclusterCentral = subcluster.GetCentralNode();

                if (subclusterCentral != null)
                    connectedNodes.Add(subclusterCentral);
            }
        }

        return connectedNodes;
    }

    /// <summary>
    /// Creates a subcluster for this cluster.
    /// Given node will be moved from the parent clusters list of nodes to the subclusters list.
    /// </summary>
    /// <param name="node">Central node of subcluster.</param>
    /// <returns>Instance of created subcluster</returns>
    public Subcluster CreateSubcluster(Node node)
    {
        GameObject instance = Instantiate(ClusterManager.Instance.SubclusterPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject; // Create cluster

        instance.name = "Subcluster";
        instance.transform.SetParent(this.transform); // Set current cluster to be parent for the subcluster
        node.transform.SetParent(instance.transform); // set the subcluster to be the parent object of the node
        node.IsClustered = true;
        node.IsInSubcluster = true;

        ClusterNodes.Remove(node); // Remove node from parent cluster

        Subcluster subcluster = instance.GetComponent<Subcluster>();
        subcluster.ClusterNodes.Add(node); // Add node to subcluster

        Subclusters.Add(subcluster); // add created cluster to the list

        return subcluster;
    }
}
