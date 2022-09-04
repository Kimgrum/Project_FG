using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Linq;
using System.Text;
using FGDefine;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum ENUM_RPC_TARGET
{
    All,
    MASTER,
    OTHER
}

public class BroadcastMethodAttribute : PunRPC { }
public class PhotonCustomType { }

[Serializable]
public class PhotonCustomType<T> : PhotonCustomType
{
    public static object Deserialize(Hashtable diskData)
    {
        Type t = typeof(T);
        var fieldInfos = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        object[] retObjs = new object[fieldInfos.Length];

        for (int i = 0; i < fieldInfos.Length; i++)
        {
            if (!diskData.ContainsKey(fieldInfos[i].Name))
                continue;

            retObjs[i] = diskData[fieldInfos[i].Name];
        }

        return retObjs;
    }

    public static Hashtable Serialize(object memoryData)
    {
        Type t = typeof(T);
        T data = (T)memoryData;

        Hashtable table = new Hashtable();
        var fieldInfos = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        foreach(var fieldInfo in fieldInfos)
        {
            if (fieldInfo.IsNotSerialized)
                continue;

            var val = fieldInfo.GetValue(data);
            var name = fieldInfo.Name;

            table.Add(name, val);
        }

        return table;
    }
}

public delegate void DisconnectCallBack(string cause);
public delegate void FailedCallBack(short returnCode, string message);

public partial class PhotonLogicHandler : MonoBehaviourPunCallbacks
{
    private static PhotonLogicHandler instance;
    public static PhotonLogicHandler Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject g = new GameObject("PhotonLogicHandler");
                instance = g.AddComponent<PhotonLogicHandler>();
                instance.Initialize();

