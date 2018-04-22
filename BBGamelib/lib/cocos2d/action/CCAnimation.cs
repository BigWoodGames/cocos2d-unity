using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;

namespace BBGamelib{
	#region mark - CCAnimationFrame

	/** CCAnimationFrame
	 A frame of the animation. It contains information like:
		- sprite frame name
		- # of delay units.
		- offset
	 
	 @since v2.0
	 */
	public class CCAnimationFrame{
		CCSpriteFrame _spriteFrame;
		float _delayUnits;
		NSDictionary _userinfo;

		/// -----------------------------------------------------------------------
		/// @name Accessing the Animation Frame Attributes
		/// -----------------------------------------------------------------------
		
		/** CCSpriteFrame to be used. */
		public CCSpriteFrame spriteFrame{set{_spriteFrame=value;} get{return _spriteFrame;}}
		
		/** Number of time units to display this frame. */
		public float delayUnits{set{_delayUnits = value;} get{return _delayUnits;}}
		
		/** Custom dictionary. */
		public NSDictionary userInfo{set{_userinfo=value;} get{return _userinfo;}}

		
		/// -----------------------------------------------------------------------
		/// @name Initializing a CCAnimationFrame Object
		/// -----------------------------------------------------------------------
		
		/**
		 *  Initializes and returns an Animation Frame object using the specified frame name, delay units and user info values.
		 *
		 *  @param spriteFrame Sprite Frame.
		 *  @param delayUnits  Delay time units.
		 *  @param userInfo    Custom dictionary.
		 *
		 *  @return An initialized CCAnimationFrame Object.
		 */
		public CCAnimationFrame(CCSpriteFrame spriteFrame, float delayUnits, NSDictionary userInfo){
			this.spriteFrame = spriteFrame;
			this.delayUnits = delayUnits;
			this.userInfo = userInfo;
		}

