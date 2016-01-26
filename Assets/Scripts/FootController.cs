using UnityEngine;
using System.Collections;

public class FootController : MonoBehaviour
{
    private CharacterFootSteps _character;
    private bool _isLeftFoot;
	void Start ()
	{
        _character = GetComponentInParent<CharacterFootSteps>();
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
