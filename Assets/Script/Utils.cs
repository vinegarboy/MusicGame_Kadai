using System;
using System.Collections.Generic;
using System.Linq;

public struct Complex{
        public float X;
        public float Y;
    }

public static class Utils{

    private static double HammingWindow(int n,int framesize){
        return 0.54-0.46*Math.Cos((2*Math.PI*n)/(framesize-1));
    }
    
    private static void FFT(bool forward,int m,Complex[] data){
        int n ,i,i1,j,k,i2,l,l1,l2;
        float c1,c2,tx,ty,t1,t2,u1,u2,z;

        n = 1;
        for(i = 0;i<m;i++){
            n *= 2;
        }
        i2 = n >>1;
        j = 0;
        for(i = 0;i<n-1;i++){
            if(i<j){
                tx = data[i].X;
                ty = data[i].Y;
                data[i].X = data[j].X;
                data[i].Y = data[j].Y;
                data[j].X = tx;
                data[j].Y = ty;
            }
            k = i2;

            while(k <= j){
                j -= k;
                k>>=1;
            }
            j += k;
        }
        c1 = -1.0f;
        c2 = 0.0f;
        l2 = 1;
        for(l = 0;l<m;l++){
            l1 = l2;
            l2 <<= 1;
            u1 = 1.0f;
            u2 = 0.0f;
            for(j = 0;j<l1;j++){
                for(i = j;i<n;i+=l2){
                    i1 = i+l1;
                    t1 = u1*data[i1].X - u2*data[i1].X;
                    t2 = u1*data[i1].Y + u2 * data[i1].X;
                    data[i1].X = data[i].X - t1;
                    data[i1].Y = data[i].Y -t2;
                    data[i].X += t1;
                    data[i].Y += t2;
                }
                z = u1*c1-u2 * c2;
                u2 = u1 * c2 + u2 * c1;
                u1 = z;
            }
            c2 = (float)Math.Sqrt((1.0f-c1)/2.0f);
            if(forward){
                c2 = -c2;
            }
            c1 = (float)Math.Sqrt((1.0f+c1)/2.0f);
        }
        if(forward){
            for(i = 0;i<n;i++){
                data[i].X /= n;
                data[i].Y /= n;
            }
        }
    }

    private static Complex[,] STFT(float[] data ,int n,int noverlap){
        Complex[,] outputs = new Complex[(int)(data.Length - n)/noverlap + 1,n];
        float[] input = new float[n];
        for(int i = 0;i<(int)(data.Length-n)/noverlap + 1;i++){
            Array.Copy(data,noverlap*i,input,0,n);

            var fftsample = new Complex[n];
            for(int j = 0;j<n;j++){
                fftsample[j].X = (float)(input[j]*HammingWindow(j,n));
                fftsample[j].Y = 0.0f;
            }
            var m = (int) Math.Log(n,2);

            FFT(true,m,fftsample);

            for(int j = 0;j<n;j++){
                outputs[i,j] = fftsample[j];
            }
        }
        return outputs;
    }

    public static float[,] Spectrogram(float[] data,int n,int noverlap){
        Complex[,] stft = STFT(data,n,noverlap);
        float[,] spec = new float[stft.GetLength(0),stft.GetLength(1)/2];
        for(int x = 0;x<stft.GetLength(0);x++){
            for(int y = 0;y<stft.GetLength(1)/2;y++){
                double power = Math.Pow(stft[x,y].X,2)+Math.Pow(stft[x,y].Y,2)+0.00001;
                spec[x,y] = 20f * (float)Math.Log10(power);
            }
        }
        return spec;
    }

    public static List<int> PeakPickIndex(float[] x,int pre_max,int post_max,int pre_avg,int post_avg,float delta,int wait){
        int pre_i = -wait-1;
        int pre,post,length;
        List<int> peak_indexes = new List<int>();
        for(int i = 0;i<x.Length;i++){
            pre = (i - pre_max < 0) ? 0 : i -pre_max;
            post = (i+post_max < x.Length) ? i+ post_max : x.Length-1;
            length = post-pre+1;
            float[] check_max = new float[length];
            Array.Copy(x,pre,check_max,0,length);
            pre = (i-pre_avg < 0)? 0:i-pre_avg;
            post =(i+post_avg<x.Length)?i+post_avg:x.Length-1;
            length = post-pre+1;
            float[] check_avg = new float[length];
            Array.Copy(x,pre,check_avg,0,length);

            if(check_avg.Average() + delta < x[i] && check_max.Max() == x[i] && pre_i+wait<i){
                pre_i = i;
                peak_indexes.Add(i);
            }
        }
        return peak_indexes;
    }

    public static float CubicInterpolation(float x,float y1,float y2,float y3,float y4){
        return y1 / -6f * x * (x-1) * (x-2) + y2 /2f*(x+1) * (x-1) * (x-2) + y3 /2f * (x+1) * x * (x-2) +y4/6f * (x+1) * x * (x-1);
    }
} 