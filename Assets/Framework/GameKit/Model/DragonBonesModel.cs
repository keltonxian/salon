using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class DragonBonesModel : BaseModel
{
    private UnityArmatureComponent _armature;
    private Callback.CallbackV _onAnimComplete;
    [SerializeField]
    protected Callback.UnityEventV _onAnimCompleteEvent = new Callback.UnityEventV();
    public Callback.UnityEventV OnAnimCompleteEvent
    {
        get
        {
            return _onAnimCompleteEvent;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _armature = this.GetComponent<UnityArmatureComponent>();
        Init();
    }

    private void Init()
    {
        //_armature.AddDBEventListener(EventObject.START, OnAnimationEventHandler);
        //_armature.AddDBEventListener(EventObject.LOOP_COMPLETE, OnAnimationEventHandler);
        _armature.AddDBEventListener(EventObject.COMPLETE, OnAnimationEventHandler);
        //_armature.AddDBEventListener(EventObject.FADE_IN, OnAnimationEventHandler);
        //_armature.AddDBEventListener(EventObject.FADE_IN_COMPLETE, OnAnimationEventHandler);
        //_armature.AddDBEventListener(EventObject.FADE_OUT_COMPLETE, OnAnimationEventHandler);
        //_armature.AddDBEventListener(EventObject.START, OnAnimationEventHandler);
        //// custom event
        //_armature.AddDBEventListener(EventObject.FRAME_EVENT, OnAnimationEventHandler);
        //// sound event
        //UnityFactory.factory.soundEventManager.AddDBEventListener(EventObject.SOUND_EVENT, OnAnimationEventHandler);
    }

    private void OnAnimationEventHandler(string type, EventObject eventObject)
    {
        //Debug.Log(string.Format("animationName:{0},eventType:{1},eventName:{2}", eventObject.animationState.name, type, eventObject.name));
        if (type == EventObject.COMPLETE)
        {
            _onAnimComplete.Invoke();
            OnAnimCompleteEvent.Invoke();
            return;
        }
    }

    public override void PlayAnimation (string animName, Callback.CallbackV callback)
    {
        _onAnimComplete = callback;
        _armature.animation.Play(animName, 1);
    }

    public override void PlayAnimationLoop(string animName)
    {
        _armature.animation.Play(animName, 0);
    }
}
