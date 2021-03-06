﻿#if RTSCore_StandardAssets_OLD
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;
using UnityStandardAssets.CrossPlatformInput;

namespace RTSCoreFramework.StandardAssets
{

    public class RTSFreeLookCamCore : PivotBasedCameraRig
    {
        private RTSGameMaster gamemaster
        {
            get { return RTSGameMaster.thisInstance; }
        }

        private bool moveCamera = false;
        private bool bZoomCamera = false;
        private bool bZoomIsPostive = false;

        #region ZoomValues
        private float zoomOutPivot = 0.5f;
        private float zoomOutCamera = -0.5f;

        private float zoomInPivot = -0.5f;
        private float zoomInCamera = 0.5f;

        private float zoomSmoothing = 10f;

        float zoomPivot
        {
            get { return bZoomIsPostive ? zoomInPivot : zoomOutPivot; }
        }

        float zoomCam
        {
            get { return bZoomIsPostive ? zoomInCamera : zoomOutCamera; }
        }

        float zoomRotateCam
        {
            get { return bZoomIsPostive ? zoomCameraRotatePos : zoomCameraRotateNeg; }
        }

        //Pivot Y = 3 Norm
        //Cam Z = -5 Norm
        private float zoomPivotNormal = 3f;
        private float zoomCameraNormal = -5f;

        private float zoomPivotMaxAdd = 3f;
        private float zoomPivotMinAdd = 0f;

        private float zoomCameraMaxAdd = 0f;
        private float zoomCameraMinAdd = -3f;

        //Extra - Camera X Rotation
        float zoomCameraRotatePos = -5f;
        float zoomCameraRotateNeg = 5f;
        float zoomCameraMinRotation = 0f;
        float zoomCameraMaxRotation = 15f;

        private float zoomPivotMax
        {
            get { return zoomPivotNormal + zoomPivotMaxAdd; }
        }

        private float zoomPivotMin
        {
            get { return zoomPivotNormal + zoomPivotMinAdd; }
        }

        private float zoomCameraMax
        {
            get { return zoomCameraNormal + zoomCameraMaxAdd; }
        }

        private float zoomCameraMin
        {
            get { return zoomCameraNormal + zoomCameraMinAdd; }
        }
        #endregion

        #region UnityMessages
        protected override void Start()
        {
            base.Start();
        }

        private void OnEnable()
        {
            gamemaster.EventHoldingRightMouseDown += ToggleMoveCamera;
            gamemaster.OnAllySwitch += OnAllySwitch;
            gamemaster.EventEnableCameraZoom += ToggleCameraZoom;
        }

        private void OnDisable()
        {
            gamemaster.EventHoldingRightMouseDown -= ToggleMoveCamera;
            gamemaster.OnAllySwitch -= OnAllySwitch;
            gamemaster.EventEnableCameraZoom -= ToggleCameraZoom;
        }

        protected virtual void Update()
        {
            if (moveCamera)
            {
                HandleRotationMovement();
            }
            if (bZoomCamera)
            {
                ZoomCamera();
            }
        }
        #endregion

        #region ZoomFunctionality
        void ZoomCamera()
        {
            // Not Regarding Zoom Positive, Only Position Related
            //m_Pivot - Positive = Higher, Negative = Lower
            //m_Cam - Negative = Further, Positive = Closer
            //Zoom Out = Move Higher and Further Out
            // m_Pivot: Positive, m_Cam: Negative
            //Zoom In = Move Lower and Closer
            // m_Pivot: Negative, m_Cam: Positive

            //Set Pivot Y Position
            m_Pivot.transform.localPosition = new Vector3(
                m_Pivot.transform.localPosition.x,
                Mathf.Lerp(m_Pivot.localPosition.y,
                Mathf.Clamp(m_Pivot.localPosition.y + zoomPivot, zoomPivotMin, zoomPivotMax),
                Time.deltaTime * zoomSmoothing
                ),
                m_Pivot.transform.localPosition.z);
            //Set Camera Z Position
            m_Cam.transform.localPosition = new Vector3(
                m_Cam.transform.localPosition.x,
                m_Cam.transform.localPosition.y,
                Mathf.Lerp(
                    m_Cam.transform.localPosition.z,
                    Mathf.Clamp(m_Cam.transform.localPosition.z + zoomCam, zoomCameraMin, zoomCameraMax),
                    Time.deltaTime * zoomSmoothing
                )
            );
            //Set Camera X Rotation
            m_Cam.transform.localEulerAngles = new Vector3(
                Mathf.Lerp(
                    m_Cam.transform.localEulerAngles.x,
                    Mathf.Clamp(m_Cam.transform.localEulerAngles.x + zoomRotateCam, zoomCameraMinRotation, zoomCameraMaxRotation),
                    Time.deltaTime * zoomSmoothing
                ),
                m_Cam.transform.localEulerAngles.y,
                m_Cam.transform.localEulerAngles.z
                );
        }
        #endregion

