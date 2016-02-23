using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public abstract class CCAppDelegate3D : CCAppDelegate
	{
		[SerializeField] protected CCSprite3DFactory _fbxPool;	
		
		public override void applicationRunOnceOnBuilding ()
		{
			base.applicationRunOnceOnBuilding ();
			//fbx
			{
				GameObject obj = new GameObject ();
				obj.transform.parent = this.transform;
				_fbxPool = obj.AddComponent<CCSprite3DFactory> ();
			}
		}
	}
}

