using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public class PrefabGenerator : EditorWindow {
    // Tạo Enum để chọn mục tiêu lưu trữ
    public enum FolderOption { Math, Unicode }
    public FolderOption selectedFolder = FolderOption.Math;

    public string charactersToCreate = "∫Σ√π∄"; 
    public TMP_FontAsset fontAsset;
    public float defaultFontSize = 20f;
    public Vector2 defaultRectSize = new Vector2(20, 20);

    [MenuItem("Tools/Generate Symbol Prefabs")]
    public static void ShowWindow() {
        GetWindow<PrefabGenerator>("Symbol Gen");
    }

    void OnGUI() {
        GUILayout.Label("Thiet lap Prefab Ky tu", EditorStyles.boldLabel);
        
        // Hiển thị Dropdown chọn thư mục
        selectedFolder = (FolderOption)EditorGUILayout.EnumPopup("Chon thu muc luu", selectedFolder);

        charactersToCreate = EditorGUILayout.TextField("Danh sach ky tu", charactersToCreate);
        fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", fontAsset, typeof(TMP_FontAsset), false);
        
        defaultFontSize = EditorGUILayout.FloatField("Co chu (Font Size)", defaultFontSize);
        defaultRectSize = EditorGUILayout.Vector2Field("Kich thuoc khung (Size)", defaultRectSize);

        if (GUILayout.Button("Generate Prefabs")) {
            if (fontAsset == null) {
                Debug.LogError("Vui long keo Font Asset vao truoc khi bam!");
                return;
            }
            Generate();
        }
    }

    void Generate() {
        string subFolder = selectedFolder.ToString();
        string finalPath = Path.Combine("Assets/Prefabs/BaseParts", subFolder);

        if (!AssetDatabase.IsValidFolder(finalPath)) {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/BaseParts")) {
                Directory.CreateDirectory("Assets/Prefabs/BaseParts");
            }
            Directory.CreateDirectory(finalPath);
            AssetDatabase.Refresh();
        }

        // Lay danh sach cac ky tu cam cua Windows
        char[] invalidChars = Path.GetInvalidFileNameChars();

        foreach (char c in charactersToCreate) {
            string fileName = "";

            // 1. Xu ly chu cai (Phan biet Hoa/Thuong nhu cu)
            if (char.IsLetter(c)) {
                fileName = char.IsUpper(c) ? c + "_Upper" : c.ToString();
            }
            // 2. Xu ly ky tu dac biet
            else {
                // Kiem tra xem Windows co cho phep ky tu nay lam ten file khong
                bool isInvalid = false;
                foreach (char invalid in invalidChars) {
                    if (c == invalid) { isInvalid = true; break; }
                }

                if (isInvalid || char.IsWhiteSpace(c)) {
                    // Neu bi cam hoac la dau cach -> Dung ma ASCII
                    fileName = "Symbol_" + ((int)c).ToString();
                } else {
                    // Neu Windows cho phep -> Dung luon ky tu do lam ten
                    fileName = c.ToString();
                }
            }

            GameObject go = new GameObject("Part_" + fileName);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = defaultRectSize; // Size 20x20

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = c.ToString();
            tmp.font = fontAsset;
            tmp.fontSize = defaultFontSize; // Font size 20
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            string localPath = Path.Combine(finalPath, fileName + ".prefab");
            
            PrefabUtility.SaveAsPrefabAsset(go, localPath);
            DestroyImmediate(go);
        }
        AssetDatabase.Refresh();
        Debug.Log("Da tao xong tai: " + finalPath);
    }
}