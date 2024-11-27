using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;

public class Attach : MonoBehaviourPunCallbacks
{
    private GroundCheck gc;
    private InputSystem_Actions inputSystem;

    private Rigidbody _rb;
    private Collider playerCollider;

    public bool canRetach;

    [SerializeField] public float customGravity = -9.81f;
    [SerializeField] AudioClip magnetRepel;
    AudioSource _audioSource;

    //private Vector2 lookInput;
    //private Vector3 currentRotation;
    //private Vector3 rotationSmoothVelocity;
    private float yRot = 0f;
    private float xRot = 0f;
    public float ogJumpForce;

    private bool customGravityActive = true;
    public GameObject head,head2,head3,head4,head5, torso, r_Leg, l_Leg, r_Arm, l_Arm, parent;
    private int partsReattachedCount = 0;
    private int totalParts = 10;
    private Vector3 referencePosition;
    public float detachCooldown = 2.0f; // Time in seconds between detachments
    private float lastDetachTime = -2.0f; // Last time a detach was allowed


    public bool isDetached = false;
    public bool _isL_ArmDetached = false;
    public bool _isR_ArmDetached = false;
    public bool _isL_LegDetached = false;
    public bool _isR_LegDetached = false;

    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Quaternion> originalRotations = new Dictionary<GameObject, Quaternion>();
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Transform[]> originalBones = new Dictionary<GameObject, Transform[]>();
    private Dictionary<GameObject, Transform> originalRootBones = new Dictionary<GameObject, Transform>();
    private Dictionary<GameObject, Mesh> originalMeshes = new Dictionary<GameObject, Mesh>();
    private Dictionary<GameObject, ColliderData> originalCollidersData = new Dictionary<GameObject, ColliderData>();

    [System.Serializable]
    public struct ColliderData
    {
        public Vector3 size;
        public Vector3 center;
        public bool isTrigger;

        public ColliderData(BoxCollider boxCollider)
        {
            size = boxCollider.size;
            center = boxCollider.center;
            isTrigger = boxCollider.isTrigger;
        }
    }

    private void Awake()
    {
        
        inputSystem = new InputSystem_Actions();

        _rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
      
        transform.rotation = Quaternion.Euler(0, 0, 0);
       
        playerCollider.enabled = false;

        StoreOriginalTransforms(head);
        StoreOriginalTransforms(head2);
        StoreOriginalTransforms(head3);
        StoreOriginalTransforms(head4);
        StoreOriginalTransforms(head5);
        StoreOriginalTransforms(torso);
        StoreOriginalTransforms(r_Leg);
        StoreOriginalTransforms(l_Leg);
        StoreOriginalTransforms(r_Arm);
        StoreOriginalTransforms(l_Arm);
    }

    private void StoreOriginalTransforms(GameObject part)
    {
        originalPositions[part] = part.transform.localPosition;
        originalRotations[part] = part.transform.localRotation;
        originalScales[part] = part.transform.localScale;

        SkinnedMeshRenderer renderer = part.GetComponent<SkinnedMeshRenderer>();
        if (renderer != null)
        {
            originalBones[part] = renderer.bones;
            originalRootBones[part] = renderer.rootBone;
            originalMeshes[part] = renderer.sharedMesh;
        }
    }

