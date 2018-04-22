using UnityEngine;
using System.Collections;
using BBGamelib;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BBGamelib
{
    /**
     * Note: multiple animator layers mode is not supported yet.
     */ 
    public class CC3FbxGeneric : CC3Prefab
    {

        // ------------------------------------------------------------------------------
        //  init
        // ------------------------------------------------------------------------------
        protected Animator _fbxAnimator;
        protected CC3FbxEventScript _evtScript;
        protected float _transitionDuration;

        public CC3FbxGeneric(string path)
        {
            initWithString(path);
        }
        public CC3FbxGeneric(GameObject obj)
        {
            initWithPrefabObj(obj);
        }

        protected CC3FbxGeneric()
        {
        }

        protected override void initWithPrefabObj(GameObject obj)
        {
            base.initWithPrefabObj(obj);
            loadComponents();
        }

        protected virtual void loadComponents()
        {
            _fbxAnimator = _prefabObj.GetComponent<Animator>();
            NSUtils.Assert(_fbxAnimator != null, "{0}#initWithPrefabObj: Animator not found", GetType().Name);
            _fbxAnimator.speed = 1;

            _evtScript = _prefabObj.GetComponent<CC3FbxEventScript>(); 
            if (_evtScript == null)
            {
                _evtScript = _prefabObj.AddComponent<CC3FbxEventScript>();
            }
            _evtScript.eventHandler = null;
            _transitionDuration = 0.25f;            
        }

        public override void cleanup()
        {
            _evtScript.eventHandler = null;
            base.cleanup();
        }

        public CC3FbxEventScript.EventHandler eventHandler
        {
            get{ return _evtScript.eventHandler; }
            set{ _evtScript.eventHandler = value; }
        }
        // ------------------------------------------------------------------------------
        //  goto frame
        // ------------------------------------------------------------------------------       
        /**
         * label=SendEvent.stringParamter
         */
        public void gotoLabel(string anmName, string label)
        {
            int frame = getFrame (anmName, label);
            gotoFrame (anmName, frame);
        }

        public void gotoFrame(string anmName, int frame)
        {
            pause ();
            AnimationClip clip = getAnimationClip(anmName);
            if (clip != null)
            {
                float time = frame / clip.frameRate;
                float nTime = Mathf.Clamp01(time / clip.length);
                _fbxAnimator.Play(anmName, 0, nTime);          
            }
        }

        // ------------------------------------------------------------------------------
        //  animation
        // ------------------------------------------------------------------------------      
        public bool hasAnimation(string anmName)
        {
            
            return getAnimationClip(anmName) != null;
        }

        public void playAnimation(string anmName)
        {
            playAnimation(anmName, 0);
        }

        /**
         * startLabel=SendEvent.stringParamter
         */
        public void playAnimation(string anmName, string startLabel)
        {
            int startFrame = getFrame (anmName, startLabel);
            playAnimation (anmName, startFrame);
        }

        public void playAnimation(string anmName, int start)
        {     
            NSUtils.Assert (start >= 0, "{0}#playAnimation start frame should not be {1}.", GetType().Name, start);
            AnimationClip clip = getAnimationClip(anmName);
            NSUtils.Assert(clip!= null, "{0}#playAnimation {1} not found.", GetType().Name, anmName);
            int totalFrames = getTotalFrames(anmName);
            NSUtils.Assert (start < totalFrames, "{0}#playAnimation start frame {1} should not bigger than {2}.", GetType().Name, start, totalFrames);

            float startTime = Mathf.Clamp01(1f * start / totalFrames);
            if (FloatUtils.ES(_transitionDuration, 0))
                _fbxAnimator.Play(anmName, 0, startTime);
            else
            {
                _fbxAnimator.CrossFadeInFixedTime(anmName, _transitionDuration, 0, startTime * clip.length);  
            }
        }

        /**
         * animator.speed = 0
         */
        public override void pause()
        {
            base.pause();
            _fbxAnimator.speed = 0;
        }

        /**
         * animator.speed = 1
         */
        public override void resume()
        {
            base.resume();
            _fbxAnimator.speed = 1;
        }

        public int getFrame(string anmName, string label)
        {
            AnimationClip clip = getAnimationClip(anmName);
            int totalFrames = Mathf.RoundToInt(clip.frameRate * clip.length);
            for (int i = 0; i < clip.events.Length; i++)
            {
                if (clip.events [i].stringParameter == label)
                {
                    float time = clip.events [i].time;
                    int frame = Mathf.RoundToInt(totalFrames * time / clip.length);
                    return frame;
                }
            }
            return -1;
        }

        public float getFrameRate(string anmName)
        {
            AnimationClip clip = getAnimationClip(anmName);
            if (clip != null)
                return clip.frameRate;
            return 0;
        }
       
        public int getTotalFrames(string anmName)
        {
            AnimationClip clip = getAnimationClip(anmName);
            if (clip != null)
            {
                int totalFrames = Mathf.RoundToInt(clip.frameRate * clip.length);
                return totalFrames;
            }
            return 0;
        }

        public AnimationClip getAnimationClip(string anmName)
        {
            AnimationClip[] clips = _fbxAnimator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips [i].name == anmName)
                    return clips [i];
            }
            return null;
        }

        public float transitionDuration
        {
            get{ return _transitionDuration; }
            set{ _transitionDuration = value; }
        }

        public Animator animator
        {
            get{ return _fbxAnimator; }
        }
    }
}

