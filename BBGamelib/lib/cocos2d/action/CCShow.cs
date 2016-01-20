using UnityEngine;
using System.Collections;

namespace BBGamelib{
	
	//
	// Show
	//
	#region mark CCShow
	public class CCShow : CCActionInstant
	{
		public override void update (float dt)
		{
			(_target as CCNode).visible = true;
		}

		protected override CCAction reverseImpl ()
		{
			return new CCHide();
		}

		protected override CCAction copyImpl ()
		{
			return new CCShow ();
		}
	}
	#endregion
	
	//
	// Hide
	//
	#region mark CCHide
	public class CCHide : CCActionInstant
	{
		public override void update (float dt)
		{
			(_target as CCNode).visible = false;
		}
		
		protected override CCAction reverseImpl ()
		{
			return new CCShow();
		}
		protected override CCAction copyImpl ()
		{
			return new CCHide ();
		}
	}
	#endregion
	
	//
	// ToggleVisibility
	//
	#region mark CCToggleVisibility
	public class CCToggleVisibility : CCActionInstant
	{
		public override void update (float dt)
		{
			(_target as CCNode).visible = !(_target as CCNode).visible;
		}

		protected override CCAction copyImpl ()
		{
			return new CCToggleVisibility ();
		}
	}
	#endregion
}
