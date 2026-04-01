using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Collections;

public class DialogManager : MonoBehaviour
{ 
    public static DialogManager Instance { get; private set;}

    [Header("Dialog Reterences")]
    [SerializeField] private DialogDatabaseSO dialogDatabase;

    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;

    [SerializeField] private Image portrimage;

    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button NextButton;

    [Header("Dialog Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffect = true;

    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private DialogSO currentDilog;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (dialogDatabase != null)
        {
            dialogDatabase.Initalize();
        }
        else
        {
            Debug.LogError("Dialog Database is not assinged to Dialog Manager");
        }

        if (NextButton != null)
        {
            NextButton.onClick.AddListener(NextDialog);
        }
        else
        {
            Debug.LogError("Next Nutton is not assigned!");
        }

    }

    public void StartDialog(int dialogld)
    {
        DialogSO dialog = dialogDatabase.GetDialongByd(dialogld);
        if(dialog != null)
        {
            StartDialog(dialog);
        }
        else
        {
            Debug.LogError($"Dialog with ID {dialogld} not found!");
        }
    }

    public void StartDialog(DialogSO dialog)
    {
        if (dialog == null) return;

        currentDilog = dialog;
        ShowDialog();
        dialogPanel.SetActive(true);
    }

    public void ShowDialog()
    {
        Debug.Log(currentDilog.portraitPath);

        if (currentDilog == null) return;
        characterNameText.text = currentDilog.characterName;

        if(useTypewriterEffect)
        {
            StartTypingEffect(currentDilog.text);
        }
        else
        {
            dialogText.text = currentDilog.text;
        }
         
        
        if(currentDilog.portrait != null)
        {
            portrimage.sprite = currentDilog.portrait;
            portrimage.gameObject.SetActive(true);
        }
        else if (!string.IsNullOrEmpty(currentDilog.portraitPath))
        {
            Sprite portrait = Resources.Load<Sprite>(currentDilog.portraitPath);
            if (portrait != null)
            {
                portrimage.sprite = portrait;
                portrimage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"portrait not found at path : {currentDilog.portraitPath}");
                portrimage.gameObject.SetActive(false);
            }
        }
        else
        {
            portrimage.gameObject.SetActive(false);
        }
    }


    public void CloseDialog()
    {
        dialogPanel.SetActive(false);
        currentDilog = null;
        StopTypingEffect();
    }


    public void NextDialog()
    {
        if(isTyping)
        {
            StopTypingEffect();
            dialogText.text = currentDilog.text;
            isTyping = false;
            return;
        }

        if (currentDilog != null && currentDilog.nextld > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialongByd(currentDilog.nextld);
            if (nextDialog != null)
            {
                currentDilog = nextDialog;
                ShowDialog();
            }
            else
            {
                CloseDialog();
            }
        }
        else
        {
            CloseDialog();
        }
    }

    private void Start()
    {
        CloseDialog();
        StartDialog(1);
    }

    private IEnumerator TypeText(string text)
    {
        dialogText.text = "";
        foreach(char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false; 
    }

    private void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    private void StartTypingEffect(string text)
    {
        isTyping = true;
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }
}

