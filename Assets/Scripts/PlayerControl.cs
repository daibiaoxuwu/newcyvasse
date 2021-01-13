using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
namespace cs
{
    /* This version of ObjImporter first reads through the entire file, getting a count of how large
     * the final arrays will be, and then uses standard arrays for everything (as opposed to ArrayLists
     * or any other fancy things). 
     */

    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class ObjImporter
    {

        private struct meshStruct
        {
            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector2[] uv;
            public Vector2[] uv1;
            public Vector2[] uv2;
            public int[] triangles;
            public int[] faceVerts;
            public int[] faceUVs;
            public Vector3[] faceData;
            public string name;
            public string fileName;
        }

        // Use this for initialization
        public Mesh ImportFile(string filePath)
        {
            meshStruct newMesh = createMeshStruct(filePath);
            populateMeshStruct(ref newMesh);

            Vector3[] newVerts = new Vector3[newMesh.faceData.Length];
            Vector2[] newUVs = new Vector2[newMesh.faceData.Length];
            Vector3[] newNormals = new Vector3[newMesh.faceData.Length];
            int i = 0;
            /* The following foreach loops through the facedata and assigns the appropriate vertex, uv, or normal
             * for the appropriate Unity mesh array.
             */
            foreach (Vector3 v in newMesh.faceData)
            {
                newVerts[i] = newMesh.vertices[(int)v.x - 1];
                if (v.y >= 1)
                    newUVs[i] = newMesh.uv[(int)v.y - 1];

                if (v.z >= 1)
                    newNormals[i] = newMesh.normals[(int)v.z - 1];
                i++;
            }

            Mesh mesh = new Mesh();

            mesh.vertices = newVerts;
            mesh.uv = newUVs;
            mesh.normals = newNormals;
            mesh.triangles = newMesh.triangles;

            mesh.RecalculateBounds();
           // MeshUtility.Optimize(mesh);

            return mesh;
        }

        private static meshStruct createMeshStruct(string filename)
        {
            int triangles = 0;
            int vertices = 0;
            int vt = 0;
            int vn = 0;
            int face = 0;
            meshStruct mesh = new meshStruct();
            mesh.fileName = filename;
            StreamReader stream = File.OpenText(filename);
            string entireText = stream.ReadToEnd();
            stream.Close();
            using (StringReader reader = new StringReader(entireText))
            {
                string currentText = reader.ReadLine();
                char[] splitIdentifier = { ' ' };
                string[] brokenString;
                while (currentText != null)
                {
                    if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ")
                        && !currentText.StartsWith("vn "))
                    {
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");
                        }
                    }
                    else
                    {
                        currentText = currentText.Trim();                           //Trim the current line
                        brokenString = currentText.Split(splitIdentifier, 50);      //Split the line into an array, separating the original line by blank spaces
                        switch (brokenString[0])
                        {
                            case "v":
                                vertices++;
                                break;
                            case "vt":
                                vt++;
                                break;
                            case "vn":
                                vn++;
                                break;
                            case "f":
                                face = face + brokenString.Length - 1;
                                triangles = triangles + 3 * (brokenString.Length - 2); /*brokenString.Length is 3 or greater since a face must have at least
                                                                                     3 vertices.  For each additional vertice, there is an additional
                                                                                     triangle in the mesh (hence this formula).*/
                                break;
                        }
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");
                        }
                    }
                }
            }
            mesh.triangles = new int[triangles];
            mesh.vertices = new Vector3[vertices];
            mesh.uv = new Vector2[vt];
            mesh.normals = new Vector3[vn];
            mesh.faceData = new Vector3[face];
            return mesh;
        }

        private static void populateMeshStruct(ref meshStruct mesh)
        {
            StreamReader stream = File.OpenText(mesh.fileName);
            string entireText = stream.ReadToEnd();
            stream.Close();
            using (StringReader reader = new StringReader(entireText))
            {
                string currentText = reader.ReadLine();

                char[] splitIdentifier = { ' ' };
                char[] splitIdentifier2 = { '/' };
                string[] brokenString;
                string[] brokenBrokenString;
                int f = 0;
                int f2 = 0;
                int v = 0;
                int vn = 0;
                int vt = 0;
                int vt1 = 0;
                int vt2 = 0;
                while (currentText != null)
                {
                    if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ") &&
                        !currentText.StartsWith("vn ") && !currentText.StartsWith("g ") && !currentText.StartsWith("usemtl ") &&
                        !currentText.StartsWith("mtllib ") && !currentText.StartsWith("vt1 ") && !currentText.StartsWith("vt2 ") &&
                        !currentText.StartsWith("vc ") && !currentText.StartsWith("usemap "))
                    {
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");
                        }
                    }
                    else
                    {
                        currentText = currentText.Trim();
                        brokenString = currentText.Split(splitIdentifier, 50);
                        switch (brokenString[0])
                        {
                            case "g":
                                break;
                            case "usemtl":
                                break;
                            case "usemap":
                                break;
                            case "mtllib":
                                break;
                            case "v":
                                mesh.vertices[v] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
                                                         System.Convert.ToSingle(brokenString[3]));
                                v++;
                                break;
                            case "vt":
                                mesh.uv[vt] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                                vt++;
                                break;
                            case "vt1":
                                mesh.uv[vt1] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                                vt1++;
                                break;
                            case "vt2":
                                mesh.uv[vt2] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                                vt2++;
                                break;
                            case "vn":
                                mesh.normals[vn] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
                                                        System.Convert.ToSingle(brokenString[3]));
                                vn++;
                                break;
                            case "vc":
                                break;
                            case "f":

                                int j = 1;
                                List<int> intArray = new List<int>();
                                while (j < brokenString.Length && ("" + brokenString[j]).Length > 0)
                                {
                                    Vector3 temp = new Vector3();
                                    brokenBrokenString = brokenString[j].Split(splitIdentifier2, 3);    //Separate the face into individual components (vert, uv, normal)
                                    temp.x = System.Convert.ToInt32(brokenBrokenString[0]);
                                    if (brokenBrokenString.Length > 1)                                  //Some .obj files skip UV and normal
                                    {
                                        if (brokenBrokenString[1] != "")                                    //Some .obj files skip the uv and not the normal
                                        {
                                            temp.y = System.Convert.ToInt32(brokenBrokenString[1]);
                                        }
                                        temp.z = System.Convert.ToInt32(brokenBrokenString[2]);
                                    }
                                    j++;

                                    mesh.faceData[f2] = temp;
                                    intArray.Add(f2);
                                    f2++;
                                }
                                j = 1;
                                while (j + 2 < brokenString.Length)     //Create triangles out of the face data.  There will generally be more than 1 triangle per face.
                                {
                                    mesh.triangles[f] = intArray[0];
                                    f++;
                                    mesh.triangles[f] = intArray[j];
                                    f++;
                                    mesh.triangles[f] = intArray[j + 1];
                                    f++;

                                    j++;
                                }
                                break;
                        }
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");       //Some .obj files insert double spaces, this removes them.
                        }
                    }
                }
            }
        }
    }

    public class PlayerControl : MonoBehaviour
    {


        private Camera PlayerCam;           // Camera used by the player
        private GameManager _GameManager;   // GameObject responsible for the management of the game       

        static int LINENUM = 0;
        public static int player = 0, curx = 0, cury = 0;
        public int mode = -70;
        public string answer = "J";


        public static bool isPush = false;
        public static bool issteal = false;
        public GameObject floor01D;
        public GameObject floor02D;
        public GameObject blockS01D;
        public GameObject blockS02D;
        public GameObject blockSpike01D;
        public GameObject blockSpike02D;
        public GameObject Plane;
        public static GameObject tokenBlue, tokenRed;
        public static GameObject[] tokensBlue = new GameObject[3];
        public static GameObject[] tokensRed = new GameObject[3];
        public static Vector3 offset = new Vector3(0, 0, 0);
        public static Quaternion rotation = Quaternion.Euler(0, 90, 0);

        public static void drawToken(int i, int j, int num)
        {
            Vector3 position = offset + new Vector3(i * 2, 0f, j * 2);
            if (i < 5 || i > 9) position += new Vector3(0f, 0.5f, 0);
            if (i >= 0 && i < 15 && j >= 0 && j < 15)
            {

                GameObject newPiecePrefab = new GameObject();

                GameObject newPiece = Instantiate(newPiecePrefab, position, rotation);

                Mesh holderMesh = new Mesh();
                ObjImporter newMesh = new ObjImporter();

                string type = "arrowraintoken";
                Debug.Log(type);

                if (player == 0)
                {
                    newPiece.transform.position += new Vector3(-2.14f, -1f, -2.17f);
                    newPiece.transform.rotation = Quaternion.Euler(0, -90, 0);
                    type = type + "\\" + type + ".blue";

                }
                else
                {
                    newPiece.transform.position += new Vector3(2.14f, -1f, 2.17f);
                    type = type + "\\" + type + ".red";
                }
                string meshPath = "D:\\workspace\\unity\\Chess\\Resources\\pieces\\" + type + ".obj";
                string texPath = "D:\\workspace\\unity\\Chess\\Resources\\pieces\\" + type + ".png";



                holderMesh = newMesh.ImportFile(meshPath);

                MeshRenderer renderer = newPiece.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Nature/SpeedTree"));
                MeshFilter filter = newPiece.AddComponent<MeshFilter>();
                filter.mesh = holderMesh;

                byte[] fileData = File.ReadAllBytes(texPath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                renderer.material.SetTexture("_MainTex", tex);
                 if (player==0) tokensBlue[num] = newPiece; else tokensRed[num] = newPiece;
            }
        }
        public static void delToken()
        {
            for (int num = 0; num < 3; ++num)
            {
                if (player == 1) Destroy(tokensBlue[num]); else Destroy(tokensRed[num]);
            }
        }


        // Use this for initialization
        void DrawBoard()
        {
            Plate.init();
            Plate.floors = new GameObject[15][];
            for (int i = 0; i < 15; i++)
            {
                Plate.floors[i] = new GameObject[15];
            }

            for (int i = -1; i <= 15; ++i)
            {
                for (int j = -1; j <= 15; ++j)
                {
                    GameObject floor;

                    Vector3 position = offset + new Vector3(i * 2, 0f, j * 2);
                    if (i < 5 || i > 9) position += new Vector3(0f, 0.5f, 0);
                    if (i >= 0 && i < 15 && j >= 0 && j < 15)
                    {
                        if ((i + j) % 2 == 0)
                            floor = Instantiate(floor01D, position, rotation);
                        else
                            floor = Instantiate(floor02D, position, rotation);
                        floor.transform.parent = gameObject.transform;

                        floor.GetComponent<BoardSquare>().posx = i;
                        floor.GetComponent<BoardSquare>().posy = j;

                        GameObject newplane = Instantiate(Plane, position + new Vector3(0f, 1.1f, 0f), rotation);
                        newplane.SetActive(false);
                        newplane.transform.parent = floor.transform;
                        Plate.floors[i][j] = floor;
                    }
                    if (i == 15)
                    {
                        GameObject border, border2, border3;
                        if (j == 15)
                        {
                            border = Instantiate(blockSpike02D, position + new Vector3(-0.5f, 1.5f, -0.5f), rotation);
                            border.transform.parent = gameObject.transform;
                            border3 = Instantiate(blockS02D, position + new Vector3(0.5f, 1.5f, -0.5f), rotation);
                            border3.transform.parent = gameObject.transform;
                        }
                        else
                        {
                            border = Instantiate(blockS01D, position + new Vector3(-0.5f, 1.5f, -0.5f), rotation);
                            border.transform.parent = gameObject.transform;
                        }

                        if (j == -1)
                        {
                            border2 = Instantiate(blockSpike01D, position + new Vector3(-0.5f, 1.5f, 0.5f), rotation);
                            border2.transform.parent = gameObject.transform;
                            border3 = Instantiate(blockS01D, position + new Vector3(0.5f, 1.5f, 0.5f), rotation);
                            border3.transform.parent = gameObject.transform;
                        }
                        else
                        {
                            border2 = Instantiate(blockS02D, position + new Vector3(-0.5f, 1.5f, 0.5f), rotation);
                            border2.transform.parent = gameObject.transform;
                        }

                    }

                    if (i == -1)
                    {
                        GameObject border, border2, border3;
                        if (j == 15)
                        {
                            border = Instantiate(blockSpike01D, position + new Vector3(0.5f, 1.5f, -0.5f), rotation);
                            border.transform.parent = gameObject.transform;
                            border3 = Instantiate(blockS01D, position + new Vector3(-0.5f, 1.5f, -0.5f), rotation);
                            border3.transform.parent = gameObject.transform;
                        }
                        else
                        {
                            border = Instantiate(blockS02D, position + new Vector3(0.5f, 1.5f, -0.5f), rotation);
                            border.transform.parent = gameObject.transform;
                        }

                        if (j == -1)
                        {
                            border2 = Instantiate(blockSpike02D, position + new Vector3(0.5f, 1.5f, 0.5f), rotation);
                            border2.transform.parent = gameObject.transform;
                            border3 = Instantiate(blockS02D, position + new Vector3(-0.5f, 1.5f, 0.5f), rotation);
                            border3.transform.parent = gameObject.transform;
                        }
                        else
                        {
                            border2 = Instantiate(blockS01D, position + new Vector3(0.5f, 1.5f, 0.5f), rotation);
                            border2.transform.parent = gameObject.transform;
                        }

                    }
                    if ((j == -1 || j == 15) && i != -1 && i != 15)
                    {
                        float z = 0.5f;
                        if (j == 15) z = -0.5f;
                        GameObject border, border2;
                        border = Instantiate(blockS01D, position + new Vector3(z, 1.5f, z), rotation);
                        border.transform.parent = gameObject.transform;
                        border2 = Instantiate(blockS02D, position + new Vector3(-z, 1.5f, z), rotation);
                        border2.transform.parent = gameObject.transform;
                    }
                }
            }
        }
        public static void displayBoard()
        {
            if (Plate.floors == null) return;
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (Plate.plateCol[i][j] != Color.black)
                    {
                        GameObject plane = Plate.floors[i][j].transform.GetChild(0).gameObject;
                        plane.SetActive(true);
                        plane.GetComponent<Renderer>().material.SetColor("_Color", Plate.plateCol[i][j]);
                    }
                    else
                    {
                        Plate.floors[i][j].transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
            }
        }
        static bool[] flag;
        void turnTurn()
        {
            Plate.turnTurn();
            DrawPieces();
            for (int p = 0; p <= 1; ++p)
            {
                if (flag[p])
                {
                    bool findflag = false;
                    bool findKing = false;
                    int maxvalue = -1, maxx = 0, maxy = 0;
                    for (int i = 0; i < 15; ++i)
                    {
                        for (int j = 0; j < 15; ++j)
                        {
                            if (Plate.plate[i][j] == null || Plate.plate[i][j].player != p) continue;
                            if (Plate.plate[i][j].dizzy > 0) Plate.plate[i][j].dizzy--;
                            if (Plate.plate[i][j].value() > maxvalue)
                            {
                                if (maxvalue < Plate.plate[i][j].value())
                                {
                                    maxvalue = Plate.plate[i][j].value();
                                    maxx = i; maxy = j;
                                }
                            }
                            if (Plate.plate[i][j] is King) findKing = true;
                            if (Plate.plate[i][j] is Flag)
                            {
                                if ((p == 0 && i > 9) || (p == 1 && i < 5))
                                {
                                    Debug.Log("玩家" + p.ToString() + "通过插旗获得胜利。");
                                    //  string answer=Console.ReadKey().Key.ToString();
                                }
                                findflag = true;
                            }
                        }
                    }
                    if (findflag == false)
                    {
                        flag[p] = false;
                        Plate.plate[maxx][maxy] = null;
                    }
                    if (findKing == false)
                    {
                        Debug.Log("玩家" + (1 - p).ToString() + "获得胜利。");
                        //  string answer=Console.ReadKey().Key.ToString();
                    }
                }
            }

            mode = 0; answer = "J";
            player = 1 - player;

            Debug.Log("turn!" + player);
            Plate.colRefresh();
        }
        public static int[] stone;


        static void pushMec(int srcx, int srcy, int dstx, int dsty)
        {
            bool move1 = false, move2 = false, move3 = false, move4 = false;
            Piece selpiece;
            if (srcx > 0 && Plate.plate[srcx - 1][srcy] != null && Plate.plate[srcx - 1][srcy].ismechanics() && Plate.plate[srcx - 1][srcy].player == player)
                if (dstx > 0 && (Plate.plate[dstx - 1][dsty] == null || Plate.plate[srcx - 1][srcy] is Ram))
                {
                    if (Plate.plate[dstx - 1][dsty] != null && Plate.plate[dstx - 1][dsty] is Wall) stone[player]++;
                    move1 = true;
                }
            if (srcx < 14 && Plate.plate[srcx + 1][srcy] != null && Plate.plate[srcx + 1][srcy].ismechanics() && Plate.plate[srcx + 1][srcy].player == player)
                if (dstx < 14 && (Plate.plate[dstx + 1][dsty] == null || Plate.plate[srcx + 1][srcy] is Ram))
                {
                    if (Plate.plate[dstx + 1][dsty] != null && Plate.plate[dstx + 1][dsty] is Wall) stone[player]++;
                    move2 = true;
                }
            if (srcy > 0 && Plate.plate[srcx][srcy - 1] != null && Plate.plate[srcx][srcy - 1].ismechanics() && Plate.plate[srcx][srcy - 1].player == player)
                if (dsty > 0 && (Plate.plate[dstx][dsty - 1] == null || Plate.plate[srcx][srcy - 1] is Ram))
                {
                    if (Plate.plate[dstx][dsty - 1] != null && Plate.plate[dstx][dsty - 1] is Wall) stone[player]++;
                    move3 = true;
                }
            if (srcy < 14 && Plate.plate[srcx][srcy + 1] != null && Plate.plate[srcx][srcy + 1].ismechanics() && Plate.plate[srcx][srcy + 1].player == player)
                if (dsty < 14 && (Plate.plate[dstx][dsty + 1] == null || Plate.plate[srcx][srcy + 1] is Ram))
                {
                    if (Plate.plate[dstx][dsty + 1] != null && Plate.plate[dstx][dsty + 1] is Wall) stone[player]++;
                    move4 = true;
                }
            if (move1)
            {
                selpiece = Plate.plate[srcx - 1][srcy];
                selpiece.wait = 0;
                Plate.plate[dstx - 1][dsty] = selpiece;
                Plate.plate[srcx - 1][srcy] = null;
            }
            if (move2)
            {
                selpiece = Plate.plate[srcx + 1][srcy];
                selpiece.wait = 0;
                Plate.plate[dstx + 1][dsty] = selpiece;
                Plate.plate[srcx + 1][srcy] = null;
            }
            if (move3)
            {
                selpiece = Plate.plate[srcx][srcy - 1];
                selpiece.wait = 0;
                Plate.plate[dstx][dsty - 1] = selpiece;
                Plate.plate[srcx][srcy - 1] = null;
            }
            if (move4)
            {
                selpiece = Plate.plate[srcx][srcy + 1];
                selpiece.wait = 0;
                Plate.plate[dstx][dsty + 1] = selpiece;
                Plate.plate[srcx][srcy + 1] = null;
            }
        }
        // Use this for initialization
        void Start()
        {
            PlayerCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>(); // Find the Camera's GameObject from its tag 
            _GameManager = gameObject.GetComponent<GameManager>();

            // init();
            DrawBoard();
            //Plate.init();
            DrawPieces();

            flag = new bool[2];
            flag[0] = true; flag[1] = true;
            stone = new int[2];
            stone[0] = 1; stone[1] = 1;
            mode = -70;

        }
        void DrawPieces(){
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                {
                    GameObject piece = Plate.floors[i][j].GetComponent<BoardSquare>().piece;

                    if (Plate.plate[i][j] != null && (piece == null || Plate.plate[i][j].getName()=="[]"))
                    {
			if(Plate.plate[i][j].getName()=="[]") 
				Destroy(piece);
                        GameObject newPiecePrefab = new GameObject();

                        Vector3 position = offset + new Vector3(i * 2, 0f, j * 2);
                        if (i < 5 || i > 9) position += new Vector3(0f, 0.5f, 0);


                        GameObject newPiece = Instantiate(newPiecePrefab, position, rotation);
                        
                        newPiece.transform.parent = gameObject.transform;
                        Mesh holderMesh = new Mesh();
                        ObjImporter newMesh = new ObjImporter();

                        string type = Plate.plate[i][j].GetType().Name.ToLower();
                        Debug.Log(type);
                        if (type == "dragon")
                        {
                            newPiece.transform.localScale -= new Vector3(0.2f, 0.2f, 0.2f);
                            if (Plate.plate[i][j].player == 0)
                            {//blue
                                newPiece.transform.position += new Vector3(0.5f, 0.4f, 0.6f);
                            }
                            else
                            {//red
                                newPiece.transform.position += new Vector3(-0.5f, 0.4f, -0.8f);
                            }
                        }
                        if (type == "rook") { newPiece.transform.localScale -= new Vector3(0.5f, 0.5f, 0.5f); newPiece.transform.position += new Vector3(0f, 1f, 0f); }
                        if (type == "king") { newPiece.transform.localScale -= new Vector3(0.5f, 0.5f, 0.5f); newPiece.transform.position += new Vector3(0f, 1f, 0f); }


                        if (Plate.plate[i][j].player == 0)
                        {
                            newPiece.transform.position += new Vector3(-2.14f, -1f, -2.17f);
                            newPiece.transform.rotation = Quaternion.Euler(0, -90, 0);
                            type = type + "\\" + type + ".blue";

                        }
                        else
                        {
                            newPiece.transform.position += new Vector3(2.14f, -1f, 2.17f);
                            type = type + "\\" + type + ".red";
                        }
                        string meshPath = "D:\\workspace\\unity\\Chess\\Resources\\pieces\\" + type + ".obj";
                        string texPath = "D:\\workspace\\unity\\Chess\\Resources\\pieces\\" + type + ".png";



                        holderMesh = newMesh.ImportFile(meshPath);

                        MeshRenderer renderer = newPiece.AddComponent<MeshRenderer>();
                        renderer.material = new Material(Shader.Find("Nature/SpeedTree"));
                        MeshFilter filter = newPiece.AddComponent<MeshFilter>();
                        filter.mesh = holderMesh;

                        byte[] fileData = File.ReadAllBytes(texPath);
                        Texture2D tex = new Texture2D(2, 2);
                        tex.LoadImage(fileData);
                        renderer.material.SetTexture("_MainTex", tex);




                        //Debug.Log(MapGenerator.floors[1][1]);
                        Plate.floors[i][j].GetComponent<BoardSquare>().piece = newPiece;

                    }
                    else if (Plate.plate[i][j] == null && piece != null)
                    {
                        Destroy(piece);
                    }
                }
        }

        // Update is called once per frame
        void Update()
        {
            // Look for Mouse Inputs
            GetMouseInputs();
            if (Input.anyKeyDown && !(
            Input.GetMouseButtonDown(0)
         || Input.GetMouseButtonDown(1)
         || Input.GetMouseButtonDown(2)))
            {
                if (Input.GetKeyDown("j")) { print("j key was pressed"); answer = "J"; }
                if (Input.GetKeyDown("u")) { print("u key was pressed"); answer = "U"; }
                if (Input.GetKeyDown("i")) { print("i key was pressed"); answer = "I"; }
                if (Input.GetKeyDown("o")) { print("o key was pressed"); answer = "O"; }
                if (Input.GetKeyDown("p")) { print("p key was pressed"); answer = "P"; }

                print("sth key was pressed");

                if (mode == 1)
                {
                    if (answer == "O")
                    {
                        if (Plate.plate[Plate.selx][Plate.sely] != null && !Plate.plate[Plate.selx][Plate.sely].ismechanics())
                        {
                            if (Plate.selx > 0 && Plate.plate[Plate.selx - 1][Plate.sely] != null && Plate.plate[Plate.selx - 1][Plate.sely].ismechanics() && Plate.plate[Plate.selx - 1][Plate.sely].player == player ||
                                    Plate.sely > 0 && Plate.plate[Plate.selx][Plate.sely - 1] != null && Plate.plate[Plate.selx][Plate.sely - 1].ismechanics() && Plate.plate[Plate.selx][Plate.sely - 1].player == player ||
                                    Plate.selx < 14 && Plate.plate[Plate.selx + 1][Plate.sely] != null && Plate.plate[Plate.selx + 1][Plate.sely].ismechanics() && Plate.plate[Plate.selx + 1][Plate.sely].player == player ||
                                    Plate.sely < 14 && Plate.plate[Plate.selx][Plate.sely + 1] != null && Plate.plate[Plate.selx][Plate.sely + 1].ismechanics() && Plate.plate[Plate.selx][Plate.sely + 1].player == player)
                            {

                                isPush = true;
                                Debug.Log("info: start push mode!");
                                for (int i = 0; i < 15; ++i)
                                    for (int j = 0; j < 15; ++j)
                                        if ((i != Plate.selx - 2 && i != Plate.selx - 1 && i != Plate.selx + 1 && i != Plate.selx + 2 || j != Plate.sely) &&
                                                (j != Plate.sely - 2 && j != Plate.sely - 1 && j != Plate.sely + 1 && j != Plate.sely + 2 || i != Plate.selx))
                                        {
                                            Plate.plateCol[i][j] = Color.black;
                                        }
                            }
                            else
                            {
                                Debug.Log("info: cannot push!");
                            }
                        }
                    }
                    else//answer != o
                    {
                        isPush = false;
                        Debug.Log("info: exit push mode!");
                        Plate.colRefresh();
                        Plate.calMove(Plate.selx, Plate.sely);
                    }
                    if(answer=="I" || answer == "U"){
                        int response = Plate.listenKey(answer);
                        if(response==4){//assassin
                            Debug.Log("info: assassin moving!");
                            turnTurn();
                            return;
                        }
                        else if (response > 1)
                        {
                            Debug.Log("info: switched to skill " + answer + " mode " + response);
                            mode = response;
                            Plate.colRefresh();
                            Plate.calSkill();
                        }
                        else
                        {
                            answer = "J";
                            Plate.colRefresh();
                            Plate.calMove(Plate.selx, Plate.sely);
                            Debug.Log("info: do not have that skill. switch to J.");
                        }
                    }
                  


                }

                if (mode > 0 && answer == "J")
                {
                    mode = 1;
                    Plate.colRefresh();
                    Plate.calMove(Plate.selx, Plate.sely);
                    Debug.Log("info: switch to J.");
                }


                Plate.print();
                //mode = 0;
            }
        }



        // Detect Mouse Inputs
        void GetMouseInputs()
        {
            Ray _ray;
            RaycastHit _hitInfo;
            if (mode < 0)
            {
                // On Left Click
                if (Input.GetMouseButtonDown(0))
                {

                    _ray = PlayerCam.ScreenPointToRay(Input.mousePosition); // Specify the ray to be casted from the position of the mouse click

                    // Raycast and verify that it collided
                    if (Physics.Raycast(_ray, out _hitInfo))
                    {
                        // Select the piece if it has the good Tag
                        if (_hitInfo.collider.gameObject.tag == ("Cube"))
                        {
                            GameObject boardSquare = _hitInfo.collider.gameObject;
                            curx = boardSquare.GetComponent<BoardSquare>().posx;
                            cury = boardSquare.GetComponent<BoardSquare>().posy;
                            Piece piece;
                            if (Plate.plate[curx][cury] == null)
                            {
                                if (mode == -1) { piece = new LightPawn(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -2) { piece = new LightPawn(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -3) { piece = new LightPawn(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -4) { piece = new LightPawn(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -5) { piece = new LightPawn(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -6) { piece = new HeavyPawn(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -7) { piece = new HeavyPawn(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -8) { piece = new HeavyPawn(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -9) { piece = new HeavyPawn(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -10) { piece = new HeavyPawn(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -11) { piece = new bow(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -12) { piece = new bow(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -13) { piece = new Crossbow(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -14) { piece = new Crossbow(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -15) { piece = new Shield(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -16) { piece = new Shield(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -17) { piece = new Rook(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -18) { piece = new Rook(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -19) { piece = new HeavyHorse(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -20) { piece = new HeavyHorse(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -21) { piece = new LightHorse(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -22) { piece = new LightHorse(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -23) { piece = new Elephant(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -24) { piece = new Flag(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -25) { piece = new Ram(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -26) { piece = new Catapult(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -27) { piece = new WildFire(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -28) { piece = new Tower(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -29) { piece = new Assassin(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -30) { piece = new Dragon(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -31) { piece = new King(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -32) { piece = new Wall(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -33) { piece = new Wall(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -34) { piece = new Wall(); piece.player = 0; Plate.plate[curx][cury] = piece; }
                                if (mode == -35) { piece = new Wall(); piece.player = 0; Plate.plate[curx][cury] = piece; }

                                if (mode == -36) { piece = new LightPawn(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -37) { piece = new LightPawn(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -38) { piece = new LightPawn(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -39) { piece = new LightPawn(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -40) { piece = new LightPawn(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -41) { piece = new HeavyPawn(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -42) { piece = new HeavyPawn(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -43) { piece = new HeavyPawn(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -44) { piece = new HeavyPawn(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -45) { piece = new HeavyPawn(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -46) { piece = new bow(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -47) { piece = new bow(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -48) { piece = new Crossbow(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -49) { piece = new Crossbow(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -50) { piece = new Shield(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -51) { piece = new Shield(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -52) { piece = new Rook(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -53) { piece = new Rook(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -54) { piece = new HeavyHorse(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -55) { piece = new HeavyHorse(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -56) { piece = new LightHorse(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -57) { piece = new LightHorse(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -58) { piece = new Elephant(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -59) { piece = new Flag(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -60) { piece = new Ram(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -61) { piece = new Catapult(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -62) { piece = new WildFire(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -63) { piece = new Tower(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -64) { piece = new Assassin(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -65) { piece = new Dragon(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -66) { piece = new King(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -67) { piece = new Wall(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -68) { piece = new Wall(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -69) { piece = new Wall(); piece.player = 1; Plate.plate[curx][cury] = piece; }
                                if (mode == -70) {mode=-1;}// piece = new Wall(); piece.player = 1; Plate.plate[curx][cury] = piece; }

                                mode = mode + 1;
                                DrawPieces();
                            }
                        }
                    }
                }
            }


                                // Select a piece if the gameState is 0 or 1
            if (mode == 0)
            {
                // On Left Click
                if (Input.GetMouseButtonDown(0))
                {

                    _ray = PlayerCam.ScreenPointToRay(Input.mousePosition); // Specify the ray to be casted from the position of the mouse click

                    // Raycast and verify that it collided
                    if (Physics.Raycast(_ray, out _hitInfo))
                    {
                        // Select the piece if it has the good Tag
                        if (_hitInfo.collider.gameObject.tag == ("Cube"))
                        {
                            GameObject boardSquare = _hitInfo.collider.gameObject;
                            curx = boardSquare.GetComponent<BoardSquare>().posx;
                            cury = boardSquare.GetComponent<BoardSquare>().posy;
                            if (Plate.selectPiece())
                            {
                                Debug.Log("sel");
                                _GameManager.SelectPiece(boardSquare.GetComponent<BoardSquare>().piece);
                                mode = 1;
                                isPush = false;
                                issteal = false;
                                Plate.colRefresh();
                                Plate.calMove(curx, cury);
                                Plate.print();
                            }
                            else
                            {
                                Debug.Log("invalid sel");
                            }
                        }
                    }
                }
            }

            // Move the piece if the gameState is 1
            else if (mode == 1)
            {
                Vector3 selectedCoord;

                // On Left Click
                if (Input.GetMouseButtonDown(0))
                {
                    Plate.print();
                    _ray = PlayerCam.ScreenPointToRay(Input.mousePosition); // Specify the ray to be casted from the position of the mouse click

                    // Raycast and verify that it collided
                    if (Physics.Raycast(_ray, out _hitInfo))
                    {

                        // Select the piece if it has the good Tag
                        if (_hitInfo.collider.gameObject.tag == ("Cube"))
                        {
                            GameObject gameObject = _hitInfo.collider.gameObject;
                            selectedCoord = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 1, gameObject.transform.position.z);
                            curx = gameObject.GetComponent<BoardSquare>().posx;
                            cury = gameObject.GetComponent<BoardSquare>().posy;

                            Plate.colRefresh();
                            Plate.calMove(Plate.selx, Plate.sely);

                            issteal = false;
                            if (Plate.plate[Plate.selx][Plate.sely] != null && !Plate.plate[Plate.selx][Plate.sely].ismechanics())
                            {
                                if (Plate.selx > 0 && Plate.plate[Plate.selx - 1][Plate.sely] != null && Plate.plate[Plate.selx - 1][Plate.sely].ismechanics() && Plate.plate[Plate.selx - 1][Plate.sely].player != player ||
                                        Plate.sely > 0 && Plate.plate[Plate.selx][Plate.sely - 1] != null && Plate.plate[Plate.selx][Plate.sely - 1].ismechanics() && Plate.plate[Plate.selx][Plate.sely - 1].player != player ||
                                        Plate.selx < 14 && Plate.plate[Plate.selx + 1][Plate.sely] != null && Plate.plate[Plate.selx + 1][Plate.sely].ismechanics() && Plate.plate[Plate.selx + 1][Plate.sely].player != player ||
                                        Plate.sely < 14 && Plate.plate[Plate.selx][Plate.sely + 1] != null && Plate.plate[Plate.selx][Plate.sely + 1].ismechanics() && Plate.plate[Plate.selx][Plate.sely + 1].player != player)
                                {
                                    issteal = true;
                                    for (int i = Mathf.Max(0, Plate.selx - 2); i <= Mathf.Min(14, Plate.selx + 2); ++i)
                                        for (int j = Mathf.Max(0, Plate.sely - 2 + Mathf.Abs(Plate.selx - i)); j <= Mathf.Min(14, Plate.sely + 2 - Mathf.Abs(Plate.selx - i)); ++j)
                                            if (Plate.plate[i][j] != null && Plate.plate[i][j].player == 1 - player && !Plate.plate[i][j].ismechanics())
                                                issteal = false;
                                }
                                if (issteal) Debug.Log(" P 偷盗");
                            }


                            if (answer == "P" && issteal)
                            {
                                if (curx > 0 && Plate.plate[curx - 1][cury] != null && Plate.plate[curx - 1][cury].ismechanics()) Plate.plate[curx - 1][cury].player = player;
                                if (cury > 0 && Plate.plate[curx][cury - 1] != null && Plate.plate[curx][cury - 1].ismechanics()) Plate.plate[curx][cury - 1].player = player;
                                if (curx < 14 && Plate.plate[curx + 1][cury] != null && Plate.plate[curx + 1][cury].ismechanics()) Plate.plate[curx + 1][cury].player = player;
                                if (cury < 14 && Plate.plate[curx][cury + 1] != null && Plate.plate[curx][cury + 1].ismechanics()) Plate.plate[curx][cury + 1].player = player;
                                turnTurn();
                                return;
                            }
                            else if (answer == "K")
                            {
                                mode = 0; answer = "J";
                                Plate.colRefresh();
                                return;
                            }
                            else
                            {
                                Debug.Log("J!");
                                if (answer == "O") answer = "J";
                                int response = Plate.listenKey(answer);
                                Debug.Log("Respond!" + response);
                                if (response == 0)
                                { //移动，返回0
                                    Debug.Log("moving!");
                                    if (isPush) pushMec(Plate.selx, Plate.sely, curx, cury);
                                    GameObject oldPiece = Plate.floors[curx][cury].GetComponent<BoardSquare>().piece;
                                    if (oldPiece != null) Destroy(oldPiece);
                                    Plate.floors[curx][cury].GetComponent<BoardSquare>().piece = null;// Plate.floors[Plate.selx][Plate.sely].GetComponent<BoardSquare>().piece;
                                    //_GameManager.MovePiece(selectedCoord);
                                    //Plate.floors[Plate.selx][Plate.sely].GetComponent<BoardSquare>().piece = null;
                                    turnTurn();
                                    return;
                                }
                                else if (response != -1)
                                { //不是无效按键
                                  //!!!TODO skill
                                    mode = response;

                                    Plate.colRefresh();
                                    Plate.calSkill();
                                    return;
                                }
                                else
                                {
                                    mode = 0;
                                    answer = "J";
                                    Plate.colRefresh();
                                }
                            }
                            DrawPieces();

                        }
                        else
                        {
                            mode = 0; answer = "J";
                            Plate.colRefresh();
                        }
                    }
                    else
                    {
                        mode = 0; answer = "J";
                        Plate.colRefresh();
                    }
                }
            }
            else if (mode == 2)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Plate.print();
                    _ray = PlayerCam.ScreenPointToRay(Input.mousePosition); // Specify the ray to be casted from the position of the mouse click

                    // Raycast and verify that it collided
                    if (Physics.Raycast(_ray, out _hitInfo))
                    {

                        // Select the piece if it has the good Tag
                        if (_hitInfo.collider.gameObject.tag == ("Cube"))
                        {
                            GameObject gameObject = _hitInfo.collider.gameObject;
                            curx = gameObject.GetComponent<BoardSquare>().posx;
                            cury = gameObject.GetComponent<BoardSquare>().posy;

                            if (Plate.releaseSkill())
                            {
                                turnTurn();
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
