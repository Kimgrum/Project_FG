using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FGDefine;

public abstract class BaseScene : MonoBehaviour, IObserver
{
    public ENUM_SCENE_TYPE SceneType { get; protected set; } = ENUM_SCENE_TYPE.Unknown;

    protected IEnumerator Start()
    {
        Debug.Log("확인");
        yield return null;
        Managers.Sound.ResisterObserver(this);
        Init();
    }

    public virtual void Init()
    {
        Managers.Sound.NotifyObserver();
        Managers.UI.popupCanvas.Play_FadeOutEffect();
        Managers.Scene.Get_CurrSceneType(SceneType);
    }

    public virtual void Clear()
    {
        Managers.Sound.RemoveObserver(this);
    }

    public abstract void Update_BGM();
}
 