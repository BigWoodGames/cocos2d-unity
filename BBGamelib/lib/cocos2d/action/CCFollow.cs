using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** CCFollow is an action that "follows" a node.

	 Eg:
		[layer runAction: [CCFollow actionWithTarget:hero]];

	 Instead of using CCCamera as a "follower", use this action instead.
	 @since v0.99.2
	 */
	public class CCFollow : CCAction
	{
		/* node to follow */
		CCNode	_followedNode;
		
		/* whether camera should be limited to certain area */
		bool _boundarySet;
		
		/* if screen-size is bigger than the boundary - update not needed */
		bool _boundaryFullyCovered;
		
		/* fast access to the screen dimensions */
		Vector2 _halfScreenSize;
		Vector2 _fullScreenSize;
		
		/* world boundaries */
		float _leftBoundary;
		float _rightBoundary;
		float _topBoundary;
		float _bottomBoundary;

		/** creates the action with no boundary set */
		public CCFollow(CCNode fNode){
			initWithTarget (fNode);
		}
		
		/** creates the action with a set boundary */
		public CCFollow(CCNode fNode, Rect worldBoundary){
			initWithTarget (fNode, worldBoundary);
		}

		/** initializes the action */
		public void initWithTarget(CCNode fNode){
			base.init ();
			_followedNode = fNode;
			_boundarySet = false;
			_boundaryFullyCovered = false;
			
			Vector2 s = CCDirector.sharedDirector.winSize;
			_fullScreenSize = s;
			_halfScreenSize = _fullScreenSize * .5f;
		}
		
		/** initializes the action with a set boundary */
		public void initWithTarget(CCNode fNode, Rect rect){
			base.init ();
			_followedNode = fNode;
			_boundarySet = true;
			_boundaryFullyCovered = false;
			
			Vector2 winSize = CCDirector.sharedDirector.winSize;
			_fullScreenSize = winSize;
			_halfScreenSize = _fullScreenSize * .5f;
			
			_leftBoundary = -((rect.position.x+rect.size.x) - _fullScreenSize.x);
			_rightBoundary = -rect.position.x ;
			_topBoundary = -rect.position.y;
			_bottomBoundary = -((rect.position.y+rect.size.y) - _fullScreenSize.y);
			
			if(FloatUtils.Small(_rightBoundary , _leftBoundary))
			{
				// screen width is larger than world's boundary width
				//set both in the middle of the world
				_rightBoundary = _leftBoundary = (_leftBoundary + _rightBoundary) / 2;
			}
			if(FloatUtils.Small(_topBoundary , _bottomBoundary))
			{
				// screen width is larger than world's boundary width
				//set both in the middle of the world
				_topBoundary = _bottomBoundary = (_topBoundary + _bottomBoundary) / 2;
			}
			
			if( FloatUtils.EQ(_topBoundary, _bottomBoundary) &&  FloatUtils.EQ(_leftBoundary, _rightBoundary) )
				_boundaryFullyCovered = true;

		}

		/** alter behavior - turn on/off boundary */
		public bool boundarySet{
			get{return _boundarySet;}
			set{_boundarySet = value;}
		}
		protected override CCAction copyImpl ()
		{
			CCFollow copy = MemberwiseClone () as CCFollow;
			copy.tag = _tag;
			return copy;
		}

		public override void step (float dt)
		{
			if(_boundarySet)
			{
				// whole map fits inside a single screen, no need to modify the position - unless map boundaries are increased
				if(_boundaryFullyCovered)
					return;
				
				Vector2 tempPos =  _halfScreenSize - _followedNode.position;
				((CCNode)_target).position = new Vector2(Mathf.Clamp(tempPos.x, _leftBoundary, _rightBoundary), Mathf.Clamp(tempPos.y, _bottomBoundary, _topBoundary));
			}
			else
				((CCNode)_target).position = _halfScreenSize - _followedNode.position;
		}

		public override bool isDone ()
		{
			return !_followedNode.isRuning;
		}

		public override void stop ()
		{
			_target = null;
			base.stop ();
		}
	}
}
