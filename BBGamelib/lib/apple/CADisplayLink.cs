using UnityEngine;
using System.Collections;
using System;

namespace BBGamelib{
	public interface CADisplayLink
	{
		/* regist object for the display link. It will
 		* invoke the method called 'sel' on 'target', the method has the
		* signature '(void)selector:(CADisplayLink *)sender'. */
		void registWithTarget (System.Object target, Action<CADisplayLink> selOnUpdate, Action<CADisplayLink> selOnGUI);

		/* Removes the object from all runloop modes */
		void invalidate(System.Object target);
	}
}
