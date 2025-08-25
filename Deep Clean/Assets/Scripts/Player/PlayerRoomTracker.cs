using UnityEngine;

public class PlayerRoomTracker : MonoBehaviour
{
    public int currentRoomID;

    void OnTriggerEnter(Collider other)
    {
        RoomTrigger room = other.GetComponent<RoomTrigger>();
        if (room != null)
        {
            currentRoomID = room.roomID;
        }
    }
}
