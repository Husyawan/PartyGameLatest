using UnityEngine;
using Photon.Pun;
using System.Collections;

public class ArmCollisionHandler : MonoBehaviourPunCallbacks
{
    [SerializeField] private string staticParticlePrefabName; 
    [SerializeField] private string[] dynamicParticlePrefabNames;

   
    private PhotonView otherplayerPhotonView;
    public PhotonView LocalPhotonView;
    private void Start()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
       
            if (collision.gameObject.CompareTag("RedPlayer") || collision.gameObject.CompareTag("BluePlayer"))
            {
                otherplayerPhotonView = collision.gameObject.GetComponent<PhotonView>();
                Debug.Log("On Collision 2 " + collision.gameObject.tag);
               
                // Only the owner of the player object should decrease health
                if (otherplayerPhotonView.IsMine && !LocalPhotonView.IsMine)
                {
                    Debug.Log("On Collision 3 " + collision.gameObject.tag);
                    // Damage the player by 10
                    OnHit(gameObject.transform.position);
                    otherplayerPhotonView.RPC("TakeDamage", RpcTarget.All, 10, otherplayerPhotonView.ViewID);
                }
            }
      
    }
    public void OnHit(Vector3 hitPosition)
    {
        // Instantiate the static particle
        GameObject staticParticle = PhotonNetwork.Instantiate(staticParticlePrefabName, hitPosition, Quaternion.identity);

        // Select a random particle from the array
        string randomParticleName = dynamicParticlePrefabNames[Random.Range(0, dynamicParticlePrefabNames.Length)];

        // Instantiate the chosen dynamic particle
        GameObject dynamicParticle = PhotonNetwork.Instantiate(randomParticleName, hitPosition, Quaternion.identity);

        // Destroy both particles after a short delay
        StartCoroutine(DestroyAfterTime(staticParticle, 2f)); // Adjust the delay as needed
        StartCoroutine(DestroyAfterTime(dynamicParticle, 2f)); // Adjust the delay as needed
    }
    private IEnumerator DestroyAfterTime(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}
