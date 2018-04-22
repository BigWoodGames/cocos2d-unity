using UnityEngine;
using System.Collections;

namespace BBGamelib{
	//
	// MoveBy
	//
	#region mark - MoveBy
	public class CC3MoveBy : CCActionInterval
	{
		public const byte XMASK = 1 << 0;
		public const byte YMASK = 1 << 1;
		public const byte ZMASK = 1 << 2;

		protected Vector3 _positionDelta;
		protected Vector3 _startPos;
		protected Vector3 _previousPos;
		protected byte _masks;
		
		public CC3MoveBy(float t, Vector3 p):this(t,p,XMASK|YMASK|ZMASK)
        {
		}

        //XMASK = 1<<0, YMASK=1<<1, ZMASK=1<<2
		public CC3MoveBy(float t, Vector3 p, byte masks)
        {
			initWithDuration (t, p, masks);	
		}


		protected virtual void initWithDuration(float t, Vector3 p, byte masks=XMASK|YMASK|ZMASK)
        {
			base.initWithDuration (t);
			_positionDelta = p;
			_masks = masks;
		}
		
		protected override CCAction copyImpl ()
		{
			CC3MoveBy act = new CC3MoveBy (this.duration, _positionDelta, _masks);
			return act;
		}
		
		public override void startWithTarget (object aTarget)
		{
			NSUtils.Assert (aTarget is CC3Node, "CC3Move only supports with CC3Node, and target is {0}.", aTarget);
			base.startWithTarget (aTarget);
			_previousPos = _startPos = ((CC3Node)target).position3D;
		}
		
		protected override CCAction reverseImpl ()
		{
			CC3MoveBy act = new CC3MoveBy (_duration, _positionDelta * -1, _masks);
			return act;
		}
		
		public override void update (float t)
		{
			CC3Node node = (CC3Node)_target;		
			bool stackable = ccConfig.CC_ENABLE_STACKABLE_ACTIONS;
			Vector3 toPos = node.position3D;
			if(stackable){
				Vector3 currentPos = node.position3D;
				Vector3 diff = currentPos - _previousPos;
				_startPos = _startPos + diff;
				Vector3 newPos = _startPos + _positionDelta * t;
				toPos = newPos;
				_previousPos = newPos;
			} else {
				toPos = _startPos + _positionDelta * t;
			}
			if ((_masks & XMASK) != 0) {
				node.positionX = toPos.x;
			}
			if ((_masks & YMASK) != 0) {
				node.positionY = toPos.y;
			}
			if ((_masks & ZMASK) != 0) {
				node.positionZ = toPos.z;
			}
		}
	}
	#endregion

	
	//
	// MoveTo
	//
	#region mark MoveTo
	public class CC3MoveTo : CC3MoveBy
	{
		Vector3 _endPosition;
		
		public CC3MoveTo(float t, Vector3 p):base(t,p){
		}

		public CC3MoveTo(float t, Vector3 p, byte masks):base(t,p,masks){
		}

		protected override void initWithDuration(float t, Vector3 p, byte masks=XMASK|YMASK|ZMASK){
			base.initWithDuration (t);
			_endPosition = p;
			_masks = masks;
		}
		
		protected override CCAction copyImpl ()
		{
			CC3MoveTo act = new CC3MoveTo (this.duration, _endPosition, _masks);
			return act;
		}
		
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_positionDelta = _endPosition - ((CC3Node)_target).position3D;
		}
		
		protected override CCAction reverseImpl ()
		{
			CC3MoveTo act = new CC3MoveTo (_duration, _positionDelta * -1, _masks);
			return act;
		}
	}
	#endregion
}
