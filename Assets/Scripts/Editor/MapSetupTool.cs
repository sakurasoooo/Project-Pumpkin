using UnityEngine;
using UnityEditor;

public class MapSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Infinite Map Scroller")]
    public static void ShowWindow()
    {
        GetWindow<MapSetupTool>("Map Setup Tool");
    }

    private Sprite backgroundSprite;
    private Sprite grassSprite;
    private Sprite dirtSprite;

    private float backgroundSpeed = 0.3f;
    private float grassSpeed = 1f;
    private float dirtSpeed = 1f;

    private void OnGUI()
    {
        GUILayout.Label("Infinite Map Scroller Setup", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        backgroundSprite = (Sprite)EditorGUILayout.ObjectField("背景层 Sprite", backgroundSprite, typeof(Sprite), false);
        backgroundSpeed = EditorGUILayout.FloatField("背景层视差", backgroundSpeed);
        
        EditorGUILayout.Space();

        grassSprite = (Sprite)EditorGUILayout.ObjectField("草地层 Sprite", grassSprite, typeof(Sprite), false);
        grassSpeed = EditorGUILayout.FloatField("草地层视差", grassSpeed);
        
        EditorGUILayout.Space();

        dirtSprite = (Sprite)EditorGUILayout.ObjectField("泥土层 Sprite", dirtSprite, typeof(Sprite), false);
        dirtSpeed = EditorGUILayout.FloatField("泥土层视差", dirtSpeed);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Map Scroller Objects"))
        {
            GenerateMap();
        }
    }

    private void GenerateMap()
    {
        GameObject root = new GameObject("MapEnvironment");

        if (backgroundSprite != null)
            CreateLayer(root.transform, "BackgroundLayer", backgroundSprite, backgroundSpeed, -5f);
            
        if (dirtSprite != null)
            CreateLayer(root.transform, "DirtLayer", dirtSprite, dirtSpeed, -1f);
            
        if (grassSprite != null)
            CreateLayer(root.transform, "GrassLayer", grassSprite, grassSpeed, 0f);

        Debug.Log("Map Environment Generated Successfully!");
    }

    private void CreateLayer(Transform parent, string layerName, Sprite sprite, float parallaxFactor, float zPos)
    {
        GameObject layerObj = new GameObject(layerName);
        layerObj.transform.SetParent(parent);
        layerObj.transform.localPosition = new Vector3(0, 0, zPos);

        MapScroller scroller = layerObj.AddComponent<MapScroller>();
        scroller.parallaxFactor = parallaxFactor;
        
        // Calculate sprite width in world units
        float spriteWidth = sprite.bounds.size.x;
        scroller.spriteWidth = spriteWidth;

        // Create Image A
        GameObject imgA = new GameObject($"{layerName}_A");
        imgA.transform.SetParent(layerObj.transform);
        imgA.transform.localPosition = Vector3.zero;
        SpriteRenderer srA = imgA.AddComponent<SpriteRenderer>();
        srA.sprite = sprite;
        srA.sortingOrder = Mathf.RoundToInt(zPos * 10);

        // Create Image B
        GameObject imgB = new GameObject($"{layerName}_B");
        imgB.transform.SetParent(layerObj.transform);
        imgB.transform.localPosition = new Vector3(spriteWidth, 0, 0);
        SpriteRenderer srB = imgB.AddComponent<SpriteRenderer>();
        srB.sprite = sprite;
        srB.sortingOrder = Mathf.RoundToInt(zPos * 10);
    }
}
