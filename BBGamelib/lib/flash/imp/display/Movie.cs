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
        protected utList<Display> _caches;

		public Movie(TagDefineMovie define){
			_define = define;
			_curFrame = null;
			_isBoundsDirty = true;
			_tweenMode = kTweenMode.SkipNoLabelFrames;
			_depthDisplays = new Display[define.maxDepth];
            _movieCtrl = new MovieCtrl (this);
            _caches = new utList<Display>();
            if (string.IsNullOrEmpty(define.className))
            {
                this.nameInHierarchy = define.characterId.ToString();
            } else
            {
                this.nameInHierarchy = define.className;
            }
            this.cascadeColorEnabled = true;
            this.cascadeOpacityEnabled = true;
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
        public utList<Display> caches{get{return _caches;}}

        /** Do not call this method.*/
        public virtual void gotoFrame(int frameIndex, bool isCheckedPreTags){
            if (_curFrame == null || _curFrame.frameIndex != frameIndex)
            {
                _curFrame = _define.frames [frameIndex];

                if (isCheckedPreTags)
                {
                    for (int i = 1; i <= _depthDisplays.Length; i++)
                    {
                        applyPreKeyTags(frameIndex, i);
                    }
                } else
                {
                    for (int i = 0; i < _curFrame.objs.Length; i++)
                    {
                        FrameObject frameObj = _curFrame.objs [i];
                        ITag iTag = getTag(frameObj);
                        if (iTag is IDisplayListTag) {
                            (iTag as IDisplayListTag).apply(this, frameObj);
                        }
                    }
                }
                _isBoundsDirty = true;
            }
		}

        void applyPreKeyTags(int fromFrameIndex, int depth){
            utList<FrameObject> preTags = new utList<FrameObject>();
            bool removed = false, hasCharacter = false;
            int i = fromFrameIndex;
            for (; i >= 0; i--)
            {
                Frame frame = _define.frames [i];
                for (int k = 0; k < frame.objs.Length; k++)
                {
                    FrameObject frameObj = frame.objs [k];
                    ITag iTag = getTag(frameObj);
                    if (iTag is TagRemoveObject)
                    {
                        TagRemoveObject tagRemoveObj = iTag as TagRemoveObject;
                        if (tagRemoveObj.depth == depth)
                        {
                            preTags.DL_APPEND(frameObj);
                            removed = true;
                            break;
                        }
                    } else if (iTag is TagPlaceObject)
                    {
                        TagPlaceObject tagPlaceObj = iTag as TagPlaceObject;
                        if (tagPlaceObj.depth == depth)
                        {
                            preTags.DL_APPEND(frameObj);
                            hasCharacter |= tagPlaceObj.hasCharacter;
                            if (hasCharacter)
                            {
                                break;
                            }
                        }
                    }
                }

                if (removed || (hasCharacter))
                {
                    break;
                }
            }
            if (preTags.head != null)
            {             
                for (var ent = preTags.head.prev; ent != preTags.head; ent = ent.prev)
                {
                    FrameObject frameObj = ent.obj;
                    IDisplayListTag iTag = getTag(frameObj) as IDisplayListTag;
                    iTag.apply(this, frameObj);
                }
                {
                    FrameObject frameObj = preTags.head.obj;
                    IDisplayListTag iTag = getTag(frameObj) as IDisplayListTag;
                    iTag.apply(this, frameObj);
                }
            } else
            {
                Display display = _depthDisplays [depth-1];
                if (display != null) {

                    //cache
                    display.removed = true;
                    display.visible = false;
                }
            }
        }
        ITag getTag(FrameObject frameObj){
            ITag iTag = null;
            if(frameObj.lastModfiedAtIndex != 0){
                iTag = _define.tags[frameObj.lastModfiedAtIndex];
            }else{
                iTag = _define.tags[frameObj.placedAtIndex];
            }
            return iTag;
        }

        public override bool removed
        {
            get
            {
                return base.removed;
            }
            set
            {
                base.removed = value;
                if (value)
                {
                    _movieCtrl.pause();
                } else
                {
                    _movieCtrl.resume();
                }
            }
        }
	}
}