    public override void OnEnable()
    {
        if (!photonView.IsMine) return;
        inputSystem.Player.Detach.performed += On_Detach;
        inputSystem.Player.Detach.Enable();
        inputSystem.Player.Reattach.performed += On_Reattach;
        inputSystem.Player.Reattach.Enable();
        inputSystem.Player.DetachHead.performed += DetachHead;
        inputSystem.Player.DetachHead.Enable();
        inputSystem.Player.DetachTorso.performed += DetachTorso;
        inputSystem.Player.DetachTorso.Enable();
        inputSystem.Player.DetachRightLeg.performed += DetachRightLeg;
        inputSystem.Player.DetachRightLeg.Enable();
        inputSystem.Player.DetachLeftLeg.performed += DetachLeftLeg;
        inputSystem.Player.DetachLeftLeg.Enable();
        inputSystem.Player.DetachRightArm.performed += DetachRightArm;
        inputSystem.Player.DetachRightArm.Enable();
        inputSystem.Player.DetachLeftArm.performed += DetachLeftArm;
        inputSystem.Player.DetachLeftArm.Enable();
        inputSystem.Player.ShootRightArm.performed += ShootRightArm;
        inputSystem.Player.ShootRightArm.Enable();
        inputSystem.Player.ShootLeftArm.performed += ShootLeftArm;
        inputSystem.Player.ShootLeftArm.Enable();
        inputSystem.Player.ReattachLeftArm.performed += ReattachLeftArm;
        inputSystem.Player.ReattachLeftArm.Enable();
        inputSystem.Player.ReattachRightArm.performed += ReattachRightArm;
        inputSystem.Player.ReattachRightArm.Enable();
    }
    // Function to detach the head
    public void DetachHead(InputAction.CallbackContext context)
    {
        if (photonView.IsMine && Time.time >= lastDetachTime + detachCooldown)
        {
            lastDetachTime = Time.time;
            Debug.Log("Working on local ");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Head");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Head2");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Head3");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Head4");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Head5");
        }
    }

    // Function to detach the torso
    public void DetachTorso(InputAction.CallbackContext context)
    {
        if (photonView.IsMine && Time.time >= lastDetachTime + detachCooldown)
        {
            lastDetachTime = Time.time;
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Torso");
        }
    }

    // Function to detach the right leg
    public void DetachRightLeg(InputAction.CallbackContext context)
    {
        if (photonView.IsMine && !_isR_LegDetached && Time.time >= lastDetachTime + detachCooldown)
        {
            lastDetachTime = Time.time;
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "RightLeg");
            photonView.RPC("RPC_SetBoolAccordingtoPart", RpcTarget.All, "RightLeg",true);
        }
    }

    // Function to detach the left leg
    public void DetachLeftLeg(InputAction.CallbackContext context)
    {
        if (photonView.IsMine && !_isL_LegDetached && Time.time >= lastDetachTime + detachCooldown)
        {
            lastDetachTime = Time.time;
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "LeftLeg");
            photonView.RPC("RPC_SetBoolAccordingtoPart", RpcTarget.All, "LeftLeg", true);
        }
    }

    // Function to detach the right arm
    public void DetachRightArm(InputAction.CallbackContext context)
    {
        if (photonView.IsMine&&!_isR_ArmDetached && Time.time >= lastDetachTime + detachCooldown)
        {
            lastDetachTime = Time.time;
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "RightArm");
            photonView.RPC("RPC_SetBoolAccordingtoPart", RpcTarget.All, "RightArm", true);
        }
    }

    // Function to detach the left arm
    public void DetachLeftArm(InputAction.CallbackContext context)
    {
        if (photonView.IsMine && !_isL_ArmDetached && Time.time >= lastDetachTime + detachCooldown)
        {
            lastDetachTime = Time.time;
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "LeftArm");
            photonView.RPC("RPC_SetBoolAccordingtoPart", RpcTarget.All, "LeftArm", true);
        }
    }

    // Function to shoot the right arm
    public void ShootRightArm(InputAction.CallbackContext context )
    {
        if (photonView.IsMine && !_isR_ArmDetached && Time.time >= lastDetachTime + detachCooldown)
        {
            lastDetachTime = Time.time;
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "RightArm");
            photonView.RPC("RPC_ShootRightArm_All", RpcTarget.All);
            photonView.RPC("RPC_SetBoolAccordingtoPart", RpcTarget.All, "RightArm", true);
        }
    }

    // Function to shoot the left arm
    public void ShootLeftArm(InputAction.CallbackContext context)
    {
        if (photonView.IsMine && !_isL_ArmDetached && Time.time >= lastDetachTime + detachCooldown) 
        {
            lastDetachTime = Time.time;
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "LeftArm");
            photonView.RPC("RPC_ShootLeftArm_All", RpcTarget.All);
            photonView.RPC("RPC_SetBoolAccordingtoPart", RpcTarget.All, "LeftArm", true);
        }
    }
    [PunRPC]
    private void RPC_ShootRightArm_All()
    {
        Rigidbody rb = r_Arm.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * 5000f);  // Adjust force as needed
        }
    }
    [PunRPC]
    private void RPC_ShootLeftArm_All()
    {
        Rigidbody rb = l_Arm.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * 5000f);
        }
    }
    // Function to reattach the left arm
    public void ReattachLeftArm(InputAction.CallbackContext context)
    {
        if (photonView.IsMine && _isL_ArmDetached)
        {
            photonView.RPC("RPC_ReattachPart_LeftArm", RpcTarget.All);
            photonView.RPC("RPC_SetBoolAccordingtoPart", RpcTarget.All, "LeftArm", false);
            _audioSource.PlayOneShot(magnetRepel);
        }
    }

    // Function to reattach the right arm
    public void ReattachRightArm(InputAction.CallbackContext context)
    {
        if (photonView.IsMine && _isR_ArmDetached)
        {
            photonView.RPC("RPC_ReattachPart_RightArm", RpcTarget.All);
            photonView.RPC("RPC_SetBoolAccordingtoPart", RpcTarget.All, "RightArm", false);
            _audioSource.PlayOneShot(magnetRepel);
        }
    }
    public override void OnDisable()
    {
        if (!photonView.IsMine) return;
        inputSystem.Player.Detach.Disable();
        inputSystem.Player.Reattach.Disable();
        inputSystem.Player.DetachHead.Disable();
        inputSystem.Player.DetachTorso.Disable();
        inputSystem.Player.DetachRightLeg.Disable();
        inputSystem.Player.DetachLeftLeg.Disable();
        inputSystem.Player.DetachRightArm.Disable();
        inputSystem.Player.DetachLeftArm.Disable();
        inputSystem.Player.ReattachLeftArm.Disable();
        inputSystem.Player.ReattachRightArm.Disable();
    }

    public void On_Detach(InputAction.CallbackContext context)
    {
        if (!isDetached && photonView.IsMine && Time.time >= lastDetachTime + detachCooldown)
        {
            lastDetachTime = Time.time;
            referencePosition = transform.position;

            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Head");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Head2");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Head3");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Head4");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Head5");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "Torso");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "RightLeg");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "LeftLeg");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "RightArm");
            photonView.RPC("DetachPart_RPC", RpcTarget.All, "LeftArm");

            playerCollider.enabled = true;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
            isDetached = true;
            customGravityActive = false;
        }
    }

    [PunRPC]
    private void DetachPart_RPC(string partName)
    {
        GameObject part = GetPartByName(partName);
        if (part != null)
        {
            DetachPart(part);
        }
    }
    [PunRPC]
    private void RPC_SetBoolAccordingtoPart(string partName,bool isChoice)
    {
        switch (partName)
        {
            case "RightLeg":
                _isR_LegDetached = isChoice;
            break;

            case "LeftLeg":
                _isL_LegDetached = isChoice;
            break;
            case "RightArm":
                _isR_ArmDetached = isChoice;
            break;
            case "LeftArm":
                _isL_ArmDetached = isChoice;
            break;
            default:
                break;
        }
    }
    [PunRPC]
    private void RPC_RetachAllParts(bool isChoice)
    {
       
                _isR_LegDetached = isChoice;
       
                _isL_LegDetached = isChoice;
        
                _isR_ArmDetached = isChoice;
         
                _isL_ArmDetached = isChoice;
          
    }
    [PunRPC]
    private void ReattachPart_RPC(bool isChoice)
    {

        _isR_LegDetached = isChoice;

        _isL_LegDetached = isChoice;

        _isR_ArmDetached = isChoice;

        _isL_ArmDetached = isChoice;

    }
    private GameObject GetPartByName(string partName)
    {
        switch (partName)
        {
            case "Head": return head;
            case "Head2": return head2;
            case "Head3": return head3;
            case "Head4": return head4;
            case "Head5": return head5;
            case "Torso": return torso;
            case "RightLeg": return r_Leg;
            case "LeftLeg": return l_Leg;
            case "RightArm": return r_Arm;
            case "LeftArm": return l_Arm;
            default: return null;
        }
    }

    private void DetachPart(GameObject part)
    {
        if (part == null) return;

        canRetach = true;

        // Store the world scale before detaching (used for the collider only)
        Vector3 worldScale = part.transform.lossyScale;

        // Detach part from parent without affecting its local transformation
        part.transform.SetParent(null); // Keeps the world transformation

        // Adjust only the collider, not the object's scale
        BoxCollider partCollider = part.GetComponent<BoxCollider>();
        if (partCollider != null)
        {
            // Scale the collider's size and center based on world scale
            //partCollider.size = Vector3.Scale(partCollider.size, worldScale);
            //partCollider.center = Vector3.Scale(partCollider.center, worldScale);

            Debug.Log($"After Detach - Collider Size: {partCollider.size}, Collider Center: {partCollider.center}");
        }

        // SkinnedMeshRenderer baking
        SkinnedMeshRenderer skinnedMesh = part.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMesh != null)
        {
            Mesh bakedMesh = new Mesh();
            skinnedMesh.BakeMesh(bakedMesh);
            MeshFilter meshFilter = part.AddComponent<MeshFilter>();
            meshFilter.mesh = bakedMesh;
            MeshRenderer meshRenderer = part.AddComponent<MeshRenderer>();
            meshRenderer.materials = skinnedMesh.materials;
            Destroy(skinnedMesh);
        }

        // Add Rigidbody for physics interaction
        Rigidbody rb = part.AddComponent<Rigidbody>();
        rb.mass = 1f;
    }

    public void On_Reattach(InputAction.CallbackContext context)
    {
        if (isDetached || canRetach && photonView.IsMine)
        {
            photonView.RPC("ReattachAllParts_RPC", RpcTarget.All);
            photonView.RPC("RPC_RetachAllParts", RpcTarget.All,false);
        }
    }

    [PunRPC]
    private void ReattachAllParts_RPC()
    {
        StartCoroutine(ShakeAndReattach(head));
        StartCoroutine(ShakeAndReattach(head2));
        StartCoroutine(ShakeAndReattach(head3));
        StartCoroutine(ShakeAndReattach(head4));
        StartCoroutine(ShakeAndReattach(head5));
        StartCoroutine(ShakeAndReattach(torso));
        StartCoroutine(ShakeAndReattach(r_Leg));
        StartCoroutine(ShakeAndReattach(l_Leg));
        StartCoroutine(ShakeAndReattach(r_Arm));
        StartCoroutine(ShakeAndReattach(l_Arm));

        canRetach = false;
        isDetached = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        playerCollider.enabled = false;
        customGravityActive = true;

    }
    [PunRPC]
    private void RPC_ReattachPart_LeftArm()
    {

        StartCoroutine(ShakeAndReattach(l_Arm));
        canRetach = false;
    }
    [PunRPC]
    private void RPC_ReattachPart_RightArm()
    {

        StartCoroutine(ShakeAndReattach(r_Arm));
        canRetach = false;
       
    }
    private IEnumerator ShakeAndReattach(GameObject part)
    {  
        Rigidbody partRb = part.GetComponent<Rigidbody>();
        if (partRb != null)
        {
            Destroy(partRb);
        }
        part.transform.SetParent(parent.transform);
        float reattachSpeed = 5f;
        float rotationSpeed = 5f;
        float reattachDistanceThreshold = 0.1f;
        while (Vector3.Distance(part.transform.localPosition, originalPositions[part]) > reattachDistanceThreshold)
        {
            part.transform.localPosition = Vector3.Lerp(part.transform.localPosition, originalPositions[part], reattachSpeed * Time.deltaTime);
            part.transform.localRotation = Quaternion.Slerp(part.transform.localRotation, originalRotations[part], rotationSpeed * Time.deltaTime);
            yield return null;
        }
        part.transform.localPosition = originalPositions[part];
        part.transform.localRotation = originalRotations[part];
        part.transform.localScale = originalScales[part];
        MeshRenderer meshRenderer = part.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Destroy(meshRenderer);
        }
        MeshFilter meshFilter = part.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Destroy(meshFilter);
        }
        SkinnedMeshRenderer skinnedMeshRenderer = part.AddComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
        {
            skinnedMeshRenderer.sharedMesh = originalMeshes[part];
            skinnedMeshRenderer.bones = originalBones[part];
            skinnedMeshRenderer.rootBone = originalRootBones[part];
        }
    }
}
