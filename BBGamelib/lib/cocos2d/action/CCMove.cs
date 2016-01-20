using UnityEngine;
using System.Collections;

namespace BBGamelib{
	//
	// MoveBy
	//
	#region mark - MoveBy
	public class CCMoveBy : CCActionInterval
	{
		protected Vector2 _positionDelta;
		protected Vector2 _startPos;
		protected Vector2 _previousPos;

		public CCMoveBy(float t, Vector2 p){
			initWithDuration (t, p);	
		}

		public virtual void initWithDuration(float t, Vector2 p){
			base.initWithDuration (t);
			_positionDelta = p;
		}

		protected override CCAction copyImpl ()
		{
			CCMoveBy act = new CCMoveBy (this.duration, _positionDelta);
			return act;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_previousPos = _startPos = ((CCNode)target).position;
		}

		protected override CCAction reverseImpl ()
		{
			CCMoveBy act = new CCMoveBy (_duration, _positionDelta * -1);
			return act;
		}

		public override void update (float t)
		{
			CCNode node = (CCNode)_target;		
			bool stackable = ccConfig.CC_ENABLE_STACKABLE_ACTIONS;
			if(stackable){
				Vector2 currentPos = node.position;
				Vector2 diff = currentPos - _previousPos;
				_startPos = _startPos + diff;
				Vector2 newPos = _startPos + _positionDelta * t;
				node.position = newPos;
				_previousPos = newPos;
			} else {
				node.position = _startPos + _positionDelta * t;
			}
		}
	}
	#endregion

	//
	// MoveTo
	//
	#region mark MoveTo
	public class CCMoveTo : CCMoveBy
	{
		Vector2 _endPosition;

		public CCMoveTo(float t, Vector2 p):base(t,p){
		}
		
		public override void initWithDuration(float t, Vector2 p){
			base.initWithDuration (t);
			_endPosition = p;
		}
		
		protected override CCAction copyImpl ()
		{
			CCMoveTo act = new CCMoveTo (this.duration, _endPosition);
			return act;
		}
		
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_positionDelta = _endPosition - ((CCNode)_target).position;
		}

		protected override CCAction reverseImpl ()
		{
			CCMoveTo act = new CCMoveTo (_duration, _positionDelta * -1);
			return act;
		}
	}
	#endregion
}
