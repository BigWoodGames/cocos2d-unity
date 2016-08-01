using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public abstract class CC3AppDelegate : CCAppDelegate
	{
		[SerializeField] protected CC3SpriteFactory _fbxPool;	
		
		public override void applicationRunOnceOnBuilding ()
		{
			base.applicationRunOnceOnBuilding ();
			//fbx
			{
				GameObject obj = new GameObject ();
				obj.transform.parent = this.transform;
				_fbxPool = obj.AddComponent<CC3SpriteFactory> ();
			}
		}
	}
}