                DontDestroyOnLoad(g);
            }

            return instance;
        }
    }

    private readonly string GameVersion = "1";
    private static Dictionary<int, PhotonView> photonViewDictionary = new Dictionary<int, PhotonView>();

    private Action _OnCreateRoom = null;
    private FailedCallBack _OnCreateRoomFailed = null;

    private Action _OnConnectedToMaster = null;
    private DisconnectCallBack _OnDisconnectedFromMaster = null;

    private Action _OnJoinRoom = null;
    private FailedCallBack _OnJoinRoomFailed = null;

    private Action _OnJoinLobby = null;
    private FailedCallBack _OnJoinLobbyFailed = null;



    private void OnDestroy()
    {
        _OnConnectedToMaster = null;
        _OnDisconnectedFromMaster = null;
        _OnJoinRoom = null;
        _OnJoinRoomFailed = null;
        _OnJoinLobby = null;
        _OnJoinLobbyFailed = null;
    }

    PhotonView view = null;

    private void Initialize()
    {
        view = gameObject.AddComponent<PhotonView>();

        if (view.ViewID == 0)
            PhotonNetwork.AllocateViewID(view);

        // SetPhotonPeerParameterType();
    }

    private void SetPhotonPeerParameterType()
    {
        var types = Assembly.GetAssembly(typeof(PhotonCustomType)).GetTypes();
        byte code = 1;

        foreach (var type in types)
        {
            if (type.IsSubclassOf(typeof(PhotonCustomType)))
            {
                PhotonPeer.RegisterType(type, ++code, SerializeHashtable, DeserializeHashtable);
            }
        }
    }

    private static short SerializeHashtable(StreamBuffer outStream, object customObject)
    {
        return 0;
    }

    private static object DeserializeHashtable(StreamBuffer inStream, short length)
    {
        return 0;
    }

    public void TryBroadcastMethod<T, TParam1, TParam2, TParam3, TParam4>(T owner, Action<TParam1, TParam2, TParam3> targetMethod, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, ENUM_RPC_TARGET targetType = ENUM_RPC_TARGET.All)
        where T : MonoBehaviourPun
    {
        if (!IsConnected)
            return;

        MethodInfo methodInfo = targetMethod.Method;
        if (!IsValidBroadcastMethod(owner, methodInfo))
            return;

        BroadcastMethod(owner, param1, param2, param3, param4, methodInfo, targetType);
    }

    public void TryBroadcastMethod<T, TParam1, TParam2, TParam3>(T owner, Action<TParam1, TParam2, TParam3> targetMethod, TParam1 param1, TParam2 param2, TParam3 param3, ENUM_RPC_TARGET targetType = ENUM_RPC_TARGET.All)
        where T : MonoBehaviourPun
    {
        if (!IsConnected)
            return;

        MethodInfo methodInfo = targetMethod.Method;
        if (!IsValidBroadcastMethod(owner, methodInfo))
            return;

        BroadcastMethod(owner, param1, param2, param3, methodInfo, targetType);
    }

    public void TryBroadcastMethod<T, TParam1, TParam2>(T owner, Action<TParam1, TParam2> targetMethod, TParam1 param1, TParam2 param2, ENUM_RPC_TARGET targetType = ENUM_RPC_TARGET.All)
        where T : MonoBehaviourPun
    {
        if (!IsConnected)
            return;

        MethodInfo methodInfo = targetMethod.Method;
        if (!IsValidBroadcastMethod(owner, methodInfo))
            return;

        BroadcastMethod(owner, param1, param2, methodInfo, targetType);
    }

    public void TryBroadcastMethod<T, TParam>(T owner, Action<TParam> targetMethod, TParam param, ENUM_RPC_TARGET targetType = ENUM_RPC_TARGET.All)
        where T : MonoBehaviourPun
    {
        if (!IsConnected)
            return;

        MethodInfo methodInfo = targetMethod.Method;
        if (!IsValidBroadcastMethod(owner, methodInfo))
            return;

        BroadcastMethod(owner, param, methodInfo, targetType);
    }

    /// <summary>
    /// 1. 넘기는 Action Method에 람다식은 허용되지 않습니다.
    /// 2. Method의 속성에 [BroadcastMethodAttribute]가 추가되어야 합니다.
    /// </summary>
    // [BroadcastMethodAttribute] 다음과 같이 함수 위에 추가
    public void TryBroadcastMethod<T>(T owner, Action targetMethod, ENUM_RPC_TARGET targetType = ENUM_RPC_TARGET.All)
        where T : MonoBehaviourPun
    {
        if (!IsConnected)
            return;

        MethodInfo methodInfo = targetMethod.Method;
        if (!IsValidBroadcastMethod<T>(owner, methodInfo))
            return;

        BroadcastMethod(owner, methodInfo, targetType);
    }

    private bool IsValidBroadcastMethod<T>(T owner, MethodInfo methodInfo)
        where T : MonoBehaviourPun
    {
        string methodName = methodInfo.Name;
        var ownerType = typeof(T);

        if (owner == null || owner.photonView == null)
        {
            Debug.LogError("동기화될 객체가 없거나 네트워킹 가능 상태가 아닙니다. 객체의 상태를 확인해주세요.");
            return false;
        }
        else if (ownerType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) == null)
        {
            Debug.LogError("넘기는 Action Method에 람다식은 허용되지 않습니다. 객체 내부에 Method를 구현 후 인자로 넘겨주세요.");
            return false;
        }
        else if (!methodInfo.IsDefined(typeof(BroadcastMethodAttribute)))
        {
            Debug.LogError("Broadcast할 메소드에 [BroadcastMethodAttribute] 속성이 없습니다. 추가해주세요.");
            return false;
        }

        return true;
    }

    private RpcTarget GetRPCTarget(ENUM_RPC_TARGET targetType)
    {
        RpcTarget RPCTargetType = RpcTarget.All;

        switch (targetType)
        {
            case ENUM_RPC_TARGET.All:
                RPCTargetType = RpcTarget.AllBuffered;
                break;

            case ENUM_RPC_TARGET.MASTER:
                RPCTargetType = RpcTarget.MasterClient;
                break;

            case ENUM_RPC_TARGET.OTHER:
                RPCTargetType = RpcTarget.OthersBuffered;
                break;

            default:
                break;
        }

        return RPCTargetType;
    }

    private void BroadcastMethod<T>(T owner, MethodInfo methodInfo, ENUM_RPC_TARGET targetType = ENUM_RPC_TARGET.All)
        where T : MonoBehaviourPun
    {
        var RPCTargetType = GetRPCTarget(targetType);
        owner.photonView.RPC(methodInfo.Name, RPCTargetType);
    }

    private void BroadcastMethod<T, TParam>(T owner, TParam param, MethodInfo methodInfo, ENUM_RPC_TARGET targetType = ENUM_RPC_TARGET.All)
        where T : MonoBehaviourPun
    {
        var RPCTargetType = GetRPCTarget(targetType);
        owner.photonView.RPC(methodInfo.Name, RPCTargetType, param);
    }

    private void BroadcastMethod<T, T1, T2>(T owner, T1 param1, T2 param2, MethodInfo methodInfo, ENUM_RPC_TARGET targetType = ENUM_RPC_TARGET.All)
        where T : MonoBehaviourPun
    {
        var RPCTargetType = GetRPCTarget(targetType);
        owner.photonView.RPC(methodInfo.Name, RPCTargetType, param1, param2);
    }

    private void BroadcastMethod<T, T1, T2, T3>(T owner, T1 param1, T2 param2, T3 param3, MethodInfo methodInfo, ENUM_RPC_TARGET targetType = ENUM_RPC_TARGET.All)
        where T : MonoBehaviourPun
    {
        var RPCTargetType = GetRPCTarget(targetType);
        owner.photonView.RPC(methodInfo.Name, RPCTargetType, param1, param2, param3);
    }

    private void BroadcastMethod<T, T1, T2, T3, T4>(T owner, T1 param1, T2 param2, T3 param3, T4 param4, MethodInfo methodInfo, ENUM_RPC_TARGET targetType = ENUM_RPC_TARGET.All)
        where T : MonoBehaviourPun
    {
        var RPCTargetType = GetRPCTarget(targetType);
        owner.photonView.RPC(methodInfo.Name, RPCTargetType, param1, param2, param3, param4);
    }

    private void BroadcastMethod<T, T1, T2, T3, T4, T5>(T owner, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, MethodInfo methodInfo, ENUM_RPC_TARGET targetType = ENUM_RPC_TARGET.All)
        where T : MonoBehaviourPun
    {
        var RPCTargetType = GetRPCTarget(targetType);
        owner.photonView.RPC(methodInfo.Name, RPCTargetType, param1, param2, param3, param4, param5);
    }


    #region Register 계열 외부 함수, MonoBehaviourPhoton을 등록, 파기할 때 사용
    public static int Register(PhotonView view)
    {
        if (view == null)
            return 0;

        if (view.ViewID == 0)
            PhotonNetwork.AllocateViewID(view);

        if (view.ViewID == 0)
		{
            Debug.LogError("유효한 포톤 뷰 객체가 아님 ㅅㅂ 암튼 아님 문의줘보셈");
            return 0;
        }
            
        if (!photonViewDictionary.ContainsKey(view.ViewID))
        {
            photonViewDictionary.Add(view.ViewID, view);
        }
        else
        {
            Debug.LogWarning($"같은 MonoBehaviourPhoton 오브젝트를 추가하려 들었음. {view.ViewID}");
            return 0;
        }

        return view.ViewID;
    }

    public static int Unregister(int viewID)
    {
        PhotonView view = null;

        if (photonViewDictionary.TryGetValue(viewID, out view))
        {
            view.ViewID = 0;
            photonViewDictionary.Remove(viewID);
        }
        else
        {
            Debug.LogWarning($"등록되지 않은 MonoBehaviourPhoton 오브젝트를 제거하려 들었음. {viewID}");
        }

        return 0;
    }

    #endregion

    #region Try 계열 외부 함수

    /// <summary>
    /// 마스터 서버에 접속을 시도합니다. 해당 함수가 성공된 상태여야 다른 네트워크 함수를 사용할 수 있습니다.
    /// </summary>
    /// <param name="_OnConnectedToMaster"> 마스터 서버 접속 성공 시 불리는 콜백 </param>
    /// <param name="_OnDisconnectedFromMaster"> 마스터 서버 접속이 실패했거나, 접속이 끊어졌을 때 불리는 콜백 </param>
    /// <returns> 성공 여부 </returns>

    public bool TryConnectToMaster(Action _OnConnectedToMaster = null, DisconnectCallBack _OnDisconnectedFromMaster = null)
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.LogError("이미 마스터 서버에 연결되어 있는 상태에서 연결을 시도했습니다.");
            return false;
        }
            
        this._OnConnectedToMaster = _OnConnectedToMaster;
        this._OnDisconnectedFromMaster = _OnDisconnectedFromMaster;

        PhotonNetwork.GameVersion = GameVersion;
        return PhotonNetwork.ConnectUsingSettings(); // 마스터 서버 접속 시도
    }

    /// <summary>
    /// 랜덤 룸에 접속을 시도합니다.
    /// </summary>
    /// <param name="_OnJoinRoom"> 룸 접속에 성공했을 때 불리는 콜백 </param>
    /// <param name="_OnJoinRoomFailed">룸 접속에 실패했을 때 불리는 콜백 </param>
    /// <returns> 성공 여부 </returns>

    public bool TryJoinRandomRoom(Action _OnJoinRoom, FailedCallBack _OnJoinRoomFailed, string nickname)
    {
        Debug.Log($"랜덤 룸에 접속을 시도합니다.");

        this._OnJoinRoom = _OnJoinRoom;
        this._OnJoinRoomFailed = _OnJoinRoomFailed;

        PhotonNetwork.LocalPlayer.NickName = nickname;

        ExitGames.Client.Photon.Hashtable optionTable = new ExitGames.Client.Photon.Hashtable()
        {
           
        };

        return PhotonNetwork.JoinRandomRoom(optionTable, 0);
    }

    public bool TryJoinLobby(Action onSuccess = null, FailedCallBack onfailed = null)
    {
        this._OnJoinLobby = onSuccess;
        this._OnJoinLobbyFailed = onfailed;

        return PhotonNetwork.JoinLobby();
    }


    public bool IsConnectedAndReady() => PhotonNetwork.IsConnectedAndReady;
    public bool IsMasterServer() => PhotonNetwork.Server == ServerConnection.MasterServer;
    public bool IsInLobby() => PhotonNetwork.InLobby;
    public bool IsInRoom() => PhotonNetwork.InRoom;

    /// <summary>
    /// 모든 클라이언트가 준비가 되었음을 체크합니다.
    /// </summary>
    /// <returns></returns>
    public bool IsAllReady() 
    {
        var players = PhotonNetwork.PlayerList;
        return players.All(p => p.CustomProperties.ContainsKey("Ready") && (bool)p.CustomProperties["Ready"]);
    }

    /// <summary>
    /// 방장에게 준비가 되었음을 알립니다.
    /// </summary>
    public void Ready() 
    {
        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        hash["Ready"] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
    public void UnReady()
    {
        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        hash["Ready"] = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }


    /// <summary>
    /// 방장이 해당 함수를 호출하게 되면, 방의 멤버가 모두 씬이 동기화된 채로 이동하게 됨
    /// </summary>
    /// <param name="sceneName"></param>

    public bool TrySceneLoadWithRoomMember(ENUM_SCENE_TYPE sceneType)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("마스터 클라이언트가 아닌 경우 부를 수 없는 함수입니다.");
            return false;
        }

        PhotonNetwork.LoadLevel(sceneType.ToString());
        return true;
    }

    /// <summary>
    /// 방을 새로 만듦
    /// </summary>
    /// <param name="roomName"> 방의 이름 </param>
    /// <param name="maxPlayerCount"> 방 인원 최대 수 </param>
    /// <returns> 성공 여부 </returns>

    public bool TryCreateRoom(Action OnCreateRoom = null, FailedCallBack OnCreateRoomFailed = null, string roomName = "이름 없음", string playerNickname = "", int maxPlayerCount = 2)
    {
        this._OnCreateRoom = OnCreateRoom;
        this._OnCreateRoomFailed = OnCreateRoomFailed;

        PhotonNetwork.LocalPlayer.NickName = playerNickname;

        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = (byte)maxPlayerCount };
        // roomOptions.CustomRoomProperties.Add(typeof(ENUM_MAP_TYPE).ToString(), "아직 맵 이름 쓰지마~~");

        return PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    /// <summary>
    /// 프리팹을 룸 내 모든 인원에 대하여 생성
    /// </summary>
    /// <param name="prefabPath">리소스 폴더를 기준으로 한 프리팹 경로</param>
    /// <param name="pos"></param>
    /// <param name="quaternion"></param>

    public GameObject TryInstantiate(string prefabPath, Vector3 pos = default, Quaternion quaternion = default)
    {
        return PhotonNetwork.Instantiate(prefabPath, pos, quaternion);
    }

    #endregion

    #region 포톤 자체 콜백 - 가급적 건드리지 마시오.

    /// <summary>
    /// 마스터 서버 접속 시도 성공 시 콜백
    /// </summary>

    public override void OnConnectedToMaster() 
    {
        Debug.Log("마스터 서버에 성공적으로 접속했습니다.");
        _OnConnectedToMaster?.Invoke();
    }

    /// <summary>
    /// 접속 중 접속이 끊어졌을 때 불리는 콜백
    /// </summary>
    /// <param name="cause"></param>

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"마스터 서버로부터 접속이 끊어졌습니다. 사유 : {cause}");
        _OnDisconnectedFromMaster?.Invoke(cause.ToString()); 
    }

    /// <summary>
    /// 방 만들기 성공 시 콜백
    /// </summary>

	public override void OnCreatedRoom()
	{
        _OnCreateRoom?.Invoke();
    }

    /// <summary>
    /// 방 만들기 실패 시 콜백
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
        _OnCreateRoomFailed?.Invoke(returnCode, message);
	}

	/// <summary>
	/// 로비에 접속되었을 때 불리는 콜백
	/// </summary>

	public override void OnJoinedLobby()
	{
        Debug.Log("로비 접속 성공");
        _OnJoinLobby?.Invoke();
    }

    /// <summary>
    /// 로비에 접속이 실패했을 때 불리는 콜백
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
        Debug.LogError($"{returnCode} - {message} ");
        _OnJoinLobbyFailed?.Invoke(returnCode, message);
    }

	/// <summary>
	/// Room에 참가했을 때 불리는 콜백
	/// </summary>
	/// 

	public override void OnJoinedRoom()
    {
        Debug.Log($"룸에 성공적으로 접속하였습니다.");
#if UNITY_EDITOR
        Info();
#endif
        PhotonNetwork.AutomaticallySyncScene = true;
        _OnJoinRoom?.Invoke();
    }

    /// <summary>
    /// Room 참가에 실패했을 때 불리는 콜백
    /// </summary> 
    /// <param name="returnCode"></param> 
    /// <param name="message"></param>

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"룸 접속에 실패하였습니다. 코드 : {returnCode}, 사유 : {message}");
        _OnJoinRoomFailed?.Invoke(returnCode, message);
    }

	public override void OnConnected()
	{
		base.OnConnected();
	}

	public override void OnLeftRoom()
	{
        PhotonNetwork.AutomaticallySyncScene = false;
    }

	public override void OnLeftLobby()
	{
		
	}

    /// <summary>
    /// 방 리스트 정보 업데이트
    /// </summary>
    /// <param name="roomList"></param>

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		
	}

	/// <summary>
	/// 방장 바뀜
	/// </summary>
	/// <param name="newMasterClient"></param>

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		
	}

    /// <summary>
    /// 방 설정 변경
    /// </summary>
    /// <param name="targetPlayer"></param>
    /// <param name="changedProps"></param>

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
	{
		
	}

	/// <summary>
	/// 유저 입장
	/// </summary>
	/// <param name="newPlayer"></param>
	public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
		{
            PhotonNetwork.CurrentRoom.IsOpen = false;
		}
    }

    /// <summary>
    /// 유저 나감
    /// </summary>
    /// <param name="otherPlayer"></param>

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

    public string Info()
    {
        StringBuilder str = new StringBuilder();

        if (PhotonNetwork.InRoom)
        {
            str.Append($"현재 방 이름 : {PhotonNetwork.CurrentRoom.Name} \n");
            str.Append($"현재 방 인원 수 : {PhotonNetwork.CurrentRoom.PlayerCount} \n");
            str.Append($"현재 방 최대 인원 수 : {PhotonNetwork.CurrentRoom.MaxPlayers} \n");
            str.Append($"{PhotonNetwork.CurrentRoom.MasterClientId}, {PhotonNetwork.LocalPlayer.ActorNumber} 두 개가 같으면 나는 마스터");
        }
        else
        {
            str.Append($"접속한 인원 수 : {PhotonNetwork.CountOfPlayers} \n");
            str.Append($"방 개수 : {PhotonNetwork.CountOfRooms} \n");
            str.Append($"모든 방에 있는 인원 수 : {PhotonNetwork.CountOfPlayersInRooms} \n");
            str.Append($"로비에 있는가? : { PhotonNetwork.InLobby} \n");
            str.Append($"연결이 됐는가? : { PhotonNetwork.IsConnected} \n");
        }

        print(str);
        return str.ToString();
    }

    #endregion
}
