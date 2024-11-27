using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth_Koth : MonoBehaviourPunCallbacks
{
    public bool isLocalInstance;
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public Slider HealthSlider;

    private void Start()
    {
        currentHealth = maxHealth;
    }
    [PunRPC]
    public void TakeDamage(int _damage, int targetViewID)
    {
        if (photonView.ViewID == targetViewID)
        {
            currentHealth -= _damage;
            HealthSlider.value = currentHealth;
            if (currentHealth <= 0)
            {
                //photonView.RPC("setisdead", RpcTarget.All);
                if (isLocalInstance && gameObject.tag == "RedPlayer")
                {
                    Debug.Log("Red Died");
                   
                    StartCoroutine(Respawn_aftersomeseconds("RedPlayer"));
                    //RoomManager_KothMode.Instance.Respawn_Red();
                   
                }
                else if (isLocalInstance && gameObject.tag == "BluePlayer")
                {
                    Debug.Log("Blue Died");
                    StartCoroutine(Respawn_aftersomeseconds("BluePlayer"));
                    //RoomManager_KothMode.Instance.Respawn_Blue();
                }
                Destroy(gameObject, 2.2f);
            }
        }
    }
    IEnumerator Respawn_aftersomeseconds(string tag)
    {
        yield return new WaitForSeconds(2f);
        if(tag== "RedPlayer")
        {
            RoomManager_KothMode.Instance.Respawn_Red();
            
        }
        else if(tag== "BluePlayer")
        {
            RoomManager_KothMode.Instance.Respawn_Blue();
           
        }
    }
    //[PunRPC]
    //public void setisdead()
    //{
    //    RoomManager_KothMode.Instance.isdead = true;
    //}
}
