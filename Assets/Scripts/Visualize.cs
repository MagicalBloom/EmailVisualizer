using UnityEngine;
using System.Collections.Generic;

public class Visualize : MonoBehaviour {

    #region Fields
    public static bool runCalculations = false; // This determines when calculations are run in the Update()
    private bool gpgpu = true; // use graphics card?

    //GPU stuff
    public ComputeShader repulsionShader;
    private ComputeBuffer repulsionBuffer;
    private int repulsionKernel;

    // Data structures
    private NodeStruct[] gpuBufferArray;
    private Node[] nodeArray;
    #endregion

    /// <summary>
    /// Gets everything ready for visualization.
    /// </summary>
    void Start()
    {
        // Set targetFramerate
        Application.targetFrameRate = 120;

        // Read csv
        foreach (List<string> connections in Parser.ReadCsv(Settings.CsvPath))
        {
            NodeManager.Instance.SpawnNodes(connections);
        }

        NodeManager.Instance.SetConnectedNodes(); // Populate list of connected nodes for every node
        ClusterManager.Instance.GenerateClusters(); // Generate clusters (cluster = group of nodes that are connected to each other directly or indirectly)
        NodeManager.Instance.SetEndpoints();  // Determine whether nodes are endpoints or not (endpoint = leaf in graph terminology = a node that is only connected to one other node)
        ClusterManager.Instance.GenerateSubclusters(); // Generate subclusters (subcluster = group of nodes which consist of single normal node and endpoints connected to that node)
        NodeManager.Instance.InitLinerenderers(); // Initialize line renderers

        // Styling
        ClusterManager.Instance.SetColors(Settings.RelativeColoring, Settings.Hue);
        ClusterManager.Instance.SetSizes(Settings.MaxSize, Settings.MinSize, Settings.Increment, Settings.RelativeSizing);

        // Find center contact from largest cluster and center it. TODO: The proper method for finding center node
        Cluster largestCluster = ClusterManager.Instance.GetLargestCluster();
        Node centerNode = largestCluster.GetComponent<Cluster>().GetCentralNode();
        if (centerNode != null) {
            centerNode.transform.position = Vector3.zero;
        }

        // Disable all except largest cluster
        foreach(Cluster cluster in ClusterManager.Instance.SpawnedClusters)
        {
            if (!cluster.Equals(largestCluster))
                cluster.gameObject.SetActive(false);
        }

        // Create random positions for contacts
        foreach (Node node in largestCluster.AllClusterNodes)
        {
            node.transform.position = Util.CreatePositionRandom(NodeManager.Instance.SpawnedNodes.Count / 2);
        }

        // Init variables
        nodeArray = largestCluster.AllClusterNodes.ToArray();
        gpuBufferArray = new NodeStruct[nodeArray.Length];

        FillBufferArray();
        InitRepulsionShader();

        runCalculations = true; // start calculations for visualization
    }

    /// <summary>
    /// Creates the buffer for the shader. Also finds the kernel id that will be needed when running the shader and sets repulsion-constant for the shader.
    /// </summary>
    private void InitRepulsionShader()
    {
        // Create the buffer and set data to it
        // Single element of buffer is 4(float) * 6 = 24 bytes
        repulsionBuffer = new ComputeBuffer(nodeArray.Length, sizeof(float) * 6);

        repulsionKernel = repulsionShader.FindKernel(Settings.GPU_KERNEL); // Find the kernel id
        repulsionShader.SetFloat(Settings.GPU_REPULSION, Settings.Repulsion); // Set repulsion-constant value
    }

    /// <summary>
    /// Set buffer and deltatime for the shader and run it.
    /// </summary>
    private void RunRepulsionShader()
    {
        float time = Time.deltaTime * Settings.VisualizationSpeed;

        // Set resources to the shader
        repulsionShader.SetBuffer(repulsionKernel, Settings.GPU_BUFFER, repulsionBuffer);
        repulsionShader.SetFloat(Settings.GPU_DELTATIME, time);

        // Run the shader
        var numberOfGroups = Mathf.CeilToInt((float)nodeArray.Length / 128); //GroupSize = 128
        repulsionShader.Dispatch(repulsionKernel, numberOfGroups, 1, 1);
    }

    /// <summary>
    /// Calculates the velocity from attraction forces and direction. Updates the gpu buffer with velocity and current position.
    /// </summary>
    private void CalcAttraction()
    {
        Vector3 updatedVelocity;
        Vector3 parentPosition;

        for (int i = 0; i < nodeArray.Length; i++)
        {
            updatedVelocity = Vector3.zero;
            parentPosition = nodeArray[i].transform.position;

            for (int j = 0; j < nodeArray[i].ConnectedNodes.Count; j++)
            {
                updatedVelocity += Util.CalcAttraction(parentPosition, nodeArray[i].ConnectedNodes[j].transform.position, Settings.Attraction) *
                    (nodeArray[i].ConnectedNodes[j].transform.position - parentPosition).normalized;
            }

            gpuBufferArray[i].velocity = updatedVelocity;
            gpuBufferArray[i].position = parentPosition;
        }
    }

    /// <summary>
    /// Fills the gpu buffer with structs
    /// </summary>
    private void FillBufferArray()
    {
        for (int i = 0; i < nodeArray.Length; i++)
        {
            // Create struct for GPU buffer
            NodeStruct nodeStruct = new NodeStruct();
            nodeStruct.position = nodeArray[i].transform.position;
            nodeStruct.velocity = Vector3.zero;

            gpuBufferArray[i] = nodeStruct; // place created struct to array
        }
    }

    /// <summary>
    /// Runs calculations for the visualization.
    /// If gpgpu is enabled, attraction is calculated on the cpu and repulsion is calculated on the gpu.
    /// If gpgpu is disabled all calculations are handled on the cpu-side.
    /// </summary>
    void Update()
    {
        if (runCalculations && gpgpu) {
            CalcAttraction(); // Calculate attraction and save results to gpuBufferArray

            repulsionBuffer.SetData(gpuBufferArray); // Update data for the buffer (gpu)

            RunRepulsionShader(); // Run the shader

            repulsionBuffer.GetData(gpuBufferArray); // Get data from the buffer (gpu)

            for (int i = 0; i < nodeArray.Length; i++)
            {
                nodeArray[i].transform.position += gpuBufferArray[i].position; // set the position

                if (Settings.DrawLines)
                    nodeArray[i].DrawLines();
            }
        }
        else if(runCalculations && !gpgpu)
        {
            for(int i = 0; i < nodeArray.Length; i++)
            {
                // Run force calculations with Nodes own method for both attraction and repulsion
                nodeArray[i].transform.position += nodeArray[i].CalculatePosition(Settings.VisualizationSpeed);

                if(Settings.DrawLines)
                    nodeArray[i].DrawLines();
            }
        }
    }

    /// <summary>
    /// Release the compute buffer
    /// </summary>
    void OnDestroy()
    {
        repulsionBuffer.Release();
    }
}
