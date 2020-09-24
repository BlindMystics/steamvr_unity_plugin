//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Controls for the non-VR debug camera
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Camera ) )]
	public class FallbackCameraController : MonoBehaviour
	{
		public float speed = 4.0f;
		public float shiftSpeedMultiplier = 4.0f;
        public float ctrlSpeedMultiplier = 0.2f;
        public bool showInstructions = true;
		public float lookSpeedMultiplierHorizontal = 0.25f;
		public float lookSpeedMultiplierVertical = 0.25f;
		public float verticalLimitOffset = 10.0f; //Degrees from -90 -> 90.

		[Header("Axes")]
		[SerializeField] private bool upDownControlEnabled = false;

		private Vector3 startEulerAngles;
		private Vector3 startMousePosition;
		private float realTime;
		private bool lookLockedToMouse = false;

		//-------------------------------------------------
		protected virtual void OnEnable()
		{
			realTime = Time.realtimeSinceStartup;
		}


		//-------------------------------------------------
		protected virtual void Update()
		{
            if (PointingInputModule.Instance.InputLock) {
                return;
            }

			float forward = 0.0f;
			if ( Input.GetKey( KeyCode.W ) || Input.GetKey( KeyCode.UpArrow ) )
			{
				forward += 1.0f;
			}
			if ( Input.GetKey( KeyCode.S ) || Input.GetKey( KeyCode.DownArrow ) )
			{
				forward -= 1.0f;
			}

            float up = 0.0f;
			if (upDownControlEnabled) {
				if (Input.GetKey(KeyCode.E))
				{
					up += 1.0f;
				}
				if (Input.GetKey(KeyCode.Q))
				{
					up -= 1.0f;
				}
			}

            float right = 0.0f;
			if ( Input.GetKey( KeyCode.D ) || Input.GetKey( KeyCode.RightArrow ) )
			{
				right += 1.0f;
			}
			if ( Input.GetKey( KeyCode.A ) || Input.GetKey( KeyCode.LeftArrow ) )
			{
				right -= 1.0f;
			}

			float currentSpeed = speed;
			if ( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) )
			{
				currentSpeed *= shiftSpeedMultiplier;
			}
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                currentSpeed *= ctrlSpeedMultiplier;
            }

            float realTimeNow = Time.realtimeSinceStartup;
			float deltaRealTime = realTimeNow - realTime;
			realTime = realTimeNow;

			Vector3 delta = new Vector3( right, up, forward ) * currentSpeed * deltaRealTime;

			transform.position += transform.TransformDirection( delta );

			Vector3 mousePosition = Input.mousePosition;

			if (Input.GetKeyDown(KeyCode.RightAlt)) {
				lookLockedToMouse = !lookLockedToMouse;
			}

			bool updateLookRotation = lookLockedToMouse || 
				Input.GetMouseButton(1); /* right mouse */

			if (updateLookRotation) {
				Cursor.lockState = CursorLockMode.Locked;
				Vector3 currentForward = transform.localRotation * Vector3.forward;
				Vector2 xzComponent = new Vector2(currentForward.x, currentForward.z);
				float phi = -(Mathf.Atan2(currentForward.y, xzComponent.magnitude) * Mathf.Rad2Deg);
				float theta = -((Mathf.Atan2(currentForward.z, currentForward.x) * Mathf.Rad2Deg) - 90.0f);

				float horizontal = Input.GetAxis("Camera Horizontal") * lookSpeedMultiplierHorizontal * (1000.0f / Screen.width);
				float vertical = Input.GetAxis("Camera Vertical") * lookSpeedMultiplierVertical * (1000.0f / Screen.height);

				phi -= vertical;
				theta += horizontal;

				phi = Mathf.Clamp(phi, -90.0f + verticalLimitOffset, 90.0f - verticalLimitOffset);

				Quaternion newVertical = Quaternion.Euler(phi, 0f, 0f);
				Quaternion newHorizontal = Quaternion.Euler(0f, theta, 0f);
				transform.localRotation = newHorizontal * newVertical;
			} else {
				Cursor.lockState = CursorLockMode.None;
			}

		}


		//-------------------------------------------------
		void OnGUI()
		{
			if ( showInstructions )
			{
				GUI.Label( new Rect( 10.0f, 10.0f, 600.0f, 400.0f ),
					"WASD EQ/Arrow Keys to translate the camera\n" +
					"Right mouse click to rotate the camera\n" +
					"Left mouse click for standard interactions.\n" );
			}
		}
	}
}
