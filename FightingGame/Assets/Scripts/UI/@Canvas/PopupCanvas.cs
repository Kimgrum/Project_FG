using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using FGDefine;

/// <summary>
/// 최초 한번만 생성되고, 게임 종료까지 파괴되지 않는 캔버스?
/// </summary>
public class PopupCanvas : MonoBehaviour
{
    [Header("Set In Editor")]
    [SerializeField] CharSelectPopup charSelectPopup;
    [SerializeField] SelectPopup selectPopup;
    [SerializeField] NotifyPopup notifyPopup;
    [SerializeField] LoadingPopup loadingPopup;
    [SerializeField] ErrorPopup errorPopup;
    [SerializeField] FadeEffectPopup fadeEffectPopup;
    [SerializeField] TimerNotifyPopup timerNotifyPopup;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        DontDestroyOnLoad(this);
    }

    public void Check_ActivePopup()
    {
        // 애들 활성화 상태 확인해서 전체를 끈다던가 이런거 생각중인데
        // 나중에 해보던가 하자
    }

    /// <summary>
    /// 에러코드와 메세지를 출력하는 Popup Window
    /// </summary>
    public void Open_ErrorPopup(short _returnCode, string _message)
    {
        if(errorPopup.isUsing)
        {
            Debug.Log("이미 에러팝업창이 떠있습니다.");
            return;
        }

        errorPopup.Open(_returnCode, _message);
    }

    /// <summary>
    /// 캐릭터 선택창 Popup Window
    /// </summary>
    public void Open_CharSelectPopup(Action<ENUM_CHARACTER_TYPE> _charCallBack)
    {
        if(charSelectPopup.isUsing)
        {
            Debug.Log("이미 캐릭터선택팝업창이 사용중입니다.");
            return;
        }

        charSelectPopup.Open(_charCallBack);
    }

    /// <summary>
    /// 예, 아니오 선택창 Popup Window 
    /// 해당 버튼의 Action이 없을 경우 null
    /// </summary>
    public void Open_SelectPopup(Action _succeededCallBack, Action _failedCallBack, string _message)
    {
        if(selectPopup.isUsing)
        {
            Debug.Log("이미 선택팝업창이 사용중입니다.");
            return;
        }

        selectPopup.Open(_succeededCallBack, _failedCallBack, _message);
    }

    /// <summary>
    /// 알림창 Popup Window 
    /// 해당 버튼의 Action이 없을 경우 null
    /// 알림 팝업창은 중복해서 호출 시에 새로운 창으로 갱신됨
    /// </summary>
    public void Open_NotifyPopup(string _message, Action _checkCallBack = null)
    {
        if (notifyPopup.isUsing)
        {
            notifyPopup.Open_Again(_message, _checkCallBack);
            return;
        }

        notifyPopup.Open(_message, _checkCallBack);
    }

    /// <summary>
    /// 일정 시간 뒤에 자동으로 사라지는 알림창 Popup Window 
    /// 알림 팝업창은 중복해서 호출 시에 새로운 창으로 갱신됨
    /// </summary>
    public void Open_TimeNotifyPopup(string _message, float _runTime)
    {
        if (timerNotifyPopup.isUsing)
        {
            timerNotifyPopup.Open_Again(_message, _runTime);
            return;
        }

        timerNotifyPopup.Open(_message, _runTime);
    }

    public void Play_FadeInEffect(Action _FadeInCallBack, float _fadeInTime)
    {
        fadeEffectPopup.Play_FadeInEffect(_FadeInCallBack, _fadeInTime);
    }

    public void Play_FadeOutEffect(Action _FadeOutCallBack, float _fadeOutTime)
    {
        fadeEffectPopup.Play_FadeOutEffect(_FadeOutCallBack, _fadeOutTime);
    }

    /// <summary>
    /// 로딩 팝업창 Popup Window
    /// 반드시 Close를 따로 호출해주어야 함
    /// </summary>
    public void Open_LoadingPopup()
    {
        if(loadingPopup.isUsing)
        {
            Debug.Log("이미 선택팝업창이 사용중입니다.");
            return;
        }

        loadingPopup.Open();
    }

    public void Close_LoadingPopup()
    {
        if (!loadingPopup.isUsing)
        {
            Debug.Log("팝업창이 사용중이지 않습니다.");
            return;
        }

        loadingPopup.Close();
    }
}
