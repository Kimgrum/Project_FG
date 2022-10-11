using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FGDefine;
using System;

public class CustomRoomWindowUI : MonoBehaviour, IRoomPostProcess
{
    [SerializeField] Text roomNameText;

    [SerializeField] Image currMapIamge;
    [SerializeField] Image nextMapIamge_Left;
    [SerializeField] Image nextMapIamge_Right;

    [SerializeField] CharProfileUI masterProfile;
    [SerializeField] CharProfileUI slaveProfile;
    CharProfileUI MyProfile
    {
        get
        {
            if (PhotonLogicHandler.IsMasterClient)
                return masterProfile;
            else
                return slaveProfile;
        }
    }
    CharProfileUI YourProfile
    {
        get
        {
            if (PhotonLogicHandler.IsMasterClient)
                return slaveProfile;
            else
                return masterProfile;
        }
    }

    public bool isInit = false;
    public bool isRoomRegisting = false;

    Coroutine readyLockCoroutine;
    Coroutine allReadyCheckCoroutine;

    #region Register, CallBack, OnUpdateProperty 함수 (Server)
    public bool Register_LobbyCallback()
    {
        if (isRoomRegisting)
        {
            Debug.Log("이미 등록된 상태");
            return false;
        }

        isRoomRegisting = true;
        this.RegisterRoomCallback();
        return true;
    }
    public void UnRegister_LobbyCallback()
    {
        if (!isRoomRegisting)
            return;

        isRoomRegisting = false;
        this.UnregisterRoomCallback();
    }

    /// <summary>
    /// 슬레이브 클라이언트가 방에 입장하면 불리는 콜백함수
    /// </summary>
    public void SlaveClientEnterCallBack(string nickname)
    {
        if (PhotonLogicHandler.IsMasterClient)
            YourProfile.Set_UserNickname(nickname);
    }

    /// <summary>
    /// 마스터 클라이언트가 변경됐을 때 불리는 콜백함수
    /// </summary>
    public void MasterClientExitCallBack(string nickname)
    {
        MyProfile.Clear();
        MyProfile.Init();
    }

    /// <summary>
    /// 자신 외의 다른 클라이언트가 방을 나가면 불리는 콜백함수
    /// ( 이 경우, 자신이 무조건 마스터 클라이언트가 됨 )
    /// </summary>
    public void SlaveClientExitCallBack(string nickname)
    {
        YourProfile.Clear();
    }

    public void OnUpdateRoomProperty(CustomRoomProperty property)
    {
        if (PhotonLogicHandler.IsMasterClient)
            return; // 나의 변경된 정보면 리턴

        Set_CurrMapInfo(property.currentMapType);
    }

    public void OnUpdateRoomPlayerProperty(CustomPlayerProperty property)
    {
        if (property.isMasterClient == PhotonLogicHandler.IsMasterClient)
            return; // 나의 변경된 정보면 리턴

        Debug.Log("상대에게 정보를 받아 갱신합니다.");
        YourProfile.Set_Character(property.characterType);
        YourProfile.Set_ReadyState(property.isReady);
    }
    #endregion

    private void Init()
    {
        if (isInit) return;

        isInit = true;

        MyProfile.Init();
    }

    public void Open()
    {
        Register_LobbyCallback();

        Init();

        // 포톤콜백함수 등록
        PhotonLogicHandler.Instance.onEnterRoomPlayer += SlaveClientEnterCallBack;
        PhotonLogicHandler.Instance.onLeftRoomPlayer += SlaveClientExitCallBack;
        PhotonLogicHandler.Instance.onChangeMasterClientNickname += MasterClientExitCallBack;

        Set_CurrRoomInfo();
        this.gameObject.SetActive(true);
    }

