using UnityEngine;
using System.Collections;
using System;

public class UITouch
{
	public const int SINGLE_TOUCH_ID = int.MaxValue;
	public int fingerId;
	public DateTime timestamp;
	public TouchPhase phase;
	public int tapCount;
	public Vector2 location;

	public override bool Equals (object obj)
	{
		if (obj == null)
			return false;
		return fingerId.Equals (((UITouch)obj).fingerId);
	}

	public override int GetHashCode ()
	{
		return fingerId.GetHashCode ();
	}

	public static bool operator ==(UITouch lhs, UITouch rhs)
	{
		// If left hand side is null...
		if (System.Object.ReferenceEquals(lhs, null))
		{
			// ...and right hand side is null...
			if (System.Object.ReferenceEquals(rhs, null))
			{
				//...both are null and are Equal.
				return true;
			}
			
			// ...right hand side is not null, therefore not Equal.
			return false;
		}
		
		// Return true if the fields match:
		return lhs.Equals(rhs);
	}
	
	public static bool operator !=(UITouch lhs, UITouch rhs)
	{
		return !(lhs == rhs);
	}
}

