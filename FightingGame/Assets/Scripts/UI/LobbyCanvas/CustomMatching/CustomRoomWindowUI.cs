using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FGDefine;
using System;

public class CustomRoomWindowUI : MonoBehaviour, IRoomPostProcess
{
    [SerializeField] UserProfileUI masterProfile;
    [SerializeField] UserProfileUI slaveProfile;
    UserProfileUI MyProfile
    {
        get
        {
            if (PhotonLogicHandler.IsMasterClient)
                return masterProfile;
            else
                return slaveProfile;
        }
    }
    UserProfileUI YourProfile
    {
        get
        {
            if (PhotonLogicHandler.IsMasterClient)
                return slaveProfile;
            else
                return masterProfile;
        }
    }

    [SerializeField] Image currMapIamge;
    [SerializeField] Image nextMapIamge_Left;
    [SerializeField] Image nextMapIamge_Right;

    [SerializeField] Text roomNameText;
    [SerializeField] Text mapNameText;

    private bool isInit = false;
    public bool isRoomRegisting = false;
    public bool isStarted = false;

    Coroutine readyLockCoroutine;
    Coroutine allReadyCheckCoroutine;

    ENUM_MAP_TYPE currMap;
    public ENUM_MAP_TYPE CurrMap
    {
        get { return currMap; }
        private set { CurrmapInfoUpdateCallBack(value); }
    }

    private void OnEnable()
    {
        // 포톤콜백함수 등록
        PhotonLogicHandler.Instance.onEnterRoomPlayer -= SlaveClientEnterCallBack;
        PhotonLogicHandler.Instance.onLeftRoomPlayer -= SlaveClientExitCallBack;
        
        PhotonLogicHandler.Instance.onEnterRoomPlayer += SlaveClientEnterCallBack;
        PhotonLogicHandler.Instance.onLeftRoomPlayer += SlaveClientExitCallBack;
        
        this.RegisterRoomCallback();
    }

    private void OnDisable()
    {
        // 포톤콜백함수 해제
        PhotonLogicHandler.Instance.onEnterRoomPlayer -= SlaveClientEnterCallBack;
        PhotonLogicHandler.Instance.onLeftRoomPlayer -= SlaveClientExitCallBack;

        this.UnregisterRoomCallback();
    }

    #region CallBack, OnUpdateProperty 함수 (Server)
    /// <summary>
    /// 슬레이브 클라이언트가 방에 입장하면 불리는 콜백함수
    /// </summary>
    public void SlaveClientEnterCallBack(string nickname)
    {
        if (PhotonLogicHandler.IsMasterClient)
        {
            YourProfile.Set_UserNickname(nickname);
            Managers.Battle.Set_SlaveNickname(nickname);
        }

        PhotonLogicHandler.Instance.RequestEveryPlayerProperty();
    }

    /// <summary>
    /// 자신 외의 다른 클라이언트가 방을 나가면 불리는 콜백함수
    /// ( 이 경우, 자신이 무조건 마스터 클라이언트가 됨 )
    /// </summary>
    public void SlaveClientExitCallBack(string nickname)
    {
        slaveProfile.Clear();

        if(!masterProfile.isMine)
        {
            masterProfile.Clear();
            masterProfile.Init();
            masterProfile.Set_UserNickname(PhotonLogicHandler.CurrentMyNickname);
        }

        Debug.Log($"IsFullRoom : {PhotonLogicHandler.IsFullRoom}");

    }

    public void OnUpdateRoomProperty(CustomRoomProperty property)
    {
        if (PhotonLogicHandler.IsMasterClient || isStarted)
            return; // 나의 변경된 정보이거나 시작중이라면 리턴

        if (property.isStarted) // 게임 시작을 알림받음
        {
            isStarted = true;
            Managers.UI.popupCanvas.Play_FadeInEffect(Managers.UI.currCanvas.GetComponent<LobbyCanvas>().Open_FightingInfoWindow);
            return;
        }

        CurrMap = property.currentMapType;
    }

    public void OnUpdateRoomPlayerProperty(CustomPlayerProperty property)
    {
        if (property.isMasterClient == PhotonLogicHandler.IsMasterClient)
            return; // 나의 변경된 정보면 리턴

        Debug.Log("상대에게 정보를 받아 갱신합니다.");
        YourProfile.Set_Character(property.characterType);
        YourProfile.Set_ReadyState(property.isReady);

        Managers.Battle.Set_EnemyCharacterType(property.characterType);
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
        if (this.gameObject.activeSelf)
        {
            Debug.Log("커스텀 룸 윈도우가 이미 열려있습니다.");
        }
        else
        {
            this.gameObject.SetActive(true);
            Init();
        }

        Managers.Battle.Join_CustomRoom();
        Set_CurrRoomInfo();
    }

