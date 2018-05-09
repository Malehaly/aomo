using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSEngine
{
	public class CustomTaskButton : MonoBehaviour {

		[HideInInspector]
		public int ID;
		[HideInInspector]
		public CustomPanel Panel;

		public void LaunchTask ()
		{
			Panel.LaunchTask (ID);
		}
	}
}