using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

    // Hud
    public GameObject hudContacts;
    public GameObject hudGroups;
    public GameObject hudFps;

    private float frameCount = 0f;
    private float dt = 0f;
    private float fps = 0f;
    private float updateRate = 2f;

    private bool updated = false;
	
	// Update is called once per frame
	void Update () {
        // FPS Counter
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1f / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0f;
            dt -= 1f / updateRate;

            hudFps.GetComponent<Text>().text = fps.ToString("F1");
        }

        // Update hud
        if (Visualize.runCalculations && updated == false)
        {
            Cluster largestCluster = ClusterManager.Instance.GetLargestCluster();
            string contactCountText = largestCluster.AllClusterNodes.Count.ToString() + " (" + NodeManager.Instance.SpawnedNodes.Count.ToString() + ")";

            hudContacts.GetComponent<Text>().text = contactCountText;
            hudGroups.GetComponent<Text>().text = ClusterManager.Instance.SpawnedClusters.Count.ToString();
            updated = true;
        }
    }
}
