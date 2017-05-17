using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControls : MonoBehaviour {

    public float movementSpeed;
    public Camera playerCamera;
	
	// Update is called once per frame
	void Update () {

        float horizontal = Input.GetAxis("Horizontal");
        transform.Translate((new Vector3(horizontal, 0f, 0f) * movementSpeed) * Time.deltaTime, playerCamera.transform);

        float vertical = Input.GetAxis("Vertical");
        transform.Translate((new Vector3(0f, 0f, vertical) * movementSpeed) * Time.deltaTime, playerCamera.transform);

        if (Input.GetButton("Ascend"))
        {
            transform.Translate((Vector3.up * movementSpeed) * Time.deltaTime, playerCamera.transform);
        }
        if (Input.GetButton("Descend"))
        {
            transform.Translate((Vector3.down * movementSpeed) * Time.deltaTime, playerCamera.transform);
        }
        if (Input.GetButtonDown("Stop"))
        {
            Visualize.runCalculations = (Visualize.runCalculations == true) ? false : true;

            if (!Visualize.runCalculations)
            {
                foreach (Node node in NodeManager.Instance.SpawnedNodes)
                {
                    node.DrawLines();
                }
            }
            else
            {
                // Remove lines
                NodeManager.Instance.RemoveLines();
            }
        }
        if(Input.GetButtonDown("Cancel")) // could not rename this. Unity flipped out
        {
            Visualize.runCalculations = false;
            SceneManager.LoadScene("startScene");
        }

        // Hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        RotateView();
    }

    // Just a quick makeshift solution for now
    private void RotateView()
    {
        float xAxis = Input.GetAxis("Mouse X");
        float yAxis = Input.GetAxis("Mouse Y");

        transform.localRotation *= Quaternion.Euler(0f, xAxis, 0f);
        playerCamera.transform.localRotation *= Quaternion.Euler(-yAxis, 0f, 0f);
    }
}