        #region Handlers
        void ToggleMoveCamera(bool enable)
        {
            moveCamera = enable;
        }

        void OnAllySwitch(PartyManager _party, AllyMember _toSet, AllyMember _current)
        {
            if (_party.bIsCurrentPlayerCommander && _toSet != null)
            {
                SetTarget(_toSet.transform);
            }
        }

        /// <summary>
        /// Positive: Scroll In, Negative: Scroll Out
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="isPositive"></param>
        void ToggleCameraZoom(bool enable, bool isPositive)
        {
            bZoomCamera = enable;
            bZoomIsPostive = isPositive;
        }
        #endregion

        #region BoilerPlateCode

        // This script is designed to be placed on the root object of a camera rig,
        // comprising 3 gameobjects, each parented to the next:

        // 	Camera Rig
        // 		Pivot
        // 			Camera

        [SerializeField] private float m_MoveSpeed = 1f;                      // How fast the rig will move to keep up with the target's position.
        [Range(0f, 10f)] [SerializeField] private float m_TurnSpeed = 1.5f;   // How fast the rig will rotate from user input.
        [SerializeField] private float m_TurnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
        [SerializeField] private float m_TiltMax = 75f;                       // The maximum value of the x axis rotation of the pivot.
        [SerializeField] private float m_TiltMin = 45f;                       // The minimum value of the x axis rotation of the pivot.
        [SerializeField] private bool m_LockCursor = false;                   // Whether the cursor should be hidden and locked.
        [SerializeField] private bool m_VerticalAutoReturn = false;           // set wether or not the vertical axis should auto return

        private float m_LookAngle;                    // The rig's y axis rotation.
        private float m_TiltAngle;                    // The pivot's x axis rotation.
        private const float k_LookDistance = 100f;    // How far in front of the pivot the character's look target is.
        private Vector3 m_PivotEulers;
        private Quaternion m_PivotTargetRot;
        private Quaternion m_TransformTargetRot;

        protected override void Awake()
        {
            base.Awake();
            // Lock or unlock the cursor.
            //Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            //Cursor.visible = !m_LockCursor;
            m_PivotEulers = m_Pivot.rotation.eulerAngles;

            m_PivotTargetRot = m_Pivot.transform.localRotation;
            m_TransformTargetRot = transform.localRotation;
        }

        protected override void FollowTarget(float deltaTime)
        {
            if (m_Target == null) return;
            // Move the rig towards target position.
            transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime * m_MoveSpeed);
        }


        protected virtual void HandleRotationMovement()
        {
            if (Time.timeScale < float.Epsilon)
                return;

            // Read the user input
            var x = CrossPlatformInputManager.GetAxis("Mouse X");
            var y = CrossPlatformInputManager.GetAxis("Mouse Y");

            // Adjust the look angle by an amount proportional to the turn speed and horizontal input.
            m_LookAngle += x * m_TurnSpeed;

            // Rotate the rig (the root object) around Y axis only:
            m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);

            if (m_VerticalAutoReturn)
            {
                // For tilt input, we need to behave differently depending on whether we're using mouse or touch input:
                // on mobile, vertical input is directly mapped to tilt value, so it springs back automatically when the look input is released
                // we have to test whether above or below zero because we want to auto-return to zero even if min and max are not symmetrical.
                m_TiltAngle = y > 0 ? Mathf.Lerp(0, -m_TiltMin, y) : Mathf.Lerp(0, m_TiltMax, -y);
            }
            else
            {
                // on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
                m_TiltAngle -= y * m_TurnSpeed;
                // and make sure the new value is within the tilt range
                m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);
            }

            // Tilt input around X is applied to the pivot (the child of this object)
            m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, m_PivotEulers.y, m_PivotEulers.z);

            if (m_TurnSmoothing > 0)
            {
                m_Pivot.localRotation = Quaternion.Slerp(m_Pivot.localRotation, m_PivotTargetRot, m_TurnSmoothing * Time.deltaTime);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TransformTargetRot, m_TurnSmoothing * Time.deltaTime);
            }
            else
            {
                m_Pivot.localRotation = m_PivotTargetRot;
                transform.localRotation = m_TransformTargetRot;
            }
        }

        #endregion
    }

}
#endif