﻿using UnityEngine;

namespace Assets.Scripts
{
    public class Cursor : MonoBehaviour {

        private MeshRenderer _meshRenderer;

        // Use this for initialization
        void Start()
        {
            // Grab the mesh renderer that's on the same object as this script.
            _meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            // Do a raycast into the world based on the user's
            // head position and orientation.
            Vector3 headPosition = Camera.main.transform.position;
            Vector3 gazeDirection = Camera.main.transform.forward;

            RaycastHit hitInfo;

            bool raycastResult = Physics.Raycast(headPosition, gazeDirection, out hitInfo);

            if (raycastResult)
            {
                // If the raycast hit a hologram...
                // Display the cursor mesh.
                _meshRenderer.enabled = true;

                // Move thecursor to the point where the raycast hit.
                this.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y + .01f, hitInfo.point.z);

                // Rotate the cursor to hug the surface of the hologram.
                this.transform.rotation = Quaternion.FromToRotation(Vector3.back, hitInfo.normal);
            }
            else
            {
                // If the raycast did not hit a hologram, hide the cursor mesh.
                _meshRenderer.enabled = false;
            }
        }
    }
}
