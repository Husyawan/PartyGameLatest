using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomItem : MonoBehaviour
{
    public string roomToJoinOfItem;
    
    public void JoinRoom()
    {
        Lobby.Instance.JoinRoomByName(roomToJoinOfItem);
    }
}
