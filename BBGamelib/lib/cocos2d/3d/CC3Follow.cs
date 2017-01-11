using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** CCFollow is an action that "follows" a node.

	 Eg:
		[layer runAction: [CCFollow actionWithTarget:hero]];

	 Instead of using CCCamera as a "follower", use this action instead.
	 @since v0.99.2
	 */
	public class CC3Follow : CCAction
	{
		/* node to follow */
		CC3Node	_followedNode;


		/** creates the action with no boundary set */
		public CC3Follow(CC3Node fNode){
			initWithTarget (fNode);
		}
		/** initializes the action */
		public void initWithTarget(CC3Node fNode){
			base.init ();
			_followedNode = fNode;
		}

		protected override CCAction copyImpl ()
		{
			CC3Follow copy = MemberwiseClone () as CC3Follow;
			copy.tag = _tag;
			return copy;
		}

		public override void step (float dt)
		{
			((CC3Node)_target).position3D = _followedNode.position3D;
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
