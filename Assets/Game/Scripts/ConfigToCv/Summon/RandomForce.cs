using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(Rigidbody))]
public class RandomForce : MonoBehaviour {
    public Transform Origin;
    public float Force;
    private Rigidbody Rig;
    private Vector3 MoveSpeed = Vector3.zero;

    private void Start() {
        // Rig = GetComponent<Rigidbody>();
    }

    private void Update() {
        /*
        if (Input.GetMouseButtonDown(0))
            Broken();
        */
        transform.position += MoveSpeed;
    }

    public void Broken() {
        /*
        Vector3 f = transform.position - Origin.position;
        f = f.normalized * Force;
        Rig.AddForce(f, ForceMode.Impulse);
        */
        MoveSpeed = transform.position - Origin.position;
        MoveSpeed.Normalize();
        MoveSpeed *= Force * Time.deltaTime;

        Destroy(gameObject, 0.5f);
    }
}