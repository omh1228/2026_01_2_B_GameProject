#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using Unity.VisualScripting;
using NUnit.Framework.Constraints;



public enum ConversionType
{
    Items,
    Dialogs
}

[Serializable]
public class DialogRowData
{
    public int? id;
    public string characterName;
    public string text;
    public int? nextid;
    public string portraitPath;
    public string choiceText;
    public int? choiceNextid;
}

public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilepath = "";
    private string outputFolder = "Assets/ScriptableObjects/Items";
    private bool createDatabase = true;
    private ConversionType conversionType = ConversionType.Items;

    [MenuItem("Tools/JSON to Scriptable Objects")]

    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSON to Scriptable Objecta");
    }

    void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable object Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if(GUILayout.Button("Select JSON File"))
        {
            jsonFilepath = EditorUtility.OpenFilePanel("Select JSON File", "", "JSON");
        }

        EditorGUILayout.LabelField("Selected File :", jsonFilepath);
        EditorGUILayout.Space();

        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type: ", conversionType);


        if(conversionType == ConversionType.Items && outputFolder == "Assets/Scriptab;eObjects")
        {
            outputFolder = "Assets/ScriptableObtects/Items";
        }
        else if (conversionType == ConversionType.Dialogs && outputFolder == "Assets/ScriptableObjects")
        {
            outputFolder = "Assets/ScriptableObjects/Dialogs";
        }
            outputFolder = EditorGUILayout.TextField("output Foloder : ", outputFolder);
        createDatabase = EditorGUILayout.Toggle("Create Database Asset", createDatabase);
        EditorGUILayout.Space();

        if (GUILayout.Button("convert to Scriptable Objects"))
        {
            if(string.IsNullOrEmpty(jsonFilepath))
            {
                EditorUtility.DisplayDialog("Error", "Pease Select a JSON file first", "OK");
                return;
            }
            
            switch(conversionType)
            {
                case ConversionType.Items:
                    ConvertJsonToItemScriptableObjects();
                    break;
                case ConversionType.Dialogs:
                    ConvertJsonToDiaLogScriptablejects();
                    break;
            }
        }
    }

    private void ConvertJsonToItemScriptableObjects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string jsonText = File.ReadAllText(jsonFilepath);

        try
        {
            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createdItems = new List<ItemSO>();

            foreach (ItemData itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();

                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                
                if (System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"아이템 {itemData.itemName}의 유효하지 않은 타입 : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;


                if(!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");

                    if (itemSO.icon == null)
                    {
                        Debug.LogWarning($"아이템 {itemData.nameEng} 의 아이콘을 찾을 수 없습니다. : {itemData.iconPath}");
                    }
                }



                string assePath = $"{outputFolder}/item_{itemData.id.ToString("D4")}_{itemData.nameEng}.assets";
                AssetDatabase.CreateAsset(itemSO, assePath);

                itemSO.name = $"Item_{itemData.id.ToString("D4")} + {itemData.nameEng}";
                createdItems.Add(itemSO);

                EditorUtility.SetDirty(itemSO);

                if (createDatabase && createdItems.Count > 0)
                {
                    ItemDataBaseSO dataBase = ScriptableObject.CreateInstance<ItemDataBaseSO>();
                    dataBase.items = createdItems;

                    AssetDatabase.CreateAsset(dataBase, $"{outputFolder}/ItemDatabase.asset");
                    EditorUtility.SetDirty(dataBase);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Sucess", $"Created {createdItems.Count} scriptable objects!", "OK");
            }

        }
        catch(System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to Conver JSON : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }

    private void ConvertJsonToDiaLogScriptablejects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string JsonText = File.ReadAllText(jsonFilepath);

        try
        {
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(JsonText);

            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> createDialogs = new List<DialogSO>();

            //1단계 : 대화 항목 생성
            foreach(var rowData in rowDataList)
            {
                if (!rowData.id.HasValue)
                    continue;
                DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                dialogSO.id = rowData.id.Value;
                dialogSO.characterName = rowData.characterName;
                dialogSO.text = rowData.text;
                dialogSO.nextld = rowData.nextid.HasValue ? rowData.nextid.Value : -1;
                dialogSO.portraitPath = rowData.portraitPath;
                dialogSO.choices = new List<DialogChoiceSO>();

                if (!string.IsNullOrEmpty(rowData.portraitPath))
                {
                    dialogSO.portrait = Resources.Load<Sprite>(rowData.portraitPath);
                        
                    if (dialogSO.portrait == null)
                    {
                        Debug.LogWarning($"대화 {rowData.id}의 초상화를 찾을 수 없습니다.");
                    }
                }
                dialogMap[dialogSO.id] = dialogSO;
                createDialogs.Add(dialogSO);
            }
            //2단계 : 선택시 항목 처리 및 연결
            foreach (var rowData in rowDataList)
            {
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextid.HasValue)
                {
                    int parentid = -1;

                    int currentindex = rowDataList.IndexOf(rowData);
                    for (int i = currentindex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentid = rowDataList[i].id.Value;
                            break;
                        }
                    }

                    if(parentid == -1)
                    {
                        Debug.LogWarning($"선택지 {rowData.choiceText} 의 부모 대화를 찾을 수 없습니다. ");
                    }

                    if(dialogMap.TryGetValue(parentid , out DialogSO parentDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextid = rowData.choiceNextid.Value;

                        string choiceAssetpath = $"{outputFolder}/Choice_{parentid}_{parentDialog.choices.Count + 1}.asset";
                        AssetDatabase.CreateAsset(choiceSO, choiceAssetpath );
                        EditorUtility.SetDirty(choiceSO);
                        parentDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.LogWarning($"선택지 {rowData.choiceText}를 연결할 대화 (ID : {parentid}를 찾을 수 없습니다.");
                    }
                }
            }
            //3단계 : 대화 스크립터블 오브젝트 저장
            foreach(var dialog in createDialogs)
            {

                string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset(dialog, assetPath);

                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";

                EditorUtility.SetDirty(dialog);
            }

            if(createDatabase && createDialogs.Count > 0)
            {
                DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                database.dialogs = createDialogs;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/DialogDatabase.asset");
                EditorUtility.SetDirty(database);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Created {createDialogs.Count} dialogMap scriptable objects!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Faild to convert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }
}


#endif
