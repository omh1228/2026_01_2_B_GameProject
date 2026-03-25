using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogSO", menuName = "Dialog System/DialogSO")]
public class DialogSO : ScriptableObject
{
    public int id;
    public string characterName;
    public string text;
    public int nextld;

    public List<DialogChoiceSO> choices = new List<DialogChoiceSO>();
    public Sprite portrait;

    public string portraitPath;
}
