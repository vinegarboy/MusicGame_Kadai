using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using System.Data;

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

    async void Start(){
        scoreText.text = $"Score:{score}";
        comboText.text = $"Combo:{combo}";
        notes = new List<Note>();
        MusicAnalyze musicAnalyze = new MusicAnalyze(audioClip,2048,512);

        await Task.Run(()=>musicAnalyze.Analyze());
        notes = await Task.Run(()=>musicAnalyze.GetNotes());

        waitPanel.SetActive(false);
        foreach(Note note in notes){
            GameObject note_Object = Instantiate(NoteObject);
            note_Object.transform.parent = transform;
            note_Object.transform.localPosition = new Vector3(Mathf.Abs((float)note.tone-5.5f)-3,noteSpeed*(note.time+silentTime)-3+offset,0);
            note_Object.GetComponent<NoteScript>().manager = this;
        }
        transform.position = Vector3.zero;
        audioSource.clip = audioClip;
        playing = true;
        await Task.Delay((int)(silentTime * 1000));
        audioSource.Play();
    }

    void Update(){
        if(playing){
            transform.Translate(0,-Time.deltaTime*noteSpeed ,0);
            if(Input.GetMouseButtonDown(0)){
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray.origin,ray.direction,out hit ,Mathf.Infinity,9)){
                    if(!Tap(hit)){
                        Vector3 rayDirection = new Vector3(0,1,0);
                        Vector3 origin = new Vector3(hit.point.x , -4.5f,0.001f);
                        if(Physics.Raycast(origin,rayDirection,out hit , 3f)){
                            Tap(hit);
                        }
                    }
                }
            }
        }
    }

    bool Tap(RaycastHit hit){
        if(hit.collider.gameObject.CompareTag("Note")){
            Destroy(hit.collider.gameObject);
            float diff= Mathf.Abs(hit.collider.gameObject.transform.position.y+3);
            AddScore(diff/noteSpeed);
            return true;
        }

        return false;
    }

    public void AddScore(float diff){
        int base_score;
        if(diff<0.3f){
            Debug.Log("Miss");
            combo =0;
        }
        else{
            if(diff < 0.2f){
                base_score = 3;
                Debug.Log($"Good! diff:{diff}");
            }
            else if(diff > 0.1f){
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
    }
}
