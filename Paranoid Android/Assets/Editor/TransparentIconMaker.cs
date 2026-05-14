using UnityEngine;
using UnityEditor;
using System.IO;

public class TransparentIconMaker : EditorWindow
{
    [MenuItem("Tools/Generate Item Icon")]
    public static void CreateIcons()
    {
        // 1. 获取选中的 Prefab
        GameObject[] targets = Selection.gameObjects;
        if (targets.Length == 0) return;

        // 2. 创建临时渲染相机
        GameObject camObj = new GameObject("TempIconCam");
        Camera cam = camObj.AddComponent<Camera>();

        // --- 关键设置 ---
        cam.backgroundColor = new Color(0, 0, 0, 0); // 透明背景
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.orthographic = true; // 正交相机更适合 UI 图标
        cam.nearClipPlane = 0.01f;

        // 创建 RenderTexture
        RenderTexture rt = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;

        foreach (GameObject prefab in targets)
        {
            // 3. 实例化对象并放到相机前
            GameObject instance = Instantiate(prefab);
            instance.transform.position = Vector3.zero;

            // 自动调整相机位置和缩放以包围物体
            SetupCamera(cam, instance);

            // 4. 渲染并保存
            cam.Render();
            SaveRTToPNG(rt, prefab.name);

            DestroyImmediate(instance);
        }

        // 5. 清理
        cam.targetTexture = null;
        DestroyImmediate(camObj);
        AssetDatabase.Refresh();
        Debug.Log("所有图标已生成到 Assets/Sprites/Icons 文件夹！");
    }

    private static void SetupCamera(Camera cam, GameObject target)
    {
        // 获取物体及其子物体的边界
        Bounds bounds = new Bounds(target.transform.position, Vector3.zero);
        foreach (var renderer in target.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // 设置相机视角（略微倾斜的俯视角，45度）
        cam.transform.position = bounds.center + new Vector3(0, 0, -1).normalized * 5f;
        cam.transform.LookAt(bounds.center);

        // 自动缩放
        cam.orthographicSize = bounds.extents.magnitude * 1.2f;
    }

    private static void SaveRTToPNG(RenderTexture rt, string fileName)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = prev;

        byte[] bytes = tex.EncodeToPNG();
        string dir = Application.dataPath + "/Sprites/Icons";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes($"{dir}/{fileName}.png", bytes);
    }
}