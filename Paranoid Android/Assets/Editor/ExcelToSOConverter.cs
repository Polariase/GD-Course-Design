using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Data;
using ExcelDataReader;

public class ExcelToSOConverter
{
    // 配置路径
    private static string excelPath = Path.Combine(Application.dataPath, "Configs/ItemTable.xlsx");
    private static string soSavePath = "Assets/GameData/Items/";

    [MenuItem("Tools/Import Items From Excel")]
    public static void Import()
    {
        if (!Directory.Exists(soSavePath))
            Directory.CreateDirectory(soSavePath);

        using (var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                var sheet = result.Tables[0];

                if (sheet.Rows.Count < 2)
                {
                    Debug.LogError("Excel");
                    return;
                }

                Dictionary<string, int> headerMap = new Dictionary<string, int>();
                DataRow headerRow = sheet.Rows[0];

                for (int col = 0; col < sheet.Columns.Count; col++)
                {
                    string headerName = headerRow[col]?.ToString().Trim();
                    if (!string.IsNullOrEmpty(headerName) && !headerMap.ContainsKey(headerName))
                    {
                        headerMap.Add(headerName, col);
                    }
                }

                for (int i = 1; i < sheet.Rows.Count; i++)
                {
                    DataRow row = sheet.Rows[i];

                    string GetValue(string columnName)
                    {
                        if (headerMap.TryGetValue(columnName, out int index))
                        {
                            return row[index]?.ToString() ?? "";
                        }
                        return "";
                    }

                    string idStr = GetValue("ItemID");
                    if (string.IsNullOrEmpty(idStr)) continue;

                    int id = int.Parse(idStr);

                    //查找或创建 ScriptableObject
                    string assetPath = $"{soSavePath}Item_{id}.asset";
                    ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);

                    if (item == null)
                    {
                        item = ScriptableObject.CreateInstance<ItemData>();
                        AssetDatabase.CreateAsset(item, assetPath);
                    }

                    item.itemID = id;
                    item.itemName = GetValue("Name");
                    item.description = GetValue("Description");
                    item.prefabAddress = GetValue("PrefabAddress");
                    item.iconAddress = GetValue("IconAddress");
                    int.TryParse(GetValue("MaxStack"), out int maxStack);
                    item.maxStack = maxStack == 0 ? 1 : maxStack;
                    float.TryParse(GetValue("Weight"), out float w);
                    item.weight = w;

                    string typeStr = GetValue("ItemType");
                    if (System.Enum.TryParse(typeStr, out ItemType t))
                    {
                        item.itemType = t;
                    }
                    else
                    {
                        item.itemType = ItemType.Loot; // 默认 Loot
                    }

                    EditorUtility.SetDirty(item);
                }
            }
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Excel 数据导入成功！");
    }
}