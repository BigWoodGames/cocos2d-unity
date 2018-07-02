using UnityEngine;
using System.Collections;
using System;

namespace BBGamelib{
	public class CC3Node : CCNodeRGBA
	{
		public enum kRotationSortingOrder
		{
			XYZ,
			XZY,
			YXZ,
			YZX,
			ZXY,
			ZYX,

		}
		protected float _rotationX;
		protected float _rotationY;
		protected kRotationSortingOrder _rotationSortingOrder;
		protected float _scaleZ;
		protected float _positionZ;

        protected override void initWithGear(CCFactoryGear gear)
        {
            base.initWithGear(gear);
            _rotationX = 0;
            _rotationY = 0;
            _rotationSortingOrder = kRotationSortingOrder.YXZ;
            _scaleZ = 1;
            _positionZ = 0;
        }

		public virtual float rotationX{
			get{return _rotationX;}
			set{
				if(!FloatUtils.EQ(_rotationX, value)){
					_rotationX = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}
		public virtual float rotationY{
			get{return _rotationY;}
			set{
				if(!FloatUtils.EQ(_rotationY, value)){
					_rotationY = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}
        public virtual float rotationZ{
			get{return _rotation;}
			set{this.rotation=value;}
		}

        /**
         * rotationSortingOrder = YXZ
         */
        public virtual Quaternion quaternion{
            set {
                Vector3 r = value.eulerAngles;
                this.rotationX = -r.x;
                this.rotationY = -r.y;
                this.rotationZ = -r.z;
                this.rotationSortingOrder = kRotationSortingOrder.YXZ;
            }
            get{
                return this.gameObject.transform.localRotation;
            }
        }

		public virtual kRotationSortingOrder rotationSortingOrder{
			get{return _rotationSortingOrder;}
			set{
				if(_rotationSortingOrder!=value){
					_rotationSortingOrder = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}


		public override float scale{
			get{
				NSUtils.Assert( FloatUtils.EQ(_scaleX, _scaleY) && FloatUtils.EQ(_scaleX, _scaleZ), "CCNode#scale. ScaleX != ScaleY != ScaleZ. Don't know which one to return");
				return _scaleX;
			} 
			set{
				if(FloatUtils.NEQ(_scaleX, value) || FloatUtils.NEQ(_scaleY, value) || FloatUtils.NEQ(_scaleZ, value)){
					scaleX = value;
					scaleY = value;
					scaleZ = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}

		public virtual float scaleZ {
			get{return _scaleZ;}
			set {
				if(FloatUtils.NEQ(_scaleZ, value)){
					_scaleZ = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}

		/** Position (x,y) of the node in points. (0,0) is the left-bottom corner. */
		public virtual Vector3 position3D{
			get{
				return new Vector3(_position.x, _position.y, _positionZ);	
			}
			set{
				if(FloatUtils.NEQ(_position.x, value.x)||FloatUtils.NEQ(_position.y, value.y)||FloatUtils.NEQ(_positionZ, value.z)){
					_position = value;
					_positionZ = value.z;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}
		public virtual float positionZ{
			get{
				return _positionZ;
			}
			set{
				if(FloatUtils.NEQ(_positionZ, value)){
					_positionZ = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}

		/** convert position in pixels of world space into local space.*/
		public virtual Vector3 convertToNodeSpace3d(Vector3 pos3dInWorld){
			transformAncestors ();
			updateTransform();
			Vector3 posInLocal = transform.InverseTransformPoint(pos3dInWorld/UIWindow.PIXEL_PER_UNIT);
			posInLocal *= UIWindow.PIXEL_PER_UNIT;
			return posInLocal;
		}

		/** convert position in pixels of local space into world space.*/
		public virtual Vector3 convertToWorldSpace3d(Vector3 pos3dInLocal){
			transformAncestors ();
			updateTransform();
			Vector3 pos3dInWorld = transform.TransformPoint(pos3dInLocal/UIWindow.PIXEL_PER_UNIT);
			pos3dInWorld *= UIWindow.PIXEL_PER_UNIT;
			return pos3dInWorld;
		}

		public virtual Vector3 convertToParentSpace3d(Vector3 pos3dInLocal){
			Vector3 pos3dInWorld = convertToWorldSpace3d (pos3dInLocal);
			
			Vector3 pos3dInParent =Vector3.zero;
			if (this.parent is CC3Node) {
				pos3dInParent = (this.parent as CC3Node).convertToNodeSpace3d (pos3dInWorld);
			} else {
				pos3dInParent = this.parent.convertToNodeSpace (pos3dInWorld);
			}
			return pos3dInParent;
		}

		public override void updateTransform ()
		{
			if (_isUpdateTransformDirty) {
				//position
				Vector2 pInParentAR = _position;
				if(_parent!=null){
					if(_parent.ignoreAnchorPointForPosition){
						if(FloatUtils.NEQ(_parent.scaleX, 0) && FloatUtils.NEQ(_parent.scaleY, 0)){
							pInParentAR += new Vector2(_parent.anchorPointInPixels.x * (1/_parent.scaleX - 1), _parent.anchorPointInPixels.y * (1/_parent.scaleY - 1)); 
						}
					}else{
						pInParentAR -= _parent.anchorPointInPixels;
					}
				}
                Vector2 uPInParentAR = ccUtils.PixelsToUnits (pInParentAR);
				Vector3 pos = transform.localPosition;
                pos.x = uPInParentAR.x;
                pos.y = uPInParentAR.y;
				pos.z = _positionZ / UIWindow.PIXEL_PER_UNIT;
				transform.localPosition = pos;

				//rotation
				transform.localPosition = pos;
				transform.localEulerAngles =Vector3.zero;
				switch(_rotationSortingOrder){
				case kRotationSortingOrder.XYZ:
					transform.Rotate(-_rotationX, 0, 0);
					transform.Rotate(0, -_rotationY, 0);
					transform.Rotate(0, 0, -_rotation);
					break;
				case kRotationSortingOrder.XZY:
					transform.Rotate(-_rotationX, 0, 0);
					transform.Rotate(0, 0, -_rotation);
					transform.Rotate(0, -_rotationY, 0);
					break;
				case kRotationSortingOrder.YXZ:
					transform.Rotate(0, -_rotationY, 0);
					transform.Rotate(-_rotationX, 0, 0);
					transform.Rotate(0, 0, -_rotation);
					break;
				case kRotationSortingOrder.YZX:
					transform.Rotate(0, -_rotationY, 0);
					transform.Rotate(0, 0, -_rotation);
					transform.Rotate(-_rotationX, 0, 0);
					break;
				case kRotationSortingOrder.ZXY:
					transform.Rotate(0, 0, -_rotation);
					transform.Rotate(-_rotationX, 0, 0);
					transform.Rotate(0, -_rotationY, 0);
					break;
				case kRotationSortingOrder.ZYX:
					transform.Rotate(0, 0, -_rotation);
					transform.Rotate(0, -_rotationY, 0);
					transform.Rotate(-_rotationX, 0, 0);
					break;
				}
				//scale
				transform.localScale = new Vector3 (_scaleX, _scaleY, _scaleZ);

				_isUpdateTransformDirty = false;
			}
		}

		protected override void recycleGear ()
		{
			this.transform.position =Vector3.zero;
			this.transform.localScale = new Vector3 (1, 1, 1);
			this.transform.eulerAngles =Vector3.zero;
			base.recycleGear ();
		}
	}
}

