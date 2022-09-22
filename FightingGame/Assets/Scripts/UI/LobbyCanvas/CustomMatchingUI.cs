using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomMatchingUI : MonoBehaviour
{
    public CustomRoomListUI customRoomList;
    public CustomRoomWindowUI customRoomWindow;

    private void OnEnable()
    {
        customRoomList.RegisterLobbyCallback();
        OnClick_GetLobbyList();
    }

    private void OnDisable()
    {
        customRoomList.UnRegister_LobbyCallback();
    }

    public void OnClick_GetLobbyList()
    {
        customRoomList.Get_CustomRoomList();
    }
}
