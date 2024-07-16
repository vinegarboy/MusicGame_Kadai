using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data;
using Cysharp.Threading.Tasks;

public class Note{
    public float time;
    public int tone;

    public Note(float time,int tone){
        this.time = time;
        this.tone = tone;
    }
}

public class GameManager : MonoBehaviour{
    [SerializeField]
    AudioSource audioSource;

    public AudioClip audioClip;

    [SerializeField]
    public GameObject NoteObject;

    public float noteSpeed = 5f;
    public float offset = 0f;
    public float silentTime = 3f;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    int score = 0;
    int combo = 0;

    public GameObject waitPanel;

    List<Note> notes;

    bool playing = false;

    [SerializeField]
    GameObject[] tapArea;

    [SerializeField]
    GameObject Miss;

    [SerializeField]
    GameObject Good;

    [SerializeField]
    GameObject Perfect;

    async void Start(){
        scoreText.text = $"Score:{score}";
        comboText.text = $"Combo:{combo}";
        notes = new List<Note>();
        MusicAnalyze musicAnalyze = new MusicAnalyze(audioClip,2048,512);

        await UniTask.RunOnThreadPool(()=>musicAnalyze.Analyze());
        notes = await UniTask.RunOnThreadPool(()=>musicAnalyze.GetNotes());

        waitPanel.SetActive(false);
        foreach(Note note in notes){
            GameObject note_Object = Instantiate(NoteObject);
            note_Object.transform.parent = transform;
            note_Object.transform.localPosition = new Vector3(tapArea[Random.Range(0,6)].transform.position.x,noteSpeed*(note.time+silentTime)-3+offset,0);
            note_Object.GetComponent<NoteScript>().manager = this;
        }
        transform.position = Vector3.zero;
        audioSource.clip = audioClip;
        playing = true;
        await UniTask.Delay((int)(silentTime * 1000));
        audioSource.Play();
    }

    void Update(){
        if(playing){
            transform.Translate(0,-Time.deltaTime*noteSpeed ,0);
            if(Input.GetKeyDown(KeyCode.S)){
                RaycastHit hit;
                if (Physics.Raycast(tapArea[0].transform.position,new Vector3(0,1,0),out hit ,8.5f)){
                    Tap(hit);
                }
            }
            if(Input.GetKeyDown(KeyCode.D)){
                RaycastHit hit;
                if (Physics.Raycast(tapArea[1].transform.position,new Vector3(0,1,0),out hit ,8.5f)){
                    Tap(hit);
                }
            }
            if(Input.GetKeyDown(KeyCode.F)){
                RaycastHit hit;
                if (Physics.Raycast(tapArea[2].transform.position,new Vector3(0,1,0),out hit ,8.5f)){
                    Tap(hit);
                }
            }
            if(Input.GetKeyDown(KeyCode.J)){
                RaycastHit hit;
                if (Physics.Raycast(tapArea[3].transform.position,new Vector3(0,1,0),out hit ,8.5f)){
                    Tap(hit);
                }
            }
            if(Input.GetKeyDown(KeyCode.K)){
                RaycastHit hit;
                if (Physics.Raycast(tapArea[4].transform.position,new Vector3(0,1,0),out hit ,8.5f)){
                    Tap(hit);
                }
            }
            if(Input.GetKeyDown(KeyCode.L)){
                RaycastHit hit;
                if (Physics.Raycast(tapArea[5].transform.position,new Vector3(0,1,0),out hit ,8.5f)){
                    Tap(hit);
                }
            }

            if(audioSource.time >= audioClip.length){
                playing = false;
                ScoreManager.now_score = score;
                if(ScoreManager.max_combo < combo){
                    ScoreManager.max_combo = combo;
                }
                UnityEngine.SceneManagement.SceneManager.LoadScene("Result");
            }
        }
    }

    bool Tap(RaycastHit hit){
        if(hit.collider.gameObject.CompareTag("Note")){
            Destroy(hit.collider.gameObject);
            AddScore(hit.distance);
            ShowParticle(hit.distance,hit.collider.gameObject.transform.position);
            return true;
        }

        return false;
    }

    public void ShowParticle(float diff,Vector3 position){
        if(diff>0.5f){
            Instantiate(Miss,position,Quaternion.Euler(-90, 0, 0));
        }
        else if(diff>0.1f){
            Instantiate(Good,position,Quaternion.Euler(-90, 0, 0));
        }
        else{
            Instantiate(Perfect,position,Quaternion.Euler(-90, 0, 0));
        }
    }

    public void AddScore(float diff){
        int base_score;
        if(diff>0.5f){
            Debug.Log("Miss");
            combo =0;
        }
        else{
            if(diff < 0.5f){
                base_score = 3;
                Debug.Log($"Good! diff:{diff}");
            }
            else if(diff > 0.2f){
                base_score = 6;
                Debug.Log($"diff:{diff}");
            }
            else{
                base_score = 10;
                Debug.Log($"diff:{diff}");
            }
            combo += 1;
            int coeffivient = (int)(Mathf.Log(combo,2)-5);
            score += coeffivient < 0 ? base_score:base_score*(int) Mathf.Pow(2,coeffivient);
        }
        scoreText.text = $"Score:{score}";
        comboText.text = $"Combo:{combo}";
        ScoreManager.now_score = score;
        if(ScoreManager.max_combo < combo){
            ScoreManager.max_combo = combo;
        }
    }
}