    public void Close()
    {
        if (!this.gameObject.activeSelf)
            return;

        isInit = false;
        Managers.Battle.Leave_CustomRoom();

        masterProfile.Clear();
        slaveProfile.Clear();

        this.gameObject.SetActive(false);
        Managers.UI.currCanvas.GetComponent<LobbyCanvas>().Close_CustomMatchingWindow();
    }

    public void CurrmapInfoUpdateCallBack(ENUM_MAP_TYPE _mapType)
    {
        if (PhotonLogicHandler.IsMasterClient)
            PhotonLogicHandler.Instance.ChangeMap(_mapType);

        currMap = _mapType;
        currMapIamge.sprite = Managers.Resource.Load<Sprite>($"Art/Sprites/Maps/{_mapType}");
        mapNameText.text = Managers.Battle.Get_MapNameDict(_mapType);

        int mapIndex = (int)_mapType - 1;
        if (mapIndex <= 0)
            mapIndex = (int)ENUM_MAP_TYPE.Max - 1;
        nextMapIamge_Left.sprite = Managers.Resource.Load<Sprite>($"Art/Sprites/Maps/{(ENUM_MAP_TYPE)mapIndex}");

        mapIndex = (int)_mapType + 1;
        if (mapIndex >= (int)ENUM_MAP_TYPE.Max)
            mapIndex = 0;
        nextMapIamge_Right.sprite = Managers.Resource.Load<Sprite>($"Art/Sprites/Maps/{(ENUM_MAP_TYPE)mapIndex}");
    }

    public void Set_CurrRoomInfo()
    {
        roomNameText.text = PhotonLogicHandler.CurrentRoomName;
        MyProfile.Set_UserNickname(PhotonLogicHandler.CurrentMyNickname);
        
        if (!PhotonLogicHandler.IsMasterClient)
            YourProfile.Set_UserNickname(PhotonLogicHandler.CurrentMasterClientNickname);
        else if(PhotonLogicHandler.CurrentRoomMemberCount == 2 && YourProfile.Get_UserNickname() == "")
            YourProfile.Set_UserNickname(Managers.Battle.Get_SlaveNickname());   
            
        CurrMap = PhotonLogicHandler.CurrentMapType;
    }

    public void GoTo_BattleScene()
    {
        if (!PhotonLogicHandler.IsMasterClient)
            return;

        if (PhotonLogicHandler.Instance.IsAllReady() && PhotonLogicHandler.CurrentRoomMemberCount == 2)
        {
            PhotonLogicHandler.Instance.UnReadyAll(); // 모두 준비해제 시키고
            PhotonLogicHandler.Instance.GameStart(); // 게임 시작을 알림
            isStarted = true;
            Managers.UI.popupCanvas.Play_FadeInEffect(Managers.UI.currCanvas.GetComponent<LobbyCanvas>().Open_FightingInfoWindow);
        }
        else
            Managers.UI.popupCanvas.Open_NotifyPopup("게임 시작에 실패했습니다.", UnReadyMyProfile);
    }

    public void UnReadyMyProfile()
    {
        MyProfile.Set_ReadyState(false);
    }

    public void ExitRoom()
    {
        bool isLeaveRoom = PhotonLogicHandler.Instance.TryLeaveRoom(Close);

        if(!isLeaveRoom)
        {
            Managers.UI.popupCanvas.Open_NotifyPopup("알 수 없는 에러\n나가기 실패");
        }
    }

    public void OnClick_ChangeMap(bool _isRight)
    {
        if (!PhotonLogicHandler.IsMasterClient)
            return;

        int _mapIndex = (int)CurrMap;

        if (_isRight)
        {
            _mapIndex += 1;
            if (_mapIndex >= (int)ENUM_MAP_TYPE.Max)
                _mapIndex = 0;
        }
        else
        {
            _mapIndex -= 1;
            if (_mapIndex <= 0)
                _mapIndex = (int)ENUM_MAP_TYPE.Max - 1;
        }

        CurrMap = (ENUM_MAP_TYPE)_mapIndex;
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
            if (allReadyCheckCoroutine != null)
                allReadyCheckCoroutine = null;

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