using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public struct NodeStruct
{
    public Vector3 position;
    public Vector3 velocity;
}

public class Node : MonoBehaviour {

    #region Fields
    private string nodeName;

    [SerializeField]
    private bool isClustered;
    [SerializeField]
    private bool isInSubcluster;
    [SerializeField]
    private bool isEndpoint;

    private List<Node> connectedNodes;
    private Cluster parentCluster;
    private List<string> connections;
    private LineRenderer cLineRenderer;
    #endregion

    #region Properties
    public string NodeName
    {
        get { return nodeName; }
        set { nodeName = value; }
    }
    public bool IsClustered
    {
        get { return isClustered; }
        set { isClustered = value; }
    }
    public bool IsInSubcluster
    {
        get { return isInSubcluster; }
        set { isInSubcluster = value; }
    }
    public bool IsEndpoint
    {
        get { return isEndpoint; }
        set { isEndpoint = value; }
    }
    public List<Node> ConnectedNodes
    {
        get { return connectedNodes; }
        set { connectedNodes = value; }
    }
    public Cluster ParentCluster
    {
        get { return parentCluster; }
        set { parentCluster = value; }
    }
    public List<string> Connections
    {
        get { return connections; }
        set { connections = value; }
    }
    public LineRenderer CLineRenderer
    {
        get { return cLineRenderer; }
        set { cLineRenderer = value; }
    }
    #endregion

    #region Constructors
    public Node()
    {
        NodeName = "";
        IsClustered = false;
        IsInSubcluster = false;
        IsEndpoint = false;
        ConnectedNodes = new List<Node>();
        ParentCluster = null;
        Connections = new List<string>();
    }
    #endregion

    /// <summary>
    /// Increment the size of the node.
    /// </summary>
    /// <param name="increment"></param>
    public void IncrementSize(float increment)
    {
        this.gameObject.transform.localScale += new Vector3(increment, increment, increment);
    }
    /// <summary>
    /// Set color based on HSV.
    /// </summary>
    /// <param name="hue"></param>
    /// <param name="saturation"></param>
    /// <param name="value"></param>
    public void SetColor(float hue, float saturation, float value)
    {
        this.gameObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(hue, saturation, value);
    }
    /// <summary>
    /// Set color based on RGBA.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    public void SetColor(float r, float g, float b, float a)
    {
        Renderer renderer = this.gameObject.GetComponent<Renderer>();
        renderer.material.color = new Color(r, g, b, a);
    }

    /// <summary>
    /// Return a list of nodes(names as string) that are connected to this one.
    /// </summary>
    /// <returns></returns>
    public HashSet<string> GetConnections()
    {
        HashSet<string> connectedNodes = new HashSet<string>();
        
        foreach (string connection in Connections)
        {
                if(connection.Trim().Length != 0)
                    connectedNodes.Add(connection);
        }
        
        return connectedNodes;
    }
    
    /// <summary>
    /// Draw lines to all nodes connected to this one.
    /// </summary>
    /// <remarks>
    /// This isn't a good approach performance-wise but it makes the lines look better (Unity LineRenderer got some problems when drawing only a single line between two nodes).
    /// </remarks>
    public void DrawLines()
    {
        if (CLineRenderer != null)
        {
            Vector3 myPosition = transform.position;

            CLineRenderer.SetPosition(0, myPosition);
            int counter = 1;

            for (int i = 0; i < ConnectedNodes.Count; i++)
            {
                CLineRenderer.SetPosition(counter, ConnectedNodes[i].transform.position);
                CLineRenderer.SetPosition(counter + 1, myPosition);
                counter += 2;
            }
        }
        else
        {
            Debug.Log("Null lineRenderer on: " + name);
        }
    }

    /// <summary>
    /// Calculates position of this node based on calculated forces between this and other nodes.
    /// </summary>
    /// <param name="visualizationSpeed">Multiplier for deltatime</param>
    /// <returns>Calculated new position</returns>
    public Vector3 CalculatePosition(float visualizationSpeed)
    {
        Vector3 netForce = Vector3.zero;
        Vector3 ownPosition = transform.position;
        
        if (IsEndpoint && true == false)
        {
            Node parentNode = ConnectedNodes.SingleOrDefault();

            if (parentNode != null)
            {
                // Calculate forces between this endpoint and endpoints in this subcluster
                foreach (Node endpoint in parentNode.ConnectedNodes)
                {
                    if (!this.Equals(endpoint)) // && contactGoEndpoint.GetComponent<Contact>().IsEndpoint
                    {
                        Vector3 direction = endpoint.transform.position - ownPosition;
                        netForce += Util.CalcRepulsion(ownPosition, endpoint.transform.position, Settings.Repulsion * 2f) * direction.normalized;
                    }
                }

                // Calculate forces between this endpoint and subclusters central contact
                Vector3 direction2 = parentNode.transform.position - ownPosition;

                netForce += Util.CalcRepulsion(ownPosition, parentNode.transform.position, Settings.Repulsion) * direction2.normalized;
                netForce += Util.CalcAttraction(ownPosition, parentNode.transform.position, Settings.Attraction) * direction2.normalized;
            }
        }
        else if (!IsEndpoint && IsInSubcluster && true == false)
        {
            foreach (Node node in ParentCluster.AllClusterNodes)
            {
                if (!node.Equals(this) && !node.IsEndpoint)
                {
                    Vector3 direction = node.transform.position - ownPosition;
                    netForce += Util.CalcRepulsion(ownPosition, node.transform.position, Settings.Repulsion * (2f)) * direction.normalized; //  + contact.GetComponent<Contact>().ConnectedContacts.Count / 4f
                }
                // Include all enpoints except this contacts own
                else if (node.IsEndpoint && !ConnectedNodes.Contains(node))
                {
                    Vector3 direction = node.transform.position - ownPosition;
                    netForce += Util.CalcRepulsion(ownPosition, node.transform.position, Settings.Repulsion * (2f)) * direction.normalized;
                }
            }

            foreach (Node node in ConnectedNodes)
            {
                if (!node.IsEndpoint)
                {
                    Vector3 direction = node.transform.position - ownPosition;
                    netForce += Util.CalcAttraction(ownPosition, node.transform.position, Settings.Attraction) * direction.normalized;
                }
            }
        }
        else
        {
            foreach (Node node in ConnectedNodes)
            {
                if (!node.IsEndpoint)
                {
                    Vector3 direction = node.transform.position - ownPosition;
                    netForce += Util.CalcAttraction(ownPosition, node.transform.position, Settings.Attraction) * direction.normalized;
                }
            }

            foreach (Node node in ParentCluster.GetComponent<Cluster>().AllClusterNodes)
            {
                if (!node.Equals(this)) // && !contact.GetComponent<Contact>().IsEndpoint
                {
                    Vector3 direction = node.transform.position - ownPosition;
                    netForce += Mathf.Min(Util.CalcRepulsion(ownPosition, node.transform.position, Settings.Repulsion), 2.0f) * direction.normalized;
                }
            }
        }

        Vector3 updatedPosition = netForce * (Time.deltaTime * visualizationSpeed) * (Time.deltaTime * visualizationSpeed);
        float updatedDistance = Vector3.Distance(updatedPosition, transform.position);

        if (updatedDistance > 5.0f)
            updatedPosition = updatedPosition * 0.2f;

        return updatedPosition;

        //gameObject.GetComponent<Rigidbody>().AddForce(netForce, ForceMode.Force);
        //gameObject.transform.position += netForce * Time.deltaTime * Time.deltaTime;
    }
}
