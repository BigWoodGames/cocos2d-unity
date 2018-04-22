using UnityEngine;
using System.Collections;

namespace BBGamelib
{
    public class CC3FbxEventScript: MonoBehaviour
    {
        public delegate void EventHandler(AnimationEvent evt);

        private EventHandler _eventHandler;
        private Animator _animator;

        public void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public EventHandler eventHandler
        {
            get{ return _eventHandler; }
            set{ _eventHandler = value; }
        }

        public void SendEvent(AnimationEvent evt)
        {
            if (_eventHandler != null)
            {
                AnimatorStateInfo evtInfo = evt.animatorStateInfo;
                AnimatorStateInfo curInfo = _animator.GetCurrentAnimatorStateInfo(0);
                AnimatorStateInfo nextInfo = _animator.GetNextAnimatorStateInfo(0);
                bool isInTransition = _animator.IsInTransition(0);

                //skip previous events
                if (isInTransition)
                {
                    if (evtInfo.fullPathHash != nextInfo.fullPathHash)
                        return;

                    //skip previous events when curState == nextStateInfo and real time < evt time.
                    //it might has bug here
                    if (curInfo.fullPathHash == nextInfo.fullPathHash)
                    {
                        float evtSupposedTime = evt.time / evtInfo.length;
                        float evtRealTime = (evtInfo.normalizedTime) % 1 ;
                        if(FloatUtils.Small(evtRealTime, evtSupposedTime))
                        {
                            return;
                        }
                    }
                } else
                {
                    if (evtInfo.fullPathHash != curInfo.fullPathHash)
                        return;
                }

                _eventHandler(evt);
            }
        }
    }
}
