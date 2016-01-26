using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterFootSteps : MonoBehaviour
{
    public float MaxDepth = -1.0f;

    public float MeshDeformAreaRadius = 0.25f;
    public float ZRadiusMul = 0.9f;
    public float ZStrengthMul = 1.0f;
    public float Strength = 5;
    public GameObject[] Terrain;
    private ComputeBuffer _buffer;
    private AudioSource _audioSource;
    public AudioClip[] GrindAudioClips, SnowAudioClips;
    public Texture2D GroundTypeMap;
    public float GroundTypeSensitivity = 0.3f;
    private Transform _leftFoot, _rightFoot;
    private List<Vector3> _spherePositionsList;

    void Start ()
    {
        _spherePositionsList = new List<Vector3>();
        _buffer = new ComputeBuffer(_spherePositionsList.Count, 12);

        _audioSource = GetComponent<AudioSource>();
        _leftFoot = GameObject.FindGameObjectWithTag("FootL").transform;
        _rightFoot = GameObject.FindGameObjectWithTag("FootR").transform;
    }

	void Update ()
    {
	
	}
    public void FootStep(bool leftFoot)
    {
         int x = Mathf.FloorToInt((transform.position.x +254) /  504 * GroundTypeMap.width);
         int y = Mathf.FloorToInt((transform.position.z + 254) / 504 * GroundTypeMap.height);
        var pixel = GroundTypeMap.GetPixel(x, y);
        if (pixel.r > GroundTypeSensitivity && pixel.g > GroundTypeSensitivity && pixel.b > GroundTypeSensitivity)
        {
            //Debug.Log("On Grind");
            _audioSource.clip = GrindAudioClips[Random.Range(0, GrindAudioClips.Length)];
            _audioSource.Play();
        }
        else
        {
            //Debug.Log("On Snow");
            _audioSource.clip = SnowAudioClips[Random.Range(0, SnowAudioClips.Length)];
            _audioSource.Play();

            

            //Determine terrain
            float shortestDistance = float.MaxValue;
            int partNr = 0;
            for (int i = 0; i < Terrain.Length ; i++)
            {
                var distance = Vector3.Distance(transform.position, Terrain[i].transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    partNr = i;
                }
            }

            var mat = Terrain[partNr].GetComponent<Renderer>().material;

            var lPos = _leftFoot.position;
            var lPosForward = lPos + _leftFoot.forward * 0.5f;
            var rPos = _rightFoot.position;
            var rPosForward = rPos + _rightFoot.forward*0.5f;

            //Calculate positions
            if (!_spherePositionsList.Contains(lPos))
                _spherePositionsList.Add(lPos);
            if (!_spherePositionsList.Contains(lPosForward))
                _spherePositionsList.Add(lPosForward);
            if (!_spherePositionsList.Contains(rPos))
                _spherePositionsList.Add(rPos);
            if (!_spherePositionsList.Contains(rPosForward))
                _spherePositionsList.Add(rPosForward);

            _buffer.Dispose();
            _buffer = new ComputeBuffer(_spherePositionsList.Count, 12);
            _buffer.SetData(_spherePositionsList.ToArray());
            mat.SetBuffer("_spherePosBuffer", _buffer);

        }
    }

    private void OnDestroy()
    {
        _buffer.Dispose();
        _buffer.Release();
    }
}
