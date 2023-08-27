using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SL {
    public class PlayerLocomotion : MonoBehaviour
    {
        PlayerManager playerManager;
        Transform cameraObject;
        InputHandler inputHandler;
        public Vector3 moveDirection;

        [HideInInspector]
        public Transform myTransform;
        [HideInInspector]
        public AnimatorHandler animatorHandler;

        public new Rigidbody rigidbody;
        public GameObject normalCamera;

        [Header("Ground & Air Detection Stats")]
        [SerializeField]
        float groundDetectionRayStartPoint = 0.5f;
        [SerializeField]
        float minimumDistanceNeededToBeginFall = 1f;
        [SerializeField]
        float groundDirectionRayDistance = 0.2f;
        LayerMask ignoreForGroundCheck;
        public float inAirTimer;

        [Header("Movement Stats")]
        [SerializeField]
        float movementSpeed = 5;
        [SerializeField]
        float sprintSpeed = 7;
        [SerializeField]
        float rotationSpeed = 10;
        [SerializeField]
        float fallingSpeed = 45;
        [SerializeField]
        float walkingSpeed = 3;

        void Start()
        {
            // Get the necessary components and references
            playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();

            playerManager.isGrounded = true;
            ignoreForGroundCheck = ~(1 << 8 | 1 << 11);

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
            if (playerManager.isInteracting || playerManager.isInAir)
                return;
                
            // Calculate move direction based on camera and input
            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;
            moveDirection.Normalize();
            moveDirection.y = 0;

            // Apply movement speed to move direction
            float speed = movementSpeed;
            if (inputHandler.sprintFlag && inputHandler.moveAmount > 0.5f) {
                speed = sprintSpeed;
                playerManager.isSprinting = true;
                moveDirection *= speed;
            } else {
                if (inputHandler.moveAmount < 0.5) {
                    moveDirection *= walkingSpeed;
                    playerManager.isSprinting = false;
                } else {
                    moveDirection *= speed;
                    playerManager.isSprinting = false;
                }
            }

            // Project the move direction onto the plane defined by normalVector
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);

            // Set the rigidbody's velocity to the projected velocity
            rigidbody.velocity = projectedVelocity;

            animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, playerManager.isSprinting);

            if (animatorHandler.canRotate) {
                HandleRotation(delta);
            }
        }

        public void HandleRollingAndSprinting(float delta) {
            if (playerManager.isInteracting || playerManager.isInAir) { // Don't want to roll if interacting with something
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

        public void HandleFalling(float delta, Vector3 moveDirection) {
            playerManager.isGrounded = false;
            RaycastHit hit;
            Vector3 origin = myTransform.position;
            origin.y += groundDetectionRayStartPoint;

            if (Physics.Raycast(origin, myTransform.forward, out hit, 0.4f)) {
                moveDirection = Vector3.zero;
            }

            if (playerManager.isInAir) {
                playerManager.isInteracting = true;
                rigidbody.AddForce(-Vector3.up * fallingSpeed);
                if (inAirTimer > 0.3f) {
                    rigidbody.AddForce(moveDirection * fallingSpeed / 30f); // Adds force in direction of player when leave platform
                } else {
                    rigidbody.AddForce(moveDirection * fallingSpeed / 5f); // Adds force in direction of player when leave platform
                }
            }

            Vector3 dir = moveDirection;
            dir.Normalize();
            origin = origin + dir * groundDirectionRayDistance;

            targetPosition = myTransform.position;

            Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeededToBeginFall, Color.red, 0.1f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, ignoreForGroundCheck)) {
                normalVector = hit.normal;
                Vector3 tp = hit.point;
                playerManager.isGrounded = true;
                targetPosition.y = tp.y;

                if (playerManager.isInAir) {
                    if (inAirTimer > 0.5f) {
                        animatorHandler.PlayTargetAnimation("Land", true);
                        inAirTimer = 0;
                    } else {
                        animatorHandler.PlayTargetAnimation("Empty", false);
                        inAirTimer = 0;
                    }

                    playerManager.isInAir = false;
                }
            } else {
                if (playerManager.isGrounded) {
                    playerManager.isGrounded = false;
                }

                if (playerManager.isInAir == false) {
                    if (playerManager.isInteracting == false) {
                        animatorHandler.PlayTargetAnimation("Falling", true);
                    }

                    Vector3 vel = rigidbody.velocity;
                    vel.Normalize();
                    rigidbody.velocity = vel * (movementSpeed / 2);
                    playerManager.isInAir = true;
                }
            }

            if (playerManager.isGrounded) {
                if (playerManager.isInteracting || inputHandler.moveAmount > 0) {
                    myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, Time.deltaTime / 0.1f);
                } else {
                    myTransform.position = targetPosition;
                }
            }

        }

        #endregion
    }
}
