using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SL {
    public class PlayerLocomotion : MonoBehaviour
    {
        Transform cameraObject;
        InputHandler inputHandler;
        Vector3 moveDirection;

        [HideInInspector]
        public Transform myTransform;
        [HideInInspector]
        public AnimatorHandler animatorHandler;

        public new Rigidbody rigidbody;
        public GameObject normalCamera;

        [Header("Stats")]
        [SerializeField]
        float movementSpeed = 5;
        [SerializeField]
        float sprintSpeed = 7;
        [SerializeField]
        float rotationSpeed = 10;

        public bool isSprinting;

        void Start()
        {
            // Get the necessary components and references
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();

        }

        public void FixedUpdate() {
            float delta = Time.deltaTime;

            // Process input
            isSprinting = inputHandler.b_Input;
            inputHandler.TickInput(delta);
            HandleMovement(delta);
            HandleRollingAndSprinting(delta);

        }

        #region Movement
        Vector3 normalVector;
        Vector3 targetPosition;

        private void HandleRotation(float delta) {
            Vector3 targetDir = Vector3.zero;
            float moveOverride = inputHandler.moveAmount;

            // Calculate target direction based on camera and input
            targetDir = cameraObject.forward * inputHandler.vertical;
            targetDir += cameraObject.right * inputHandler.horizontal;

            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
                targetDir = myTransform.forward;

            float rs = rotationSpeed;

            // Smoothly rotate the player towards the target direction
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

            myTransform.rotation = targetRotation;
        }

        public void HandleMovement(float delta) {
            if (animatorHandler.anim.GetBool("isInteracting"))
                return;
                
            // Calculate move direction based on camera and input
            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;
            moveDirection.Normalize();
            moveDirection.y = 0;

            // Apply movement speed to move direction
            float speed = movementSpeed;
            if (inputHandler.sprintFlag) {
                speed = sprintSpeed;
                isSprinting = true;
                moveDirection *= speed;
            } else {
                moveDirection *= speed;
            }

            // Project the move direction onto the plane defined by normalVector
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);

            // Set the rigidbody's velocity to the projected velocity
            rigidbody.velocity = projectedVelocity;

            animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, isSprinting);

            if (animatorHandler.canRotate) {
                HandleRotation(delta);
            }
        }

        public void HandleRollingAndSprinting(float delta) {
            if (animatorHandler.anim.GetBool("isInteracting")) { // Don't want to roll if interacting with something
                return;
            }
            if (inputHandler.rollFlag) {
                moveDirection = cameraObject.forward * inputHandler.vertical;
                moveDirection += cameraObject.right * inputHandler.horizontal;

                if (inputHandler.moveAmount > 0) {
                    animatorHandler.PlayTargetAnimation("Rolling", true);
                    moveDirection.y = 0;
                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;
                } else {
                    animatorHandler.PlayTargetAnimation("Backstep", true);
                }
            }
        }

        #endregion
    }
}
