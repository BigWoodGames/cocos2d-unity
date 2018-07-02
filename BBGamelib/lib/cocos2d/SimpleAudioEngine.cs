using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;


namespace BBGamelib{
	[AddComponentMenu("")]
	[ExecuteInEditMode]
	public class SimpleAudioEngine : MonoBehaviour
	{
		const int AUDIO_SOURCES_NUM = 8;
		[SerializeField] AudioSource _backgroundMusicSource;
		[SerializeField] AudioSource[] _audioSources;
		
		Dictionary<string, AudioClip> _preLoadedAudios;
		float _backgroundMusicVolume;
		float _effectsVolume;
		int _currentSource;
		
		#region singleton
		[SerializeField] [HideInInspector]private bool firstPassFlag=true;
		static SimpleAudioEngine _Instance=null;
		//---------singleton------
		public static SimpleAudioEngine sharedEngine{
			get{
				return _Instance;
			}
		}
		public virtual void Awake() {
			if (_Instance != null && _Instance != this) {
				Destroy (this.gameObject);
				return;
			} else {
				_Instance = this;
			}
			if (Application.isPlaying) {
				_backgroundMusicVolume = 1;
				_effectsVolume = 1;
				_currentSource = 0;
			} 
			if (firstPassFlag) {
				gameObject.transform.position =Vector3.zero;
				gameObject.name = "AudioEngine";
				firstPassFlag = false;
				
				_backgroundMusicSource = gameObject.AddComponent<AudioSource>();
				_audioSources = new AudioSource[AUDIO_SOURCES_NUM];
				for(int i=0; i<AUDIO_SOURCES_NUM; i++)
					_audioSources[i] = gameObject.AddComponent<AudioSource>();
			}
		}
		#endregion
		
		public float backgroundMusicVolume{
			get{return _backgroundMusicVolume;} 
			set{
				_backgroundMusicVolume=value;
				_backgroundMusicSource.volume = _backgroundMusicVolume;
			}
		}
		public float effectsVolume{
			get{ return _effectsVolume;}
			set{
				_effectsVolume = value;
				_audioSources[_currentSource].volume = _effectsVolume;
			}
		}
		
		/** Preloads a music file so it will be ready to play as background music */
		public void preloadBackgroundMusic(string filePath){
			if (_preLoadedAudios == null)
				_preLoadedAudios = new Dictionary<string, AudioClip> ();
			string ext = Path.GetExtension(filePath);
			if(ext!=null && ext.Length>0)
				filePath = filePath.Replace (ext, "");
			AudioClip audio = Resources.Load<AudioClip> (filePath);
			if(audio==null)
				CCDebug.Warning ("cocos2d:SimpleAudioEngine: Audio {0} not found.", filePath);
			else
				_preLoadedAudios [filePath] = audio;
		}
		
		public void playBackgroundMusic(string filePath, bool loop=true){
			AudioClip audio = getAudioClip (filePath);
			if(audio==null)
				CCDebug.Warning ("cocos2d:SimpleAudioEngine: Audio {0} not found.", filePath);
			else
                playBackgroundMusic (audio, loop);
		}
		
        public void playBackgroundMusic(AudioClip audio, bool loop=true){
            stopBackgroundMusic ();
            playAudio (audio, _backgroundMusicSource, loop, _backgroundMusicVolume);
        }

		public void stopBackgroundMusic(){
			_backgroundMusicSource.Stop ();
			_backgroundMusicSource.clip = null;
		}
		
		public void pauseBackgroundMusic(){
			_backgroundMusicSource.Pause ();
		}
		
		public void resumeBackgroundMusic(){
			_backgroundMusicSource.Play ();
		}
		
		public int playEffect(string filePath){
			if (filePath == null) {
				CCDebug.Warning ("cocos2d:SimpleAudioEngine: Audio file path should not be null.");					
				return - 1;
			}
			AudioClip audio = getAudioClip (filePath);
			if(audio==null){
				CCDebug.Warning ("cocos2d:SimpleAudioEngine: Audio {0} not found.", filePath);
				return -1;
			}else{
                return playEffect(audio);
			}
		}

        public int playEffect(AudioClip audio)
        {
            _currentSource = (_currentSource+1)%AUDIO_SOURCES_NUM;
            playAudio (audio, _audioSources[_currentSource], false, _effectsVolume);
            return _currentSource;
        }
		
		public void stopEffect(int soundId){
			if (soundId < _audioSources.Length) {
				_audioSources[soundId].Stop();
				_audioSources[soundId].clip = null;
			}
		}
		
		AudioClip getAudioClip(string filePath){
			string ext = Path.GetExtension(filePath);
			if(ext!=null && ext.Length>0)
				filePath = filePath.Replace (ext, "");
			AudioClip audio;
			if(_preLoadedAudios!=null && _preLoadedAudios.TryGetValue(filePath, out audio)){
				return audio;
			}
			audio = Resources.Load<AudioClip> (filePath);
			return audio;
		}
		
		void playAudio(AudioClip audio, AudioSource source, bool loop, float volume){
			source.loop = loop;
			source.clip = audio;
			source.volume = volume;
			if (loop)
				source.Play ();
			else
				source.PlayOneShot (audio, volume);
		}
		
	}
}
