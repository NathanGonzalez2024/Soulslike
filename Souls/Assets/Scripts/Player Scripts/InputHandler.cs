using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SL {
    public class InputHandler : MonoBehaviour
    {
        // Public variables to store input values
        public float horizontal;
        public float vertical;
        public float moveAmount;
        public float mouseX;
        public float mouseY;

        // Input actions object to handle player input
        PlayerControls inputActions;

        // Input vectors for movement and camera
        Vector2 movementInput;
        Vector2 cameraInput;

        // Method called when the component is enabled
        public void OnEnable() {
            // Initialize inputActions if it's null
            if (inputActions == null) {
                inputActions = new PlayerControls();

                // Assign a lambda expression to handle movement input
                inputActions.PlayerMovement.Movement.performed += inputActions => movementInput = inputActions.ReadValue<Vector2>();

                // Assign a lambda expression to handle camera input
                inputActions.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
            }

            // Enable input actions
            inputActions.Enable();
        }

        // Method called when the component is disabled
        private void OnDisable() {
            // Disable input actions
            inputActions.Disable();
        }

        // Method called to process input in the game loop
        public void TickInput(float delta) {
            // Call MoveInput method to handle movement input
            MoveInput(delta);
        }

        // Method to handle movement input
        private void MoveInput(float delta) {
            // Assign movement input values to respective variables
            horizontal = movementInput.x;
            vertical = movementInput.y;

            // Calculate moveAmount as the clamped sum of absolute horizontal and vertical inputs
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

            // Assign camera input values to respective variables
            mouseX = cameraInput.x;
            mouseY = cameraInput.y;
        }
    }
}
