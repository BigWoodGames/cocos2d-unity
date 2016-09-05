using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BBGamelib{
	public enum kTweenMode{
		SkipFrames,
		SkipNoLabelFrames,
		TweenFrames,
	}
}
namespace BBGamelib.flash.imp{
	public interface DisplayObject 
	{
		/*id*/
		int characterId{ get;}

		/*View for renderer*/
		CCNodeRGBA view{get;}
	
		/*Returns a rectangle that defines the area of the display object relative to the coordinate system of the targetCoordinateSpace object.*/
		Rect bounds{ get;}		

		/*Class name */ 
		string className{ get;}
		
		/*Class name */ 
		string instanceName{ get; set;}

		/*apply the matrix to the dispay object*/ 
		void applyPlaceObject(PlaceObject obj);

		float fps{ get; set;}

		DisplayObject parent{ get; set;}
		
		kTweenMode tweenMode{ get; set;}
	}

	public abstract class DisplayObjectImp : DisplayObject
	{		
		public abstract int characterId{ get;}
		public abstract CCNodeRGBA view{get;}
		public abstract Rect bounds{ get;}	
		public abstract string className{ get;}
		public abstract float fps{ get; set;}
		public abstract kTweenMode tweenMode{ get; set;}

		protected string _instanceName;
		public string instanceName{ get{return _instanceName;} set{_instanceName = value;}}

		protected DisplayObject _parent;
		public DisplayObject parent{ get{return _parent;} set{_parent = value;}}

		public void applyPlaceObject(PlaceObject obj){
			_instanceName = obj.instanceName;
			if (obj.hasMatrix) {
				view.position = new Vector2(obj.position.x, -obj.position.y);
				view.rotation = obj.rotation;
				view.scaleX = obj.scaleX;
				view.scaleY = obj.scaleY;
			} else if (obj.hasCharacter) {
				view.position = Vector2.zero;
				view.rotation = 0;
				view.scaleX = 1;
				view.scaleY = 1;
			}
			view.visible = obj.isVisible;
			view.zOrder = obj.depth;
			if (obj.hasColorTransform) {
				if (obj.colorTransform.add == new Color32 (0, 0, 0, 0)) {
					Color viewColor = view.color;
					viewColor.a = obj.colorTransform.multiply.a;
					if (viewColor != obj.colorTransform.multiply){
						view.color = obj.colorTransform.multiply;
					}
					byte alpha = (byte)(Mathf.RoundToInt(viewColor.a * 255));
					if (view.opacity != alpha){
						view.opacity = alpha;
					}
				}else{
					Color viewColor = view.color;
					viewColor.a = obj.colorTransform.multiply.a;
					byte alpha = (byte)(Mathf.RoundToInt(viewColor.a * 255));
					if (view.opacity != alpha){
						view.opacity = alpha;
					}
					//TODO shader support for add color
				}
			} 
			else if (obj.hasCharacter){
				view.color = Color.white;
				view.opacity = 255;
			}
		}

	}
}
