using UnityEngine;
using System.Collections;

namespace BBGamelib.flash.imp
{
	public interface IDisplayListTag : ITag
	{
//		void applyFrameObj (Movie movie, Frame frame, FrameObject frameObj);
        void apply (Movie movie, FrameObject frameObj);
	}
}


