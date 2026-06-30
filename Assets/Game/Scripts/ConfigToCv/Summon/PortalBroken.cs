using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalBroken : MonoBehaviour {
    public Vector2 Force;
    private RandomForce[] randomForces;

    private void Start() {
        randomForces = GetComponentsInChildren<RandomForce>();
    }

    //public void Broken() {
    //    foreach (RandomForce i in randomForces) {
    //        Timer.Run(Random.value * 0.1, delegate {
    //            i.Force = Random.Range(Force.x, Force.y);
    //            i.Force *= Mathf.Abs(transform.localScale.z);
    //            i.Broken();
    //        },gameObject);
    //    }
    //}
}