		public CCAnimationFrame copy(){
			CCAnimationFrame action = new CCAnimationFrame(_spriteFrame, _delayUnits, _userinfo==null?null:_userinfo.Clone() as NSDictionary);
			return action;
		}

		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | SpriteFrame = {2}, delayUnits = {3} >", GetType().Name, GetHashCode(), _spriteFrame, _delayUnits);
		}
	}
	#endregion
	
	
	#region mark - CCAnimation
	/** A CCAnimation object is used to perform animations on the CCSprite objects.

	 The CCAnimation object contains CCAnimationFrame objects, and a possible delay between the frames.
	 You can animate a CCAnimation object by using the CCAnimate action. Example:

	  [sprite runAction:[CCAnimate actionWithAnimation:animation]];

	 */
	public class CCAnimation : NSCopying<CCAnimation>
	{
		// Array of CCSpriteFrame.
		List<CCAnimationFrame> _frames;
		
		// Total delay units.
		float			_totalDelayUnits;
		
		// Delay in seconds of the per frame delay unit.
		float			_delayPerUnit;
		
		// True to restore original frame when animation complete.
		bool			_restoreOriginalFrame;
		
		// Number of times to loop animation.
		uint		_loops;

		/// -----------------------------------------------------------------------
		/// @name Accessing the Animation Attributes
		/// -----------------------------------------------------------------------
		
		/** Total Delay units. */
		public float totalDelayUnits{get{return _totalDelayUnits;}}
		
		/** Delay in seconds of the per frame delay unit. */
		public float delayPerUnit{set{_delayPerUnit = value;} get{return _delayPerUnit;}}
		
		/** Duration in seconds of the whole animation. */
		public float duration{
			get{
				return _totalDelayUnits * _delayPerUnit;
			}
		}
		
		/** Array of CCAnimationFrames. */
		public List<CCAnimationFrame> frames{set{_frames = value;} get{return _frames;}}
		
		/** True to restore original frame when animation complete. */
		public bool restoreOriginalFrame{set{_restoreOriginalFrame = value;} get{return _restoreOriginalFrame;}}
		
		/** Number of times to loop animation. */
		public uint loops{set{_loops = value;} get{return _loops;}}

		/** Creates an animation
		 @since v0.99.5
		 */
		public CCAnimation(){
			init ();
		}

		/** Creates an animation with an array of CCSpriteFrame.
		 The frames will be created with one "delay unit".
		 @since v0.99.5
		 */
		public CCAnimation(List<CCSpriteFrame> frames){
			initWithSpriteFrames (frames);
		}
		
		/* Creates an animation with an array of CCSpriteFrame and a delay between frames in seconds.
		 The frames will be added with one "delay unit".
		 @since v0.99.5
		 */
		public CCAnimation(List<CCSpriteFrame> frames, float delay){
			initWithSpriteFrames (frames, delay);
		}
		/* Creates an animation with an array of CCAnimationFrame, the delay per units in seconds and and how many times it should be executed.
		 @since v2.0
		 */
		public CCAnimation(List<CCAnimationFrame> frames, float delayPerUnit, uint loops){
			initWithAnimationFrames (frames, delayPerUnit, loops);
		}

		public void init(){
			initWithSpriteFrames (null, 0);
		}

		/** Initializes a CCAnimation with an array of CCSpriteFrame.
		 The frames will be added with one "delay unit".
		 @since v0.99.5
		*/
		public void initWithSpriteFrames(List<CCSpriteFrame> frames){
			initWithSpriteFrames (frames, 0);
		}

		
		/** Initializes a CCAnimation with an array of CCSpriteFrames and a delay between frames in seconds.
		 The frames will be added with one "delay unit".
		 @since v0.99.5
		 */
		public void initWithSpriteFrames(List<CCSpriteFrame> array, float delay){
			_loops = 1;
			_delayPerUnit = delay;

			this.frames = new List<CCAnimationFrame> (array==null?0:array.Count);

			if (array != null) {
				var enumerator = array.GetEnumerator();
				while (enumerator.MoveNext()) {
					var frame = enumerator.Current;
					CCAnimationFrame animFrame = new CCAnimationFrame(frame, 1, null);
					this.frames.Add (animFrame);		
					_totalDelayUnits++;
				}
			}
		}

		
		/* Initializes an animation with an array of CCAnimationFrame and the delay per units in seconds.
		 @since v2.0
		 */
		public void initWithAnimationFrames(List<CCAnimationFrame> array, float delay, uint loops ){
			_loops = loops;
			_delayPerUnit = delay;
			
			this.frames = array==null?new List<CCAnimationFrame>():new List<CCAnimationFrame> (array);
			if (array != null) {
				var enumerator = array.GetEnumerator();
				while (enumerator.MoveNext()) {
					var animeFrame = enumerator.Current;
					_totalDelayUnits += animeFrame.delayUnits;
				}
			}
		}

		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | frames={2}, totalDelayUnits={3}, delayPerUnit={4}, loops={5}>", GetType(), GetHashCode(),
			                      _frames.Count,
			                      _totalDelayUnits,
			                      _delayPerUnit,
			                      _loops);
		}


		public CCAnimation copy(){
			CCAnimation animation = new CCAnimation (_frames, _delayPerUnit, _loops);
			animation.restoreOriginalFrame = _restoreOriginalFrame;
			return animation;	
		}
		
		/** Adds a CCSpriteFrame to a CCAnimation.
		 The frame will be added with one "delay unit".
		*/
		public void addSpriteFrame(CCSpriteFrame frame){
			CCAnimationFrame animFrame = new CCAnimationFrame(frame, 1, null);
			_frames.Add (animFrame);
		
			_totalDelayUnits ++;
		}
		/** Adds a frame with an image filename. Internally it will create a CCSpriteFrame and it will add it.
		 The frame will be added with one "delay unit".
		 Added to facilitate the migration from v0.8 to v0.9.
		 */
		public void addSpriteFrameWithFilename(string file){
			string ext = Path.GetExtension (file);
			if(ext!=null && ext.Length>0)
				file = file.Replace (ext, "");
            Texture2D texture = Resources.Load<Texture2D> (file);
            CCSpriteFrame frame = new CCSpriteFrame (texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)));
			addSpriteFrame (frame);
		}
	}
	#endregion
}
