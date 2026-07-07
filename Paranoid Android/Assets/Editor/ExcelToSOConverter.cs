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

                    ItemType it;
                    string typeStr = GetValue("ItemType");
                    if (System.Enum.TryParse(typeStr, out ItemType t))
                    {
                        it = t;
                    }
                    else
                    {
                        it = ItemType.Loot; // 默认 Loot
                    }

                    string assetPath = $"{soSavePath}Item_{id}.asset";

                    ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);

                    if (item == null)
                    {
                        if (it == ItemType.Weapon)
                            item = ScriptableObject.CreateInstance<WeaponData>();
                        else
                            item = ScriptableObject.CreateInstance<ItemData>();

                        item.itemType = it;

                        AssetDatabase.CreateAsset(item, assetPath);
                    }
                    else if (item.itemType != it)
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                        if (it == ItemType.Weapon)
                            item = ScriptableObject.CreateInstance<WeaponData>();
                        else
                            item = ScriptableObject.CreateInstance<ItemData>();

                        item.itemType = it;
                        AssetDatabase.CreateAsset(item, assetPath);
                    }

                    item.itemID = id;
                    item.itemName = GetValue("Name");
                    item.description = GetValue("Description");
                    int.TryParse(GetValue("MaxStack"), out int maxStack);
                    item.maxStack = maxStack == 0 ? 1 : maxStack;
                    float.TryParse(GetValue("Weight"), out float w);
                    item.weight = w;
                    item.value = int.Parse(GetValue("Value"));
                    item.prefabAddress = item.itemName;
                    item.iconAddress = item.itemName + "Icon";

                    if (item is WeaponData weapon)
                    {
                        float.TryParse(GetValue("FireRate"), out float fr);
                        weapon.fireRate = fr;

                        float.TryParse(GetValue("LoadPerShot"), out float lps);
                        weapon.loadPerShot = lps;

                        float.TryParse(GetValue("BaseSpread"), out float bs);
                        weapon.baseSpread = bs;

                        float.TryParse(GetValue("AimSpreadMult"), out float asm);
                        weapon.aimSpreadMult = asm;

                        float.TryParse(GetValue("AimSpeed"), out float asp);
                        weapon.aimSpeed = asp;

                        int.TryParse(GetValue("Damage"), out int dmg);
                        weapon.damage = dmg;

                        float.TryParse(GetValue("BulletSpeed"), out float bsp);
                        weapon.bulletSpeed = bsp;

                        float.TryParse(GetValue("Distance"), out float dist);
                        weapon.distance = dist;

                        string bulletTypeStr = GetValue("BulletType");
                        if (System.Enum.TryParse(bulletTypeStr, out BulletType bType))
                        {
                            weapon.bulletType = bType;
                        }
                        else
                        {
                            weapon.bulletType = BulletType.Normal;
                            Debug.LogWarning($"[ExcelConverter] 物品 {weapon.itemName} 的 BulletType 配置有误或为空：'{bulletTypeStr}'，已默认设为 Normal。");
                        }

                        weapon.modelAddress = weapon.itemName + "Model";
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