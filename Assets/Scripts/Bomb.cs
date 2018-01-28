using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class Bomb : NetworkBehaviour {

	public Player Player;
	[SyncVar]public int TimeLeft  = 5;
	private Text _timerText;
	private float explosionRadius = 3;
	[SyncVar] public bool IsExploded = false;
	public Coroutine TimerCoroutine;

	// Use this for initialization
	void Start () {
		if(Player == null){
			Debug.Log("My player is null");
			NetworkServer.Destroy(this.gameObject);
			return;
		}
		transform.SetParent(Player.transform);
		transform.localPosition = Vector3.zero;
		//Set timer text
		_timerText = transform.Find("Canvas/Text").GetComponent<Text>();
		TimerCoroutine = StartCoroutine(TimerRoutine());
	}
	
	// Update is called once per frame
	IEnumerator TimerRoutine () {
		while(TimeLeft > 0){
			yield return new WaitForSeconds(1);
			if(TimeLeft != 0)TimeLeft--;
			_timerText.text = TimeLeft.ToString();
		}
		Explode();
	}
	public void Explode(){
        Transform sound = transform.Find("Sound");
        sound.GetComponent<AudioSource>().Play();
        sound.SetParent(null);
        GameObject.Destroy(sound.gameObject, 3);

        StartCoroutine(CheckExplosionHitRoutine());
	}
	[Command]
	void CmdExplode(){
		Debug.Log("Cmd Explode");
		RpcExplode();
	}
	[ClientRpc]
	void RpcExplode(){
		Debug.Log("RPC Explode");
		StartCoroutine(CheckExplosionHitRoutine());
	}
	IEnumerator CheckExplosionHitRoutine(){
		//Get al players in radius
		Player.Bomb = null;
		Player.Die();
		Debug.Log("Hi?");
		transform.Find("Radius").localScale = Vector3.one *  explosionRadius/5;
		transform.Find("Radius").gameObject.SetActive(true);
		for (int i = 1; i <= 7; i++) {
			yield return new WaitForSeconds(0.08f);
			Collider[] playersInRadius = Physics.OverlapSphere(transform.position, explosionRadius/7 * i);//, LayerMask.NameToLayer("Player"));
			transform.Find("Radius").localScale = Vector3.one *  explosionRadius/7 * i * 2;
			if(playersInRadius.Length == 0){
				continue;
			}
			foreach(Collider col in playersInRadius){
				if(col.transform == transform) continue;
				if(col.GetComponent<Player>() == null) continue;
				Bomb bomb = col.GetComponent<Player>().Bomb;
				if(bomb != null && bomb != this){
					bomb.StopCoroutine(bomb.TimerCoroutine);
					_timerText.gameObject.SetActive(false);
					col.GetComponent<Player>().Bomb.Explode();
				}
				else col.GetComponent<Player>().Die();
			}
		}
		transform.Find("Radius").gameObject.SetActive(false);
		NetworkServer.Destroy(this.gameObject);
	}
}
