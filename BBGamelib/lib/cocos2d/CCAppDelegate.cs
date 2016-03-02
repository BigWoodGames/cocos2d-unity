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
			
			#if !UNITY_EDITOR
			if(Application.isMobilePlatform) 
			{ 
				Cursor.visible = false; // Once Android cursor bug is fixed, you can delete the next 3 lines. 
				Texture2D cursorTexture = new Texture2D(1, 1); 
				cursorTexture.SetPixel(0, 0, Color.clear); 
				Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto); 
			}
			#endif
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

