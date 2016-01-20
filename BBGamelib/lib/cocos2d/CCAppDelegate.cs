using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public abstract class CCAppDelegate : NSAppDelegate
	{
		[SerializeField] protected CCDirector _director;	
		[SerializeField] protected CCGLView _view;
		[SerializeField] protected CCFactory _factory;
		[SerializeField] protected SimpleAudioEngine _audioEngine;
		
		public override void applicationRunOnceOnBuilding ()
		{
			//CCGLView
			{
				GameObject obj = new GameObject ();
				obj.transform.parent = _window.transform;
				_view = obj.AddComponent<CCGLView> ();
			}
			//CCComponentsPool
			{
				GameObject obj = new GameObject();
				obj.transform.parent = this.transform;
				_factory = obj.AddComponent<CCFactory>();
			}

			//audio
			{
				GameObject obj = new GameObject ();
				obj.transform.parent = this.transform;
				_audioEngine = obj.AddComponent<SimpleAudioEngine> ();
			}
		}
	}
}

