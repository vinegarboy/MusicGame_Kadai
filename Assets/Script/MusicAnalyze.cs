using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MusicAnalyze{
    private float sdb_max = 0;
    float[,] sdb;
    private AudioClip audio;
    private int nframe;
    private int noverlap;
    int samples;
    int channels;
    int frequency;
    float[] allSamples;

    public MusicAnalyze(AudioClip audioClip,int nframe = 1024,int noverlap = 256){
        this.audio = audioClip;
        this.nframe = nframe;
        this.noverlap = noverlap;
        this.samples = audio.samples;
        this.channels = audio.channels;
        this.frequency = audio.frequency;
        allSamples = new float[this.samples*this.channels];
        audio.GetData(allSamples,0);
    }

    public void Analyze(){
        float[] samples = new float[this.samples];
        for(int i = 0;i<this.samples;i++){
            samples[i] = allSamples[i*this.channels]/this.channels;
            for(int ch = 1;ch<this.channels;ch++){
                samples[i] += allSamples[i*this.channels+ch]/this.channels;
            }
        }
        sdb = Utils.Spectrogram(samples,nframe,noverlap);
        sdb_max = sdb.Cast<float>().Max();
        for(int x= 0;x<sdb.GetLength(0);x++){
            for(int y = 0;y>sdb.GetLength(1);y++){
                sdb[x,y] = db_min(sdb[x,y]);
            }
        }
    }

    private float db_min(float i ,float min = -80f){
        return i - sdb_max<min?min:i -sdb_max;
    }

    public Texture2D GetTexture2DSample(int sampleSize){
        if(sampleSize > sdb.GetLength(0)){
            sampleSize = sdb.GetLength(0);
        }
        Texture2D texture = new Texture2D(sampleSize,sdb.GetLength(1),TextureFormat.RGBA32,false);
        var data = texture.GetRawTextureData<Color32>();
        int index = 0;
        for(int x = 0;x<sdb.GetLength(1);x++){
            for(int y = 0;y<sampleSize;y++){
                byte color = (byte)((int)255+sdb[y,x]*255/80);
                data[index++] = new Color32(color,color,color,255);
            }
        }
        texture.Apply();
        return texture;
    }

    public List<Note> GetNotes(){
        List<Note> notes = new List<Note>();
        float[] levels = new float[sdb.GetLength(0)];
        levels[0] = 0;
        for(int x = 1;x<sdb.GetLength(0);x++){
            levels[x] = 0;
            for(int y = 0;y<sdb.GetLength(1)/2;y++){
                levels[x] += sdb[x,y];
            }
        }
        float[] onset_envelope = new float[sdb.GetLength(0)];
        for(int i = 0;i<levels.Length;i++){
            try{
                onset_envelope[i] = levels[i] - levels[i-1];
                if(onset_envelope[i] < 0){
                    onset_envelope[i] = 0;
                }
            }
            catch (System.IndexOutOfRangeException){
                onset_envelope[i] = 0;
            }
        }
        List<int> onsets = Utils.PeakPickIndex(onset_envelope,15,15,15,15,1f,5);
        for(int i = 0;i<onsets.Count;i++){
            Note note = new Note(1f/this.frequency*noverlap*onsets[i],GetMaxScaleDemo(onsets[i]));
            notes.Add(note);
        }
        return notes;
    }
    public int GetMaxScaleDemo(int xIndex){
        float ratio = Mathf.Pow(2f,1f/12);
        float power_max = Get_Power_from_Hz(xIndex,440*Mathf.Pow(ratio,0));
        int indexAtMax = 0;
        for(int i = 1;i<12;i++){
            float powers = Get_Power_from_Hz(xIndex,440*Mathf.Pow(ratio,i));
            if(power_max < powers){
                indexAtMax = i;
                power_max = powers;
            }
        }
        return indexAtMax;
    }

    public float Get_Power_from_Hz(int xIndex,float Hz,float min = -80f){
        float indexf = Hz/this.frequency*nframe;
        int index = (int)indexf;
        float y;
        try{
            y = Utils.CubicInterpolation(indexf-index,sdb[xIndex,index-1],sdb[xIndex,index],sdb[xIndex,index+1],sdb[xIndex,index+2]);
        }
        catch (System.IndexOutOfRangeException){
            y = min;
        }
        return y;
    }
}