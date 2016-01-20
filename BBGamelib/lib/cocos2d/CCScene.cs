using UnityEngine;
using System.Collections;

namespace BBGamelib{
	[AddComponentMenu("")]
	public class CCScene : CCNode
	{
		protected override void init ()
		{
			base.init ();
			
			this.ignoreAnchorPointForPosition = true;
			_anchorPoint = new Vector2 (0.5f, 0.5f);
			this.contentSize = CCDirector.sharedDirector.winSize;
		}
    }
}

