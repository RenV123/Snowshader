using UnityEngine;
using System.Collections;

public class FootControllerCompute : MonoBehaviour
{
    private CharacterFootStepsCompute _character;
    private bool _isLeftFoot;
	void Start ()
	{
        _character = GetComponentInParent<CharacterFootStepsCompute>();
	    if (gameObject.tag == "FootL") _isLeftFoot = true;
	}

	void Update ()
    {
	
	}

    void OnTriggerEnter(Collider other)
    {
        _character.FootStep(_isLeftFoot);
    }
}