    private void Close()
    {
        isInit = false;

        // 포톤콜백함수 해제
        PhotonLogicHandler.Instance.onEnterRoomPlayer -= SlaveClientEnterCallBack;
        PhotonLogicHandler.Instance.onLeftRoomPlayer -= SlaveClientExitCallBack;
        PhotonLogicHandler.Instance.onChangeMasterClientNickname -= MasterClientExitCallBack;

        masterProfile.Clear();
        slaveProfile.Clear();

        UnRegister_LobbyCallback();
        this.gameObject.SetActive(false);
    }

    public void Set_CurrRoomInfo()
    {
        roomNameText.text = PhotonLogicHandler.CurrentRoomName;
        MyProfile.Set_UserNickname(PhotonLogicHandler.CurrentMyNickname);
        
        if (!PhotonLogicHandler.IsMasterClient)
            YourProfile.Set_UserNickname(PhotonLogicHandler.CurrentMasterClientNickname);
        

        // Error : ArgumentNullException: Value cannot be null.
        // ENUM_MAP_TYPE mapType = (ENUM_MAP_TYPE)Enum.Parse(typeof(ENUM_CHARACTER_TYPE), PhotonLogicHandler.CurrentMapName);
        // Set_CurrMapInfo(mapType);
    }

    public void Set_CurrMapInfo(ENUM_MAP_TYPE _mapType = ENUM_MAP_TYPE.BasicMap)
    {
        switch (_mapType)
        {
            case ENUM_MAP_TYPE.BasicMap:
                PhotonLogicHandler.Instance.ChangeMap(_mapType);
                return;
            default:
                Debug.Log("알 수 없는 맵을 선택");
                return;
        }
    }
    public void GoTo_BattleScene()
    {
        if (!PhotonLogicHandler.IsMasterClient)
            return;

        if (PhotonLogicHandler.Instance.IsAllReady() && PhotonLogicHandler.CurrentRoomMemberCount == 2)
            PhotonLogicHandler.Instance.TrySceneLoadWithRoomMember(ENUM_SCENE_TYPE.Battle);
        else
            Managers.UI.popupCanvas.Open_NotifyPopup("게임 시작에 실패했습니다.", UnReadyMyProfile);
    }

    public void UnReadyMyProfile()
    {
        MyProfile.Set_ReadyState(false);
    }

    public void ExitRoom()
    {
        PhotonLogicHandler.Instance.TryLeaveRoom(Close);
    }

    public void OnClick_ChangeMap()
    {
        Debug.Log("맵 변경시켜. (미구현)");
        // 맵 변경시켜.
    }

    public void OnClick_ExitRoom()
    {
        MyProfile.Set_ReadyState(false);

        Managers.UI.popupCanvas.Open_SelectPopup(ExitRoom, null, "정말 방에서 나가시겠습니까?");
    }

    public void OnClick_Ready()
    {
        if (readyLockCoroutine != null)
            return;

        readyLockCoroutine = StartCoroutine(IReadyButtonLock(2f));

        MyProfile.Set_ReadyState(!MyProfile.IsReady);

        if(PhotonLogicHandler.IsMasterClient && MyProfile.IsReady)
        {
            allReadyCheckCoroutine = StartCoroutine(IAllReadyCheck());
        }
    }

    protected IEnumerator IReadyButtonLock(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        readyLockCoroutine = null;
    }

    protected IEnumerator IAllReadyCheck()
    {
        bool allReadyState;
        int CurrRoomMemberCount;

        while (MyProfile.IsReady && PhotonLogicHandler.IsMasterClient)
        {
            allReadyState = PhotonLogicHandler.Instance.IsAllReady();
            CurrRoomMemberCount = PhotonLogicHandler.CurrentRoomMemberCount;

            if (allReadyState && CurrRoomMemberCount == 2)
            {
                Managers.UI.popupCanvas.Open_SelectPopup(GoTo_BattleScene, UnReadyMyProfile, "게임을 시작하겠습니까?");
                
                break;
            }
            yield return null;
        }

        allReadyCheckCoroutine = null;
    }
}