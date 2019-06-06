//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Collider dangling from the player's head
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( CapsuleCollider ) )]
	public class BodyCollider : MonoBehaviour
	{
		public Transform head;

		private CapsuleCollider capsuleCollider;

		//-------------------------------------------------
		void Awake()
		{
			capsuleCollider = GetComponent<CapsuleCollider>();
		}


		//-------------------------------------------------
		void FixedUpdate()
		{
            Player player = Player.instance;
            Vector3 upDirection = player.transform.up;
            Vector3 headLocalPosition = head.position - player.transform.position;
            float distanceFromFloor = Vector3.Dot(headLocalPosition, upDirection);
            capsuleCollider.height = Mathf.Max( capsuleCollider.radius, distanceFromFloor );
			transform.position = head.position - (0.5f * distanceFromFloor * upDirection);
		}
	}
}
