using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class RoleClass : MonoBehaviour {

    private string _str;
    private List<int> strList = new List<int>();

	// Use this for initialization
	void Start () {
        //create str
        StringBuilder _sb = new StringBuilder();
        int t1;
        for (var i = 0; i < 100000; i++) {
            t1 = Random.Range(10000, 99999);
            _sb.Append(t1);
            _sb.Append(" ");
            strList.Add(t1);
        }
        _str = _sb.ToString();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
