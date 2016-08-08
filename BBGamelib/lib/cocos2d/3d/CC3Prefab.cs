using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class CC3Prefab : CC3Node
	{
		protected GameObject _prefabObj;
		protected string _path;
		Renderer[] _renderers;
		ParticleSystem[] _particleSystems;
		
		bool _opacityModifyRGB;
		Color _quadColor;
		Color32[] _originalColors;
		Color32[] _originalParticleColors;

		protected Bounds _bounds;
		protected bool _isBoundsDirty;
		
		bool _reused;

		public CC3Prefab(string path){
			_path = path;

			_prefabObj = CC3SpriteFactory.Instance.getPrefabObject (path, true);
			NSUtils.Assert (_prefabObj != null, "CC3Prefab : Prefab not found at path {0}.", path);
			_prefabObj.transform.parent = this.transform;
			_prefabObj.transform.localPosition = Vector3.zero;
			_prefabObj.transform.localEulerAngles = Vector3.zero;
			_prefabObj.transform.localScale = new Vector3 (1, 1, 1);

			_opacityModifyRGB = true;
			_quadColor = new Color32 (255, 255, 255, 255);
			_isBoundsDirty = true;
			_bounds = new Bounds (Vector3.zero, Vector3.zero);
			_reused = true;
		}

		public CC3Prefab(GameObject obj){
			_path = obj.name;

			_prefabObj = obj;
			_prefabObj.transform.parent = this.transform;
			_prefabObj.transform.localPosition = Vector3.zero;
			_prefabObj.transform.localEulerAngles = Vector3.zero;
			_prefabObj.transform.localScale = new Vector3 (1, 1, 1);

			_opacityModifyRGB = true;
			_quadColor = new Color32 (255, 255, 255, 255);
			_isBoundsDirty = true;
			_bounds = new Bounds (Vector3.zero, Vector3.zero);
			_reused = true;
		}

		public string path{
			get{ return _path;}
		}

		public GameObject prefabObj{
			get{return _prefabObj;}
		}
		
		public bool resued{
			get{ return _reused;}
			set{ _reused = value;}
		}

		public Renderer[] renderers{
			get{
				if(_renderers==null){
					_renderers = this.gameObject.GetComponentsInChildren<Renderer> (true);
				}
				return _renderers;
			}
		}
		public ParticleSystem particleSystem{
				get{return (this.particleSystems == null || this.particleSystems.Length==0)? null :_particleSystems[0];}
		}

		public ParticleSystem[] particleSystems{
			get{
				if(_particleSystems==null){
					_particleSystems = _prefabObj.GetComponentsInChildren<ParticleSystem>(true);
				}
				return _particleSystems;
			}
		}
		
		/*Get the child object with specified name under prefab object.*/
		public GameObject getChildObject(string name){
			return ccUtils.GetChildObject(_prefabObj, name);
		}

		
		public Bounds getLocalbounds(bool recalculateIfNeed=true){
			if (_isBoundsDirty && recalculateIfNeed) {
				var bounds = calculateLocalBounds(this.renderers);
				bounds.center *= UIWindow.PIXEL_PER_UNIT;
				bounds.size *= UIWindow.PIXEL_PER_UNIT;
				_bounds = bounds;
				_isBoundsDirty = false;
			}
			//Fixed: convert bounds to content space
			return _bounds;
		}
		public Bounds getLocalbounds<T>() where T:Renderer{
			T[] renderers = this.gameObject.GetComponentsInChildren<T> ();
			if (renderers.Length == 0)
				return new Bounds();
			var combinedBounds = calculateLocalBounds (renderers);
			combinedBounds.center *= UIWindow.PIXEL_PER_UNIT;
			combinedBounds.size *= UIWindow.PIXEL_PER_UNIT;
			return combinedBounds;
		}
		
		Bounds calculateLocalBounds(Renderer[] renderers){
			var combinedBounds = renderers [0].bounds;
			for (int i=renderers.Length-1; i>0; i--) {
				var renderer = renderers [i];
				if(renderer.gameObject.activeSelf)
					combinedBounds.Encapsulate (renderer.bounds);
			}
			
			combinedBounds = cc3Utils.ConvertToLocalBounds (this.transform, combinedBounds);
			return combinedBounds;
		}

		protected override void draw ()
		{
			ccUtils.CC_INCREMENT_GL_DRAWS ();
			
			Renderer[] rs = this.renderers;
			for (int i=0; i<rs.Length; i++) {
				rs [i].sortingOrder = CCDirector.sharedDirector.globolRendererSortingOrder;
			}
			CCDirector.sharedDirector.globolRendererSortingOrder ++;
		}

		public override void cleanup ()
		{
			if (_originalColors != null) {
				for (int i=renderers.Length-1; i>=0; i--) {
					var renderer = renderers [i];
					renderer.material.color = _originalColors [i];
				}
			}
			if(_originalParticleColors != null){
				for(int i=_originalParticleColors.Length-1; i>=0;i--){
					_particleSystems[i].startColor = _originalParticleColors[i];
				}
			}

			//reset default layer
			for (int i=renderers.Length-1; i>=0; i--) {
				var renderer = renderers [i];
				renderer.sortingLayerName = CCFactory.LAYER_DEFAULT;
				renderer.gameObject.layer = LayerMask.NameToLayer(CCFactory.LAYER_DEFAULT);
			}

			base.cleanup ();
			if (_reused) {
				CC3SpriteFactory.Instance.recyclePrefabObject (_path, _prefabObj);
			} else {
				if(Application.isEditor)
					UnityEngine.Object.DestroyImmediate(_prefabObj, true);
				else
					UnityEngine.Object.Destroy(_prefabObj);
			}
			_prefabObj = null;
		}

		#region CCSprite - RGBA protocol
		public void updateColor()
		{
			Color32 color4 = new Color32(_displayedColor.r, _displayedColor.g, _displayedColor.b, _displayedOpacity);
			
			// special opacity for premultiplied textures
			if ( _opacityModifyRGB ) {
				color4.r = (byte)(color4.r * _displayedOpacity/255.0f);
				color4.g = (byte)(color4.g * _displayedOpacity/255.0f);
				color4.b = (byte)(color4.b * _displayedOpacity/255.0f);
			}
			_quadColor = color4;
			Renderer[] renderers = this.renderers;
			if(_originalColors == null){
				_originalColors = new Color32[_renderers.Length];
				for(int i=_originalColors.Length-1; i>=0;i--){
					if(_renderers[i].material.HasProperty("_Color"))
						_originalColors[i] = _renderers[i].material.color;
				}
			}
			for (int i=renderers.Length-1; i>=0; i--) {
				var renderer = renderers [i];
				if(renderer.material.HasProperty("_Color")){
					renderer.material.color = _quadColor;
				}
			}
			if (this.particleSystems != null && this.particleSystems.Length > 0) {
				if (_originalParticleColors == null) {
					_originalParticleColors = new Color32[_particleSystems.Length];
					for (int i=_originalParticleColors.Length-1; i>=0; i--) {
						_originalParticleColors [i] = _particleSystems [i].startColor;
					}
				}
				for (int i=_particleSystems.Length-1; i>=0; i--) {
					var particleSystem = _particleSystems [i];
					particleSystem.startColor = _originalParticleColors [i] * _quadColor;
				}
			}
		}
		
		public override Color32 color {
			set {
				base.color = value;
				updateColor();
			}
		}
		
		public override void updateDisplayedColor (Color32 parentColor)
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
		public override bool opacityModifyRGB{
			get{return _opacityModifyRGB;}
			set{
				if( _opacityModifyRGB != value ) {
					_opacityModifyRGB = value;
					updateColor();
				}
			}
		}
		
		public override void updateDisplayedOpacity (byte parentOpacity)
		{
			base.updateDisplayedOpacity (parentOpacity);
			updateColor ();
		}
		
		#endregion
	}
}
