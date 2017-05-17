using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ClusterManager : MonoBehaviour {

    #region Fields
    private static ClusterManager instance;
    private List<Cluster> spawnedClusters;
    private GameObject clusterPrefab;
    private GameObject subclusterPrefab;
    #endregion

    #region Properties
    public static ClusterManager Instance
    {
        get { return instance; }
        set { instance = value; }
    }
    public List<Cluster> SpawnedClusters
    {
        get { return spawnedClusters; }
        set { spawnedClusters = value; }
    }
    public GameObject ClusterPrefab
    {
        get { return clusterPrefab; }
        set { clusterPrefab = value; }
    }
    public GameObject SubclusterPrefab
    {
        get { return subclusterPrefab; }
        set { subclusterPrefab = value; }
    }
    #endregion

    void Awake()
    {
        Instance = this;
        SpawnedClusters = new List<Cluster>();
        ClusterPrefab = Resources.Load("Cluster") as GameObject;
        SubclusterPrefab = Resources.Load("Subcluster") as GameObject;
    }

    /// <summary>
    /// Find cluster with most nodes.
    /// </summary>
    /// <returns>Largest cluster</returns>
    public Cluster GetLargestCluster()
    {
        Cluster largestCluster = null;
        int largestCount = 0;

        foreach(Cluster cluster in SpawnedClusters)
        {
            if(cluster.AllClusterNodes.Count > largestCount)
            {
                largestCount = cluster.ClusterNodes.Count;
                largestCluster = cluster;
            }
        }

        return largestCluster;
    }

    /// <summary>
    /// Create clusters and set connected nodes in them as children.
    /// </summary>
    public void GenerateClusters()
    {
        foreach(Node node in NodeManager.Instance.SpawnedNodes)
        {
            // Check if node is already in cluster before doing anything else.
            // After few nodes in this loop most of the other nodes will probably be clustered.
            if(!node.IsClustered)
            {
                HashSet<Node> foundNodes = new HashSet<Node>(); // Create list for found connected nodes
                HashSet<Node> spawnedNodesCopy = new HashSet<Node>(NodeManager.Instance.SpawnedNodes);

                SearchLinkedNodes(node, spawnedNodesCopy, foundNodes); // Search for connected nodes recursively

                Cluster cluster = CreateCluster();

                // Add found nodes to created cluster
                foreach (Node foundNode in foundNodes)
                {
                    cluster.ClusterNodes.Add(foundNode); // Adding to hashset so no need to check for duplicates

                    if(!cluster.AllClusterNodes.Contains(foundNode)) // check duplicates
                        cluster.AllClusterNodes.Add(foundNode);

                    foundNode.transform.SetParent(cluster.transform); // set cluster to be the parent object
                    foundNode.ParentCluster = cluster; // set parentCluster
                    foundNode.IsClustered = true;
                }
            }
        }
    }

    /// <summary>
    /// Creates subclusters and set nodes in them as children.
    /// </summary>
    public void GenerateSubclusters()
    {
        foreach(Cluster cluster in SpawnedClusters)
        {
            List<Node> nonEndpoints = cluster.ClusterNodes
                .Where(item => item.IsEndpoint != true)
                .ToList(); // get nodes that aren't endpoints

            List<Node> possibleCentrals = nonEndpoints
                .Where(item => item.ConnectedNodes.Count >= 2)
                .ToList(); // possible central nodes for subclusters

            foreach (Node node in possibleCentrals)
            {
                List<Node> foundEndpoints = node.ConnectedNodes
                    .Where(item => item.GetComponent<Node>().IsEndpoint == true)
                    .ToList();

                // Found endpoint --> create subcluster
                if (foundEndpoints.Count > 0)
                {
                    Subcluster subcluster = cluster.CreateSubcluster(node); // create subcluster

                    foreach (Node foundEndpoint in foundEndpoints)
                    {
                        cluster.ClusterNodes.Remove(foundEndpoint); // Remove endpoint from parent cluster
                        subcluster.ClusterNodes.Add(foundEndpoint); // Add endpoint to subcluster
                        foundEndpoint.GetComponent<Node>().IsInSubcluster = true;
                        foundEndpoint.transform.SetParent(subcluster.transform); // Set subcluster as a parent
                    }
                }
            }
        }
    }

    /// <summary>
    /// Search through nodes recursively for connections and set those nodes in foundNodes-HashSet.
    /// </summary>
    /// <param name="parentNode">Node that will be searched for connections.</param>
    /// <param name="spawnedNodesCopy"> All spawned nodes. Nodes will be removed from the collection as they are found.</param>
    /// <param name="foundNodes">HashSet for nodes that are connected directly or indirectly.</param>
    private void SearchLinkedNodes(Node parentNode, HashSet<Node> spawnedNodesCopy, HashSet<Node> foundNodes)
    {
        foundNodes.Add(parentNode); // Add parent node to the foundNodes
        // Remove parent node so it won't be included in search from this point on
        spawnedNodesCopy.Remove(parentNode);

        foreach (Node tempNode in spawnedNodesCopy.Reverse())
        {
            if (parentNode.ConnectedNodes.Contains(tempNode))
            {
                SearchLinkedNodes(tempNode, spawnedNodesCopy, foundNodes);
            }
        }
    }

    /// <summary>
    /// Creates the Cluster-gameobject based on prefab.
    /// </summary>
    /// <returns>Created cluster</returns>
    private Cluster CreateCluster()
    {
        GameObject instance = Instantiate(ClusterPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject; // Create cluster
        instance.name = "Cluster";

        Cluster cluster = instance.GetComponent<Cluster>();
        SpawnedClusters.Add(cluster); // add created cluster to the list

        return cluster;
    }
    
    /// <summary>
    /// Set colors for all nodes in all clusters.
    /// </summary>
    /// <param name="relativeColoring">Determines if colors are based on distinct count of different connections, or just connections.</param>
    /// <param name="hue">Hue value of HSV</param>
    public void SetColors(bool relativeColoring, float hue)
    {
        foreach(Cluster cluster in SpawnedClusters)
        {
            // Count of contacts that have unique amount of connections
            HashSet<Node> connectedNodes = cluster.GetNodes(true);
            int largestCount = connectedNodes.Select(item => item.ConnectedNodes.Count).Max();
            List<int> uniqCount = connectedNodes.Select(item => item.ConnectedNodes.Count).Distinct().OrderBy(item => item).ToList();

            // amountOfShades determines how many shades of color there will be and it also determines how many connections are needed for max color value
            int amountOfShades = 0;

            // Check if relativeColoring is true and act accordingly
            amountOfShades = (relativeColoring) ? uniqCount.Count : largestCount; //uniqCount

            float increment = 1f / amountOfShades; //amountOfShades
            int multiplier = 0;
            float addedValue = 0;

            foreach (Node node in connectedNodes)
            {
                if (uniqCount.Count == 1)
                {
                    // If all contacts in this cluster have same amount of connections, use a "halfway color" for all of them 
                    node.SetColor(1f, 0.5f, 0.5f, 1f);
                    node.SetColor(hue, 0.5f, 1f);
                }
                else
                {
                    if (relativeColoring)
                    {
                        for (int i = 0; i < uniqCount.Count; i++)
                        {
                            if (node.ConnectedNodes.Count == uniqCount[i])
                            {
                                multiplier = i + 1;
                            }
                        }
                    }
                    else
                    {
                        multiplier = node.ConnectedNodes.Count;
                    }
                    
                    addedValue = multiplier * increment;
                    node.SetColor(hue, 0f + addedValue, 1f);
                }
            }
        }
    }


    /// <summary>
    /// Set sizes for all nodes in all clusters.
    /// </summary>
    /// <param name="maxSize">Maximum size of a node.</param>
    /// <param name="minSize">Minimum size of a node.</param>
    /// <param name="increment">How much one step increases the size: how much there is size difference in general?</param>
    /// <param name="relativeSizing">Determines if sizes are based on distinct count of different connections, or just connections.</param>
    public void SetSizes(float maxSize, float minSize, float increment, bool relativeSizing)
    {
        foreach(Cluster cluster in SpawnedClusters)
        {
            // Count of contacts that have unique amount of connections
            HashSet<Node> connectedNodes = cluster.GetNodes(true);
            List<int> uniqCount = connectedNodes.Select(item => item.ConnectedNodes.Count).Distinct().OrderBy(item => item).ToList();

            float relativeIncrement = maxSize / uniqCount.Count;
            float multiplier = 0f;
            float sizeAddition = 0;

            foreach (Node node in connectedNodes)
            {
                if (uniqCount.Count == 1)
                {
                    // If all contacts in this cluster have same amount of connections, keep the default size
                }
                else if (relativeSizing)
                {
                    for(int i = 0; i < uniqCount.Count; i++)
                    {
                        if (node.ConnectedNodes.Count == uniqCount[i])
                        {
                            multiplier = i + 1;
                        }
                    }

                    sizeAddition = Mathf.Clamp(multiplier * relativeIncrement, minSize, maxSize);

                    node.IncrementSize(sizeAddition);
                }
                else
                {
                    int connectionAmount = node.ConnectedNodes.Count;
                    sizeAddition = Mathf.Clamp(connectionAmount * increment, minSize, maxSize);

                    node.IncrementSize(sizeAddition);
                }
            }
        }
    }
}
