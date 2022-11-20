using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FGDefine;

public abstract class BaseScene : MonoBehaviour
{
    public ENUM_SCENE_TYPE SceneType { get; protected set; } = ENUM_SCENE_TYPE.Unknown;

    protected IEnumerator Start()
    {
        yield return null;
        Init();
    }

    public virtual void Init()
    {
        Managers.Sound.Play((ENUM_BGM_TYPE)SceneType, ENUM_SOUND_TYPE.BGM);
        Managers.UI.popupCanvas.Play_FadeOutEffect();
        Managers.Scene.Get_CurrSceneType(SceneType);
    }
    
    public virtual void Clear()
    {

    }
}
 