using UnityEngine;
using System.Linq;

public class PlayerRoomTracker : MonoBehaviour
{
    public int currentRoomID;

    private int? firstOptionRoomChosen = null;

    void OnTriggerEnter(Collider other)
    {
        RoomTrigger room = other.GetComponent<RoomTrigger>();
        if (room == null) return;

        int targetRoom = room.roomID;
        int currentRoom = currentRoomID;

        //living room + kitchen is one big room
        float currentCleanliness = GameManager.Instance.GetCurrentCleanliness(); 
        if (targetRoom == 0 || targetRoom == 2)
        {
            currentRoomID = targetRoom;
            GameManager.Instance.UpdateUI();
            return;
        }

        //must clean living + kitchen to 50%
        if (currentCleanliness < 50f)
        {
            Debug.Log("you need to clean the living room/kitchen to 50% first: " + currentCleanliness);

            PlayerMovement pm = GetComponent<PlayerMovement>();
            if (pm != null && GameManager.Instance.enemy != null)
            {
                pm.BlockMovement(GameManager.Instance.enemy);

                EnemyWander enemyWander = GameManager.Instance.enemy.GetComponentInParent<EnemyWander>();
                if (enemyWander != null)
                {
                    enemyWander.StopAllCoroutines();
                    enemyWander.StartCoroutine(enemyWander.FacePlayer());
                }
            }

            return;
        }

        //choose first room to clean (bed or bath)
        if (firstOptionRoomChosen == null)
        {
            if (targetRoom == 1 || targetRoom == 3)
            {
                firstOptionRoomChosen = targetRoom;
            }
            else
            {
                Debug.Log("Invalid room selection");
                return;
            }
        }
        else
        {
            //check if first option room is at 50% before going to final room
            int finalRoom = (targetRoom == 1 || targetRoom == 3) ? targetRoom : -1;
            if (finalRoom != -1 && firstOptionRoomChosen != finalRoom)
            {
                if (currentCleanliness < 75f)
                {
                    Debug.Log("clean the first chosen room to 50% first!!");

                    PlayerMovement pm = GetComponent<PlayerMovement>();
                    if (pm != null && GameManager.Instance.enemy != null)
                    {
                        pm.BlockMovement(GameManager.Instance.enemy);

                        EnemyWander enemyWander = GameManager.Instance.enemy.GetComponentInParent<EnemyWander>();
                        if (enemyWander != null)
                        {
                            enemyWander.StopAllCoroutines();
                            enemyWander.StartCoroutine(enemyWander.FacePlayer());
                        }
                    }

                    return;
                }
            }
        }

        //allowed to enter
        currentRoomID = targetRoom;
        GameManager.Instance.UpdateUI();
    }
}
