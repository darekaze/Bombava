using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
    public GameObject BulletPrefab;
    public Transform BulletSpawn;
    public Bomb Bomb;

    private Transform Canvas;
    public Animator CharacterAnimator;
    
    //public string PlayerName;
    [SyncVar]
    public int PlayerId;
    [SyncVar]
    public float Score;
    [SyncVar]
    public int Deaths;

    public bool _isDead = false;
    [SyncVar]
    public bool _playerLocked = false;
    [SyncVar]
    public Color _playerColor;

    public bool _canMove = true;
    public bool _disabled = false;

    private Camera cam;
    private float _passBombRadius = 2;
    private float movementMultiplier = 1;
    private bool _canPassBomb = true;

    Coroutine _pickBallRoutine;
    Coroutine _throwBallRoutine;
    Coroutine _getHitRoutine;

    MapManager _map;
    float _camMinX;
    float _camMaxX;
    float _camMinY;
    float _camMaxY;
    float _playerMinX;
    float _playerMaxX;
    float _playerMinY;
    float _playerMaxY;

    void Start() {

    }

    void Update() {
        if (!isLocalPlayer || _disabled) {
            return;
        }
        if (_canMove) {
            //MOVEMENT (movement is synced automatically)
            float horizontalAxis = Input.GetAxis("Horizontal");
            float verticalAxis = Input.GetAxis("Vertical");
            Vector3 movementVector = new Vector3(horizontalAxis, verticalAxis, 0).normalized;
            var resultVector = transform.position + movementVector * Time.deltaTime * 2 * movementMultiplier;
            //if (resultVector.x < _playerMaxX && resultVector.x > _playerMinX && resultVector.y < _playerMaxY && resultVector.y > _playerMinY) {
            transform.Translate(movementVector * Time.deltaTime * 2 * movementMultiplier);
            //}
            if (movementVector.x < 0) CmdChangeAnimation(1);
            else if (movementVector.x > 0) CmdChangeAnimation(2);
            else if (movementVector.y < 0) CmdChangeAnimation(3);
            else if (movementVector.y > 0) CmdChangeAnimation(4);
            else CmdChangeAnimation(0);
        }
        //PASS THE BOMB (creates prefab of "wave" that transmits the bomb)
        if (Input.GetKeyDown(KeyCode.Space) && !_isDead && Bomb != null && _canPassBomb) {
            StartCoroutine(EnablePassTheBomb());
            CmdPassBomb();
        }
        var targetPos = transform.position;
        transform.position = new Vector3(Mathf.Clamp(targetPos.x, _playerMinX, _playerMaxX), Mathf.Clamp(targetPos.y, _playerMinY, _playerMaxY), 0);
        Camera.main.transform.position = new Vector3(Mathf.Clamp(targetPos.x, _camMinX, _camMaxX), Mathf.Clamp(targetPos.y, _camMinY, _camMaxY), -10);

      
    }

    private void Tap() {
    }


    public IEnumerator SpeedBonusRoutine(float multiplier, float time) {
        movementMultiplier = multiplier;
        yield return new WaitForSeconds(time);
        movementMultiplier = 1;

    }

    [Command]
    public void CmdSetBombPlayer() {
        RpcSetBombPlayer();
    }
    [ClientRpc]
    void RpcSetBombPlayer() {
        if (this.Bomb != null) return;
        GameObject bomb = GameObject.Instantiate(Resources.Load("Prefabs/Bomb") as GameObject);
        bomb.GetComponent<Bomb>().Player = this;
        this.Bomb = bomb.GetComponent<Bomb>();
        this.ChangeAnimator("RedAnimator");
        StartCoroutine(EnablePassTheBomb());
        if (isServer) NetworkServer.Spawn(bomb);
    }
    IEnumerator EnablePassTheBomb() {
        _canPassBomb = false;
        yield return new WaitForSeconds(1);
        _canPassBomb = true;
    }

    [Command]
    public void CmdRemoveBombPlayer() {
        RpcRemoveBombPlayer();
    }
    [ClientRpc]
    void RpcRemoveBombPlayer() {
        if (this.Bomb == null) return;
        NetworkServer.Destroy(this.Bomb.gameObject);
        this.Bomb = null;
        Debug.Log("Change animator to green");
        ChangeAnimator("GreenAnimator");
    }

    [Command]
    void CmdChangeAnimation(int condition) {
        RpcChangeAnimation(condition);
    }
    [ClientRpc]
    void RpcChangeAnimation(int condition) {
        CharacterAnimator.SetInteger("Condition", condition);
    }

    void ChangeAnimator(string name) {
        CharacterAnimator.runtimeAnimatorController = Resources.Load("Animation/" + name) as RuntimeAnimatorController;
    }
    [ClientRpc]
    void RpcChangeAnimator(string name) {
        CharacterAnimator.runtimeAnimatorController = Resources.Load("Animation/" + name) as RuntimeAnimatorController;
    }
    [Command]
    void CmdPickBall() {
        RpcPickBall();
    }
    [ClientRpc]
    void RpcPickBall() {
        if (_pickBallRoutine == null) {
            _pickBallRoutine = StartCoroutine(PickBallRoutine());
        }
    }

    [Command]
    void CmdThrowBall() {
        RpcThrowBall();
    }
    [ClientRpc]
    void RpcThrowBall() {
        if (_throwBallRoutine == null) {
            _throwBallRoutine = StartCoroutine(ThrowBallRoutine());
        }
    }

    [Command]
    void CmdSetPlayerColor(string name) {
        RpcSetPlayerColor(name);
    }
    [ClientRpc]
    void RpcSetPlayerColor(string name) {
        ChangeAnimator(name);
    }

    [Command]
    void CmdGetHit() {
        RpcGetHit();
    }
    [ClientRpc]
    void RpcGetHit() {
        if (_getHitRoutine == null) {
            _getHitRoutine = StartCoroutine(GetHitRoutine());
        }
    }
    IEnumerator GetHitRoutine() {
        _disabled = true;
        CmdChangeAnimation(4);
        yield return new WaitForSeconds(3f);
        _getHitRoutine = null;
        _disabled = false;
    }

    IEnumerator PickBallRoutine() {
        _canMove = false;
        CmdChangeAnimation(3);
        yield return new WaitForSeconds(1.1f);
        _canMove = true;
        _pickBallRoutine = null;
        _playerLocked = false;
    }

    IEnumerator ThrowBallRoutine() {
        //		_canMove = false;
        //		_hasBall = false;
        //Play Transfer Bomb Animation
        //		CmdChangeAnimation(5);
        yield return new WaitForSeconds(0.85f);
        if (isServer) CmdPassBomb();
        yield return new WaitForSeconds(0.3f);
        //		_canMove = true;
        //		_throwBallRoutine = null;
        //		_playerLocked = false;
    }

    // This [Command] code is called on the Client …
    // … but it is run on the Server!
    [Command]
    void CmdPassBomb() {
        // Create the Bullet from the Bullet Prefab
        //		var wave = (GameObject)Instantiate(
        //			BulletPrefab,
        //			BulletSpawn.position,
        //			transform.rotation);
        //
        //		// Add velocity to the wave
        //
        //
        //		// Spawn the bullet on the Clients
        //		NetworkServer.Spawn(wave);
        //
        //		// Destroy the bullet after 2 seconds
        //		Destroy(wave, 2.0f);
        RpcPassBomb();
    }
    [ClientRpc]
    void RpcPassBomb() {
        StartCoroutine(CheckPassBombRoutine());
    }

    public void Die() {
        if (_isDead) return;
        Debug.Log("Die");
        _isDead = true;
        _canMove = false;
        if (isLocalPlayer) CmdChangeAnimation(5);
        if (isLocalPlayer) StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine() {
        Text respawnTimer = GameObject.Find("Canvas").transform.Find("RespawnTimer").GetComponent<Text>();
        respawnTimer.gameObject.SetActive(true);
        Deaths++;
        Text livesLeft = GameObject.Find("Canvas").transform.Find("Lives").GetComponent<Text>();
        livesLeft.text = "Death Count: " + Deaths;
        respawnTimer.text = "Time to Respawn: 3";
        for (int i = 1; i <= 3; i++) {
            yield return new WaitForSeconds(1);
            respawnTimer.text = "Time to Respawn: " + (3 - i).ToString();
        }
        _isDead = false;
        _canMove = true;
        CmdChangeAnimation(6);
        CmdSetPlayerColor("GreenAnimator");
        respawnTimer.gameObject.SetActive(false);
        
    }

    IEnumerator CheckPassBombRoutine() {
        //Get al players in radius
        transform.Find("Radius").localScale = Vector3.one * _passBombRadius / 5;
        transform.Find("Radius").gameObject.SetActive(true);
        for (int i = 1; i <= 5; i++) {
            yield return new WaitForSeconds(0.1f);
            Collider[] playersInRadius = Physics.OverlapSphere(transform.position, _passBombRadius / 5 * i);//, LayerMask.NameToLayer("Player"));
            transform.Find("Radius").localScale = Vector3.one * _passBombRadius / 5 * i * 10;
            if (playersInRadius.Length == 0) {
                continue;
            }
            foreach (Collider col in playersInRadius) {
                if (col.transform == transform) continue;
                if (col.GetComponent<Player>() == null) continue;
                if (col.GetComponent<Player>().Bomb != null) continue;
                Debug.Log("Player in radius");
               if (isServer) {
                    col.GetComponent<Player>().CmdSetBombPlayer();
                    //Remove bomb if has bomb
                    if (this.Bomb != null) {
                        CmdRemoveBombPlayer();
                        Debug.Log("Remove Bomb from me");
                    }
               }
                //here
            }
        }
        transform.Find("Radius").gameObject.SetActive(false);
    }

    public override void OnStartClient() {
        //GameObject characterMesh = GameObject.Instantiate(Resources.Load("Player_Mesh") as GameObject);
        //characterMesh.transform.SetParent(transform);
        //characterMesh.transform.position = new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3));
        //if (isServer) {
        //    _playerColor = Random.ColorHSV();
        //}
        //characterMesh.GetComponentInChildren<Renderer>().material.color = _playerColor;
        //characterMesh.GetComponentInChildren<PlayerMesh>().PlayerTransform = transform;
        //characterMesh.GetComponentInChildren<PlayerMesh>().Player = this;
        CharacterAnimator = GetComponentInChildren<Animator>();
        name = "Player_" + GetComponent<NetworkIdentity>().netId.ToString();
        Main.AddPlayer(this);
    }

    public override void OnStartLocalPlayer() {
        PlayerId++;
        //SetCamera
        //		Camera.main.GetComponent<FollowPlayer>().Player = this.transform;

        //CmdSetPlayerColor(_playerColor);
        //		CmdSetPlayerColor(Random.ColorHSV());
        //		Canvas = GameObject.Find("Canvas").transform;
        //Set Health Hearts References
        //		Health = GetComponent<Health>();
        //		foreach (Transform t in Canvas.Find("Health")){
        //			if(t.name == "Heart"){
        //				Health.Hearts.Add(t.Find("Fill").GetComponent<Image>());
        //			}
        //		}
        GameObject.Find("Main Camera").GetComponent<FollowTarget>().Target = this.transform;
        CmdSetPlayerName();
        Score = 0f;
        Deaths = 0;
        _map = GameObject.FindObjectOfType<MapManager>();
        var camHalfHeight = Camera.main.orthographicSize;
        var camHalfWidth = camHalfHeight * Camera.main.aspect;
        var playerHalfHeight = GetComponent<BoxCollider>().size.y / 2f;
        var playerHalfWidth = GetComponent<BoxCollider>().size.x / 2f;
        _camMaxX = _map.WidthX / 2 - camHalfWidth;
        _camMinX = -_camMaxX;
        _playerMaxX = _map.WidthX / 2 - playerHalfWidth;
        _playerMinX = -_playerMaxX;
        _camMaxY = _map.HeightY / 2 - camHalfHeight;
        _camMinY = -_camMaxY;
        _playerMaxY = _map.HeightY / 2 - playerHalfHeight;
        _playerMinY = -_playerMaxY;
    }

    [Command]
    void CmdSetPlayerName() {
        RpcSetPlayerName();
    }
    [ClientRpc]
    void RpcSetPlayerName() {
        //PlayerName = Main.Instance.PlayerName;
    }
    void OnDestroy() {
        //Clear the mesh when player is destroyed
        if (CharacterAnimator.transform.root.gameObject != null) GameObject.Destroy(CharacterAnimator.transform.root.gameObject);
    }
    public void GetHit() {
        if (isLocalPlayer) CmdGetHit();
    }

    private void OnDisconnectedFromServer(NetworkDisconnection info) {
        Debug.Log("You need restart");
    }

    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<Collectable>() != null) {
            other.GetComponent<Collectable>().Collect(this);
        }
    }
}