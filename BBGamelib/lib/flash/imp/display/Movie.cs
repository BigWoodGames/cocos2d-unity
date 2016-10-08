using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


namespace BBGamelib.flash.imp
{	
	public enum kTweenMode{
		SkipFrames,
		SkipNoLabelFrames,
		TweenFrames,
	}
	public class Movie : Display
	{
		// ------------------------------------------------------------------------------
		//  ctor
		// ------------------------------------------------------------------------------
		protected TagDefineMovie _define;
		protected Rect _bounds;
		protected bool _isBoundsDirty;
		protected Frame _curFrame;
		protected kTweenMode _tweenMode;
		protected Display[] _depthDisplays;
		protected MovieCtrl _movieCtrl;

		public Movie(TagDefineMovie define){
			_define = define;
			_curFrame = null;
			_isBoundsDirty = true;
			_tweenMode = kTweenMode.SkipNoLabelFrames;
			_depthDisplays = new Display[define.maxDepth];
			_movieCtrl = new MovieCtrl (this);
		}

		
		// ------------------------------------------------------------------------------
		//  override
		// ------------------------------------------------------------------------------
		public override TagDefineDisplay define{ get { return _define; } }
		public override Rect getBounds(){ 
			if(_isBoundsDirty){
				Rect bounds = new Rect(0, 0, 0, 0);
				for(int i=0; i<_depthDisplays.Length; i++){
					Display child = _depthDisplays[i];
					if(child != null && child.visible){
						Rect childBounds = child.getBounds();
						
						//CHANGE CHILD BOUNDS TO PARENT
						childBounds.position += child.anchorPointInPixels;
						CGAffineTransform childTransform = child.nodeToParentTransform();
						childBounds = CGAffineTransform.CGRectApplyAffineTransform(childBounds, childTransform);
						//childBounds.position += child.view.position;
						
						bounds = ccUtils.RectUnion(bounds, childBounds);
					}
				}
				_bounds = bounds;
				_isBoundsDirty = false;
			}
			return _bounds;
		}	

		
		// ------------------------------------------------------------------------------
		//  public
		// ------------------------------------------------------------------------------
		public TagDefineMovie movieDefine{ get { return _define; } }
		public int curFrame{ get{return _curFrame==null?-1:_curFrame.frameIndex;}}
		public string curLabel{get{return _curFrame.label;}}
		public Display[] depthDisplays{ get { return _depthDisplays; } }
		public MovieCtrl movieCtrl{ get { return _movieCtrl; } }
		public kTweenMode tweenMode{get{return _tweenMode;} set{_tweenMode=value;}}


		public virtual void skipFrame(int frameIndex){
			if (_curFrame.frameIndex != frameIndex) {
				_curFrame = _define.frames[frameIndex];
				for(int i=0; i<_depthDisplays.Length; i++){
					Display display = _depthDisplays[i];
					if(display != null)
						display.visible = false;
				}
			}
		}
		public virtual void gotoFrame(int frameIndex){
			if (_curFrame ==null || _curFrame.frameIndex != frameIndex) {
				_curFrame = _define.frames[frameIndex];

				for(int i=0; i<_curFrame.objs.Length; i++){
					FrameObject fobj = _curFrame.objs[i];
					applyFrameObj(_curFrame, fobj);
				}
				_isBoundsDirty = true;
			}
		}
		public void applyFrameObj(Frame frame, FrameObject frameObj){
			ITag iTag = null;
			if(frameObj.lastModfiedAtIndex != 0){
				iTag = _define.tags[frameObj.lastModfiedAtIndex];
			}else{
				iTag = _define.tags[frameObj.placedAtIndex];
			}
			if (iTag is IDisplayListTag) {
				(iTag as IDisplayListTag).applyFrameObj(this, frame, frameObj);
			}
		}
	}
}
