using UnityEngine;
using System.Collections;

public class EyeSight : MonoBehaviour {
	int count = 0;
	
	void Start () {
		// 描画をしないだけなので、Update()関数は呼ばれ続ける。
		GetComponent<Renderer>().enabled = false;
	}
	
	void Update () {

	}
}