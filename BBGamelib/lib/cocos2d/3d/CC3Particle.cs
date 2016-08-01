using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class CC3ParticleController{
		public delegate void Callback(CC3ParticleController ctr);
		Callback _callback;
		ParticleSystem _particle;
		public Callback callback{get{ return _callback;}set{_callback = value;}}
		public ParticleSystem particleSystem{get{return _particle;}}

		public CC3ParticleController(ParticleSystem particle){
			_particle = particle;
			_particle.Stop (true);
			_particle.gameObject.SetActive (false);
		}

		public void play(){
			_particle.gameObject.SetActive (true);
			_particle.Play ();
			CCDirector.sharedDirector.scheduler.schedule (this.update, this, 0, CCScheduler.kCCRepeatForever, false);
		}

		public void stop(){
			_particle.Stop ();
		}

	 	void update (float dt)
		{
			if(_particle == null || !_particle.IsAlive() || _particle.isStopped )
			{
				CCDirector.sharedDirector.scheduler.unscheduleSelector (this.update, this);
				if(_callback!=null){
					_callback(this);
				}
			}
		}

	}
	public class CC3Particle : CC3Node
	{
		public delegate void Callback(CC3Particle spt);
		CC3ParticleController _controller;
		string _path;
		bool _isAutoDestory;
		Callback _callback;
		GameObject _prefabObj;
		ParticleSystem[] _particleSystems;
		Renderer[] _renderers;
		public bool isAutoDestory{get{ return _isAutoDestory;}set{_isAutoDestory = value;}}
		public Callback callback{get{ return _callback;}set{_callback = value;}}
		public ParticleSystem particleSystem{get{return _controller.particleSystem;}}
		public CC3Particle(string path){
			_path = path;
			_isAutoDestory = true;
			_callback = null;
			_prefabObj = CC3SpriteFactory.Instance.getPrefabObject (path, true);
			NSUtils.Assert (_prefabObj != null, "CC3Particle : Prefab not found at path {0}.", path);
			ParticleSystem particle = _prefabObj.GetComponent<ParticleSystem>();
			NSUtils.Assert (particle != null, "CC3Particle : ParticleSystem not found at path {0}.", path);

			_prefabObj.transform.parent = this.transform;
			_prefabObj.transform.localPosition = Vector3.zero;
			_prefabObj.transform.localEulerAngles = Vector3.zero;
			_prefabObj.transform.localScale = new Vector3 (1, 1, 1);

			_controller = new CC3ParticleController (particle);
			_controller.callback = onControllerCallback;
		}
		public CC3Particle(GameObject obj){
			_path = obj.name;
			_isAutoDestory = true;
			_callback = null;
			_prefabObj = obj;
			ParticleSystem particle = _prefabObj.GetComponent<ParticleSystem>();
			NSUtils.Assert (particle != null, "CC3Particle : ParticleSystem not found at path {0}.", _path);

			_prefabObj.transform.parent = this.transform;
			_prefabObj.transform.localPosition = Vector3.zero;
			_prefabObj.transform.localEulerAngles = Vector3.zero;
			_prefabObj.transform.localScale = new Vector3 (1, 1, 1);
			_controller = new CC3ParticleController (particle);
			_controller.callback = onControllerCallback;
		}

		public void play(){
			_controller.play ();
		}

		void onControllerCallback(CC3ParticleController ctr){
			if(_callback!=null){
				_callback(this);
			}
			if(_isAutoDestory){
				removeFromParentAndCleanup(true);
			}
		}

		public override void cleanup ()
		{
			base.cleanup ();
			_controller.stop ();
			//reset default layer
			for (int i=renderers.Length-1; i>=0; i--) {
				var renderer = renderers [i];
				renderer.sortingLayerName = CCFactory.LAYER_DEFAULT;
				renderer.gameObject.layer = LayerMask.NameToLayer(CCFactory.LAYER_DEFAULT);
			}
			CC3SpriteFactory.Instance.recyclePrefabObject (_path, _prefabObj);
		}
		
		public Renderer[] renderers{
			get{
				if(_renderers==null){
					_renderers = this.gameObject.GetComponentsInChildren<Renderer> (true);
				}
				return _renderers;
			}
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
	}
}

