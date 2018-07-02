using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class CC3Prefab : CC3Node
	{
        public enum kCleanupMode
        {
            Ignor,
            Recycle,
            Destroyed
        }

		protected GameObject _prefabObj;
		protected string _path;
		protected Renderer[] _renderers;
		protected ParticleSystem[] _particleSystems;

		protected Color32[] _originalRendererColors;
		protected Color32[] _originalParticleColors;
		protected float[] _originalParticleTimes;
		protected float[] _originalParticleStartSize;
		protected float[] _originalParticleStartSpeed;
		protected float[] _originalParticleGravityModifer;
		protected Bounds _bounds;
		protected bool _isBoundsDirty;
		
        protected kCleanupMode _cleanupMode;

		public CC3Prefab(string path){
            initWithString(path);
		}

		public CC3Prefab(GameObject obj){
            initWithPrefabObj(obj);
            _cleanupMode = kCleanupMode.Ignor;
		}

        protected CC3Prefab(){
        }

        protected virtual void initWithString(string path){
            GameObject obj = CC3SpriteFactory.Instance.getPrefabObject (path, true);
            initWithPrefabObj(obj);

            GameObject prefab = CC3SpriteFactory.Instance.getPrefab(path);
            obj.transform.localScale = prefab.transform.localScale;
            obj.transform.rotation = prefab.transform.rotation;

            _path = path;
        }

        protected virtual void initWithPrefabObj(GameObject obj){
            _path = obj.name;

            _prefabObj = obj;
            NSUtils.Assert (_prefabObj != null, "CC3Prefab : Prefab not found at path {0}.", path);
            _prefabObj.transform.parent = this.transform;
            _prefabObj.transform.localPosition =Vector3.zero;

            Renderer[] rs = this.renderers;
            for (int i=0; i<rs.Length; i++) {
                rs [i].sortingOrder = 0;
            }

            _isBoundsDirty = true;
            _bounds = new Bounds (Vector3.zero,Vector3.zero);
            _cleanupMode = kCleanupMode.Recycle;
        }
		
		public virtual void pause(){
			pauseSchedulerAndActions ();
			if(this.particleSystems!=null){
				for(int i=0; i<_particleSystems.Length; i++){
					_particleSystems[i].Pause(true);
				}
			}
		}
		public virtual void resume(){
			resumeSchedulerAndActions ();
			if(_particleSystems!=null){
				for(int i=0; i<_particleSystems.Length; i++){
					_particleSystems[i].Play();
				}
			}
		}

		public string path{
			get{ return _path;}
		}

		public GameObject prefabObj{
			get{return _prefabObj;}
		}
		
        public kCleanupMode cleanupMode{
            get{ return _cleanupMode;}
            set{ _cleanupMode = value;}
		}

		public Renderer[] renderers{
			get{
				if(_renderers==null){
					_renderers = this.gameObject.GetComponentsInChildren<Renderer> (true);
				}
				return _renderers;
			}
		}
		
		
		// ------------------------------------------------------------------------------
		//  particles
		// ------------------------------------------------------------------------------
		public ParticleSystem particleSystem{
				get{return (this.particleSystems == null || this.particleSystems.Length==0)? null :_particleSystems[0];}
		}

		public ParticleSystem[] particleSystems{
			get{
				if(_particleSystems==null){
					_particleSystems = _prefabObj.GetComponentsInChildren<ParticleSystem>(true);
					_originalParticleTimes = new float[this.particleSystems.Length];
					_originalParticleStartSize = new float[this.particleSystems.Length];
					_originalParticleStartSpeed = new float[this.particleSystems.Length];
					_originalParticleGravityModifer = new float[this.particleSystems.Length];
					for (int i = 0; i < this.particleSystems.Length; i++) {
						_originalParticleTimes[i] = this.particleSystems[i].startLifetime;
						_originalParticleStartSize[i] = this.particleSystems[i].startSize;
						_originalParticleStartSpeed[i] = this.particleSystems[i].startSpeed;
						_originalParticleGravityModifer[i] = this.particleSystems[i].gravityModifier;
					}
				}
				return _particleSystems;
			}
		}

		void setParticleScale(float scale){
			if(this.particleSystems != null){
				for (int i = 0; i < this.particleSystems.Length; i++) {
					this.particleSystems[i].startSize = scale * _originalParticleStartSize[i];
					this.particleSystems[i].startSpeed = scale * _originalParticleStartSpeed[i];
					this.particleSystems[i].gravityModifier = scale * _originalParticleGravityModifer[i];
				}
			}
		}

		public void toggleParticles(bool enable, string tgtName=null){
			if (this.particleSystems != null) {
				for (int i = 0; i < this.particleSystems.Length; i++) {
					if (tgtName == null || this.particleSystems [i].gameObject.name == tgtName) {
						if (enable) {
							this.particleSystems [i].gameObject.SetActive (true);
							this.particleSystems [i].Play ();
							this.particleSystems [i].startLifetime = _originalParticleTimes [i];
						} else {
							this.particleSystems [i].Stop ();
							this.particleSystems [i].startLifetime = 0;
						}
					}
				}
			}
		}

		// ------------------------------------------------------------------------------
		//  utils
		// ------------------------------------------------------------------------------
		/*Get the child object with specified name under prefab object.*/
		public GameObject getChildObject(string name){
			return ccUtils.GetChildObject(_prefabObj, name);
		}

        public GameObject GetChildObjectRecursively(string name){
            return ccUtils.GetChildObjectRecursively(_prefabObj, name);
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
		
		
		// ------------------------------------------------------------------------------
		//  override
		// ------------------------------------------------------------------------------
		protected override void draw ()
		{
			ccUtils.CC_INCREMENT_GL_DRAWS ();
		}

		public override void visit ()
		{
			base.visit ();
			Vector3 scale =  transform.lossyScale;
			if (scale != new Vector3(1, 1, 1)) {
				float minScale = Mathf.Min(scale.x, Mathf.Min(scale.y, scale.z));
				setParticleScale(minScale);
			}
		}

		public override void cleanup ()
		{
			//reset renderers
			if (_originalRendererColors != null) {
				for (int i=renderers.Length-1; i>=0; i--) {
					var renderer = renderers [i];
					CCFactory.Instance.materialPropertyBlock.Clear();
					CCFactory.Instance.materialPropertyBlock.SetColor("_Color", _originalRendererColors[i]);
					CCFactory.Instance.materialPropertyBlock.SetColor("_AddedColor", new Color32(0, 0, 0, 0));
					renderer.SetPropertyBlock(CCFactory.Instance.materialPropertyBlock);
				}
			}

			//reset partcile systems
			if (_originalParticleStartSize != null) {
				setParticleScale (1);
			}
			if (_originalParticleTimes != null) {
				for(int i=_originalParticleTimes.Length-1; i>=0;i--){
					_particleSystems[i].startLifetime = _originalParticleTimes[i];
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

            if (_cleanupMode == kCleanupMode.Recycle) {
				CC3SpriteFactory.Instance.recyclePrefabObject (_path, _prefabObj);
            } else if(_cleanupMode == kCleanupMode.Destroyed){
				if(Application.isEditor)
					UnityEngine.Object.DestroyImmediate(_prefabObj, true);
				else
					UnityEngine.Object.Destroy(_prefabObj);
			}
			_prefabObj = null;
			base.cleanup ();
		}
		
		
		// ------------------------------------------------------------------------------
		//  RGBA protocol
		// ------------------------------------------------------------------------------
		public void updateColor()
		{
			Color32 tint = _displayedColor.tint;
			tint.a = _displayedOpacity.tint;
			Color32 add = _displayedColor.add;
			add.a = _displayedOpacity.add;
			
	
			Renderer[] renderers = this.renderers;
			if (renderers != null) {
				if (_originalRendererColors == null) {
					_originalRendererColors = new Color32[_renderers.Length];
					for (int i=_originalRendererColors.Length-1; i>=0; i--) {
						Renderer renderer = renderers [i];
						Material m = renderer.sharedMaterial;
						if (m!=null && m.HasProperty ("_Color"))
							_originalRendererColors [i] = m.color;
					}
				}
				for (int i=renderers.Length-1; i>=0; i--) {
					var renderer = renderers [i];
					ccUtils.SetRenderColor (renderer, tint, add);
				}
				if (this.particleSystems != null && this.particleSystems.Length > 0) {
					if (_originalParticleColors == null) {
						_originalParticleColors = new Color32[_particleSystems.Length];
						for (int i=_originalParticleColors.Length-1; i>=0; i--) {
							_originalParticleColors [i] = _particleSystems [i].startColor;
						}
					}
					Color tintColor = tint;
					for (int i=_particleSystems.Length-1; i>=0; i--) {
						var particleSystem = _particleSystems [i];
						particleSystem.startColor = _originalParticleColors [i] * tintColor;
					}
				}
			}
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
	}
}
