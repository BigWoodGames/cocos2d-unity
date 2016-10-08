using UnityEngine;
using System.Collections;

namespace BBGamelib.flash.imp
{
	public class TagRemoveObject : IDisplayListTag
	{
		public const byte TYPE = 5;

		public readonly Flash flash;		
		public readonly int depth;

		
		public TagRemoveObject(Flash flash, byte[] data, Cursor cursor){
			this.flash = flash;
			int dataLength = Utils.ReadInt32 (data, cursor);
			int nextIndex = cursor.index + dataLength;
			depth = Utils.ReadInt32(data, cursor);
			cursor.index = nextIndex;
		}
		
		public void applyFrameObj (Movie movie, Frame frame, FrameObject frameObj){
			Display display = movie.depthDisplays [depth-1];
			if (display != null) {
				display.removeFromParent ();
				movie.depthDisplays [depth - 1] = null;
				display = null;
			}
		}
	}
}
