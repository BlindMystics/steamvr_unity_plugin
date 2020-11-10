﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: This object won't be destroyed when a new scene is loaded
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class DontDestroyOnLoad : MonoBehaviour
	{
		//-------------------------------------------------
		void Awake()
		{
		    Debug.LogError("Please use the Blind Mystics variant of this, k thx bye.");
			DontDestroyOnLoad( this );
		}
	}
}
