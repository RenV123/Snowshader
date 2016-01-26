using UnityEngine;
using System.Collections;

public class CharacterFootStepsCompute : MonoBehaviour
{
    public float MaxDepth = -4.0f;

    public float MeshDeformAreaRadius = 5.25f;
    public float ZRadiusMul = 1.0f;
    public float ZStrengthMul = 1.0f;
    public float Strength = 215;
    public GameObject[] Terrain;
    public ComputeShader Shader;
    private ComputeBuffer[] _buffer;
    private Vector3[] _vertices;
    private AudioSource _audioSource;
    public AudioClip[] GrindAudioClips;
    public AudioClip[] SnowAudioClips;
    public Texture2D GroundTypeMap;
    public Texture2D FootPrintDecalNormalMap;
    public float GroundTypeSensitivity = 0.3f;

    private Transform _leftFoot, _rightFoot;

    

    private int _kernel;
    // Use this for initialization
    void Start ()
    {
        _kernel = Shader.FindKernel("DepthCalculator");

        _buffer = new ComputeBuffer[Terrain.Length];

        for (int i = 0; i < Terrain.Length; i++)
        {
            _vertices = Terrain[i].GetComponent<MeshFilter>().mesh.vertices;
            _buffer[i] = new ComputeBuffer(_vertices.Length, 12);
            _buffer[i].SetData(_vertices);
        }
        

        Shader.SetFloat("MaxDepth", MaxDepth);
        Shader.SetFloat("Radius", MeshDeformAreaRadius);
        Shader.SetFloat("Strength", Strength);

        _audioSource = GetComponent<AudioSource>();
        _leftFoot = GameObject.FindGameObjectWithTag("FootL").transform;
        _rightFoot = GameObject.FindGameObjectWithTag("FootR").transform;

    }
	
	// Update is called once per frame
	void Update ()
    {

    }

    public void FootStep(bool leftFoot)
    {
        //Set Debug param
        Shader.SetFloat("ZStrengthMul", ZStrengthMul);
        Shader.SetFloat("ZRadiusMul", ZRadiusMul);

        //Debug.Log("Step");
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


            //Determine Terrain Part 
            int i = 0;
            float shortestDistance = float.MaxValue;
            int partNr = 0;
            foreach (var part in Terrain)
            {
                var distance = Vector3.Distance(transform.position, part.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    partNr = i;
                }
                i++;
            }

            //Get Vertices
            _vertices = Terrain[partNr].GetComponent<MeshFilter>().mesh.vertices;


            //Set FootPrint Normal Map
            /*var mat = Terrain[partNr].GetComponent<Renderer>().material;
            //FootPrintDecalNormalMap
            Vector3 dwn = transform.TransformDirection(Vector3.down);
            var ray = new Ray(transform.position, dwn);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 500))
            {
               
                Vector2 pixelUV = hit.textureCoord;
                var normalMap = mat.GetTexture("_BumpMap") as Texture2D;
                
                if (normalMap != null)
                {
                    Debug.Log(hit.textureCoord);
                    pixelUV.x *= normalMap.width;
                    pixelUV.y *= normalMap.height;

                    int X = (int)pixelUV.x - FootPrintDecalNormalMap.width / 2;
                    int Y = (int)pixelUV.y - FootPrintDecalNormalMap.height / 2;
                    int width = FootPrintDecalNormalMap.width;
                    int height = FootPrintDecalNormalMap.height;

                    var decalNormalPixelsArr = FootPrintDecalNormalMap.GetPixels();
                    var floorNormalTexPixels = normalMap.GetPixels(x, y, width, height);
                    var normalColor = decalNormalPixelsArr[0];
                    Color[] normalColorArr = new Color[decalNormalPixelsArr.Length];

                    for (int a = 0; a < decalNormalPixelsArr.Length; a++)
                    {
                        if (decalNormalPixelsArr[a] == normalColor)
                            normalColorArr[a] = floorNormalTexPixels[a];
                        else
                            normalColorArr[a] = decalNormalPixelsArr[a];
                    }

                    normalMap.SetPixels(x, y, width, height, normalColorArr);

                    normalMap.Apply();

                }
            }*/


            Shader.SetBuffer(_kernel, "dataBuffer", _buffer[partNr]);


            var lPos = Terrain[partNr].transform.InverseTransformPoint(_leftFoot.position);
            var lPosForward = Terrain[partNr].transform.InverseTransformPoint(_leftFoot.forward*0.5f);
            var rPos = Terrain[partNr].transform.InverseTransformPoint(_rightFoot.position);
            var rPosForward = Terrain[partNr].transform.InverseTransformPoint(_rightFoot.forward * 0.5f);

            Shader.SetVector("PosOne", new Vector4(lPos.x, lPos.y, lPos.z, 0));
            Shader.SetVector("PosTwo", new Vector4(lPosForward.x, lPosForward.y, lPosForward.z, 0));
            Shader.SetVector("PosThree", new Vector4(rPos.x, rPos.y, rPos.z, 0));
            Shader.SetVector("PosFour", new Vector4(rPosForward.x, rPosForward.y, rPosForward.z, 0));

            Shader.Dispatch(_kernel, _vertices.Length, 1, 1);

            //Set Data from Mesh
            Vector3[] vert = new Vector3[_vertices.Length];
            _buffer[partNr].GetData(vert);
            Terrain[partNr].GetComponent<MeshFilter>().mesh.vertices = vert;

           


        }


    }

    private void OnDestroy()
    {
        for (int i = 0; i < Terrain.Length; i++)
        {
            //_buffer[i].Dispose();
            _buffer[i].Release();
        }
    }
}
