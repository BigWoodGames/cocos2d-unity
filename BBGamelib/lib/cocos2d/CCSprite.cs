using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BBGamelib{
	public class CCSpriteContent
	{
		public CCFactoryGear gear;
		public GameObject gameObject{get{return gear.gameObject;}}
		public SpriteRenderer renderer{get{return gear.components[0] as SpriteRenderer;}}
		public Transform transform{get{return gameObject.transform;}}

		public CCSpriteContent(){
			gear = CCFactory.Instance.takeGear (CCFactory.KEY_SPRITE);
			gear.gameObject.name = "content";
			gear.gameObject.transform.localPosition = Vector3.zero;
			this.renderer.color = Color.white;
			this.renderer.sortingLayerName = CCFactory.LAYER_DEFAULT;
		}
	}
	public class CCSprite : CCNodeRGBA, CCSpriteProtocol
	{
		CCSpriteFrame			_spriteFrame;
		CCSpriteContent 		_content;
		
		// image is flipped
		bool	_flipX;
		bool	_flipY;

		bool _isContentDirty;

		#region CCSprite

		//empty sprite
		public CCSprite(){}

		protected override void initWithGear (CCFactoryGear gear)
		{
			base.initWithGear (gear);
			_gear.gameObject.name = "Sprite";
			_content = new CCSpriteContent ();
			_content.gameObject.transform.parent = gameObject.transform;
		}
		
		//sprite with file
		public CCSprite(string imagedName){
			initWithImageNamed (imagedName);
		}
		public void initWithImageNamed(string imageName){
			gameObject.name = imageName;
			CCSpriteFrame frame = CCSpriteFrameCache.sharedSpriteFrameCache.spriteFrameByName (imageName);
			if (frame == null) {
				CCDebug.Info("cocos2d:CCSprite: Try to load '{0}' as a file.", imageName);
				frame = new CCSpriteFrame (imageName);
			}
			initWithSpriteFrame (frame);
		}

		public CCSprite(CCSpriteFrame frame){
			initWithSpriteFrame (frame);
		}
		public void initWithSpriteFrame(CCSpriteFrame spriteFrame){	
			_flipY = _flipX = false;
			_anchorPoint =  new Vector2(0.5f, 0.5f);
			this.displayedFrame = spriteFrame;
		}

		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | Size = ({2:0.00},{3:0.00},{4:0.00},{5:0.00}) | tag = {6}>", 
			                      GetType().Name, GetHashCode(),
			                      position.x, position.y,
			                      contentSize.x, contentSize.y,
			                      _userTag);
		}

		public override void cleanup ()
		{
			base.cleanup ();
		}

		protected override void recycleGear ()
		{
			CCFactory.Instance.materialPropertyBlock.Clear ();
			_content.renderer.SetPropertyBlock (CCFactory.Instance.materialPropertyBlock);

			//reset default layer
			_content.renderer.gameObject.layer = LayerMask.NameToLayer(CCFactory.LAYER_DEFAULT);
		
			CCFactory.Instance.recycleGear (CCFactory.KEY_SPRITE, _content.gear);
			base.recycleGear ();
		}

		public CCSpriteFrame displayedFrame{
			get{ return _spriteFrame;}
			set{ 
				_spriteFrame = value;
				_content.renderer.sprite = value.sprite;
				this.contentSize = value.originalSize;
				_isContentDirty = true;
			}
		}


		public override void updateTransform ()
		{
			base.updateTransform ();
			if (_isContentDirty) {
				Vector2 contentPosition = _contentSize / 2 + _spriteFrame.offset;
				contentPosition -= _anchorPointInPixels;
				Vector2 pInUIUnits = ccUtils.PixelsToUnits (contentPosition);
				Vector3 pos = _content.transform.localPosition;
				pos.x = pInUIUnits.x;
				pos.y = pInUIUnits.y;
				_content.transform.localPosition = pos;
		
				//rotation
				Vector3 rotation = _content.transform.localEulerAngles;
				rotation.y = 0;
				rotation.z = 0;
				if (_flipX) {
					rotation.y = 180;
				} 
				if (_flipY) {
					rotation.y = _flipX ? 0 : 180;
					rotation.z = 180;
				}
				if(_spriteFrame.rotated)
					rotation.z += 90;
				_content.transform.localEulerAngles = rotation;
				_isContentDirty = false;
			}
		}

		protected override void draw ()
		{
			ccUtils.CC_INCREMENT_GL_DRAWS ();
			_content.renderer.sortingOrder = CCDirector.sharedDirector.globolRendererSortingOrder ++;
		}

		public CCSpriteContent content{
			get{ return _content;}
		}
		#endregion

		
		
		#region mark CCSprite - CCNode overrides

		public override void addChild (CCNode child, int z, string tag)
		{
			NSUtils.Assert( child != null, "Argument must be non-nil");

			//CCNode already sets _isReorderChildDirty so this needs to be after batchNode check
			base.addChild (child, z, tag);
		}

		public override void reorderChild (CCNode child, int z)
		{
			NSUtils.Assert( child != null, "Child must be non-nil");
			NSUtils.Assert( _children.Contains(child), "Child doesn't belong to Sprite" );
			
			if( z == child.zOrder )
				return;
			base.reorderChild (child, z);
		}

		#endregion

		#region CCSprite - property overloads
		public virtual bool flipX{
			get{return _flipX;}
			set{
				if(_flipX != value){
					_flipX = value;
					_isContentDirty = true;
				}
			}
		}
		public virtual bool flipY{
			get{return _flipY;}
			set{
				if(_flipY != value){
					_flipY = value;
					_isContentDirty = true;
				}
			}
		}

		public override Vector2 anchorPoint {
			set {
				base.anchorPoint = value;
				_isContentDirty = true;
			}
		}

		#endregion
		
		#region CCSprite - RGBA protocol
		public void updateColor()
		{
			Color32 tint = _displayedColor.tint;
			tint.a = _displayedOpacity.tint;
			Color32 add = _displayedColor.add;
			add.a = _displayedOpacity.add;

			ccUtils.SetRenderColor (_content.renderer, tint, add);
		}

		public override Color32 color {
			set {
				base.color = value;
				updateColor();
			}
		}

		public override void updateDisplayedColor (ColorTransform parentColor)
		{
			base.updateDisplayedColor (parentColor);
			updateColor ();
		}

		public override byte opacity {
			set {
				base.opacity = value;
				updateColor();
			}
		}

		public override void updateDisplayedOpacity (OpacityTransform parentOpacity)
		{
			base.updateDisplayedOpacity (parentOpacity);
			updateColor ();
		}

		#endregion

		#region CCSpriteProtocol
		public CCSpriteFrame displayFrame{
			get{ return displayedFrame;}
			set{ 
				displayedFrame = value;
			}
		}
		#endregion
	}
}
