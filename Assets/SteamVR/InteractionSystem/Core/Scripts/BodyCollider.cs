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
            Vector3 headLocalPosition = head.position - player.transform.position;
            float distanceFromFloor = Vector3.Dot(headLocalPosition, Vector3.up);
            //float distanceFromFloor = Vector3.Dot( head.localPosition, Vector3.up );
            capsuleCollider.height = Mathf.Max( capsuleCollider.radius, distanceFromFloor );
			transform.position = head.position - 0.5f * distanceFromFloor * Vector3.up;
		}
	}
}
