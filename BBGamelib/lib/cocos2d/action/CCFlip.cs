using UnityEngine;
using System.Collections;

namespace BBGamelib{//
	/** Flips the sprite vertically
	 @since v0.99.0
	 */
	#region mark CCFlipX
	public class CCFlipX : CCActionInstant
	{
		bool _flipX;

		public CCFlipX(bool x){
			initWithFlipX (x);
		}
		void initWithFlipX(bool x){
			base.init ();
			_flipX = x;
		}

		public override void update (float dt)
		{
			((CCSprite)_target).flipX = _flipX;
		}

		protected override CCAction reverseImpl ()
		{
			return new CCFlipX(!_flipX);
		}

		protected override CCAction copyImpl ()
		{
			return new CCFlipX(_flipX);
		}
	}
	#endregion

	/** Flips the sprite vertically
	 @since v0.99.0
	 */
	#region mark CCFlipY
	public class CCFlipY : CCActionInstant
	{
		bool _flipY;
		
		public CCFlipY(bool y){
			initWithFlipY (y);
		}
		void initWithFlipY(bool y){
			base.init ();
			_flipY = y;
		}
		
		public override void update (float dt)
		{
			((CCSprite)_target).flipY = _flipY;
		}
		protected override CCAction reverseImpl ()
		{
			return new CCFlipY(!_flipY);
		}

		protected override CCAction copyImpl ()
		{
			return new CCFlipY(_flipY);
		}
	}
	#endregion
}

