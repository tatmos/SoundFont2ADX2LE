using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MIDI2ADX2LE : MonoBehaviour
{

    public string midiFilePath = "MIDI/siokarabushi.mid";
    public int tempo = 1500000;
    public int resolution = 480;

    List<MidiInfo> midiInfoList = new List<MidiInfo>();

    // Use this for initialization
    void Start()
    {	
    }

    public void Midi2Adx2LeMain()
    {


        string inputPath = Path.GetDirectoryName(Application.dataPath) + "/" + midiFilePath;
        ReadMIDIFile(inputPath);

        //  MIDIワークユニット
        {
            MakeAtomCraftDataFromMidi makeAtomCraftDataFromMidi = this.gameObject.AddComponent<MakeAtomCraftDataFromMidi>();

            makeAtomCraftDataFromMidi.Make(Path.GetDirectoryName(midiFilePath), Path.GetFileName(midiFilePath), midiInfoList);
        }
    }

    void ReadMIDIFile(string path)
    {

        if (System.IO.File.Exists(path) == false)
        {
            Debug.LogError("Not Read File : " + path);
            return;
        }

        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        int readSize;
        int fileSize = (int)fs.Length; // ファイルのサイズ
        int remain = fileSize; // 読み込むべき残りのバイト数

        byte[] buf = new byte[4];
        {
            // 4byte // MThd
            readSize = fs.Read(buf, 0, 4);
            DebugWrite.DebugWriteText(string.Format("ReadMIDIFile:{0:X2}{1:X2}{2:X2}{3:X2}", buf [0], buf [1], buf [2], buf [3]));
            string chunkName = "" + (char)(buf [0]) + (char)(buf [1]) + (char)(buf [2]) + (char)(buf [3]);
            remain -= readSize;

            //  size
            readSize = fs.Read(buf, 0, 4);
            DebugWrite.DebugWriteText(string.Format("ReadMIDIFile:{0:X2}{1:X2}{2:X2}{3:X2}", buf [0], buf [1], buf [2], buf [3]));
            int riffSize = (int)(buf [3] << 24) + (int)(buf [2] << 16) + (int)(buf [1] << 8) + (int)buf [0];
            DebugWrite.DebugWriteText(string.Format("ReadMIDIFile:MthdSize = {0:X8}", riffSize));
            remain -= readSize;

            if (chunkName == "MThd")
            {
                ReadMthd(fs, ref riffSize);
            }
        }

        fs.Dispose();

        Debug.Log("Read End");
    }

    void ReadMthd(FileStream fs, ref int remain)
    {
        int readSize;

        byte[] buf = new byte[2];
        readSize = fs.Read(buf, 0, 2);
        int format = (ushort)((int)(buf [1] << 8) + (int)buf [0]);
        if (format != 0)
        {
            Debug.LogError("Not Read File : Format 0 only!");
            return;
        }
        remain -= readSize;

        buf = new byte[2];
        readSize = fs.Read(buf, 0, 2);
        int trackNum = (ushort)((int)(buf [0] << 8) + (int)buf [1]);
        if (trackNum != 1)
        {
            Debug.LogError("Not Read File : TrackNum 1 only!");
            return;
        }
        remain -= readSize;

        //  分解能
        buf = new byte[2];
        readSize = fs.Read(buf, 0, 2);
        resolution = (ushort)((int)(buf [0] << 8) + (int)buf [1]);
        DebugWrite.DebugWriteText(string.Format("resolution : {0}", resolution.ToString()));
        remain -= readSize;

        while (remain > 0)
        {
            ReadChunk(fs, ref remain);
        }
    }

    void ReadChunk(FileStream fs, ref int remain)
    {
        int readSize;

        byte[] buf = new byte[4];
        readSize = fs.Read(buf, 0, 4);
        string chunkName = "" + (char)(buf [0]) + (char)(buf [1]) + (char)(buf [2]) + (char)(buf [3]);
        DebugWrite.DebugWriteText(string.Format("{0:X8} ReadChunk: '{1}' remain:{2:X8}", fs.Position - 4, chunkName, remain));
        remain -= readSize;

        readSize = fs.Read(buf, 0, 4);
        remain -= readSize;

        int chunkSize = (int)(buf [3] << 24) + (int)(buf [2] << 16) + (int)(buf [1] << 8) + (int)buf [0];
        DebugWrite.DebugWriteText(string.Format("{0:X8} ReadChunk: '{1}' {2:X8} remain:{3:X8}", fs.Position - 4, chunkName, chunkSize, remain));

        if (chunkSize == 0)
        {
            Debug.LogError("ChunkSize == 0!!!");
            remain = 0;
            return;
        }

        if (chunkName == "MTrk")
        {
            //  MTrk解析
            //  deltatime?
            buf = new byte[chunkSize];
            readSize = fs.Read(buf, 0, chunkSize);
            remain -= readSize;

            int readPoint = 0;
            //int tempo = 500000;// 120 =  500000microSec = 500msec
            int time = 0;   //  開始からの累積時間

            while (readPoint < readSize)
            {

                int deltaTime = buf [readPoint++];
                if (deltaTime >= 0x80)
                {
                    int deltaTime2 = buf [readPoint++];
                    if (deltaTime2 >= 0x80)
                    {
                        int deltaTime3 = buf [readPoint++];
                        if (deltaTime3 >= 0x80)
                        {
                            int deltaTime4 = buf [readPoint++];
                            deltaTime = (deltaTime & 0x7F) * 0x80 * 0x80 * 0x80 + (deltaTime2 & 0x7F) * 0x80 * 0x80 + (deltaTime3 & 0x7F) * 0x80 + deltaTime4;
                        } else
                        {
                            deltaTime = (deltaTime & 0x7F)* 0x80 * 0x80 + (deltaTime2 & 0x7F) * 0x80 + deltaTime3;
                        }
                    } else
                    {
                        deltaTime = (deltaTime & 0x7F) * 0x80 + deltaTime2;
                    }
                }


                time += deltaTime;

                int data = buf [readPoint++];

                if (data == 0xFF)
                {
                    //  メタイベント
                    int eventType = buf [readPoint++];

                    if (eventType == 0x2F)
                    {
                        //  EOF
                        return;
                    } else if (eventType == 0x00)
                    {
                        //SequenceNumber
                        int eventLength = buf [readPoint++];
                        int eventData = buf [readPoint++];
                        eventData = buf [readPoint++];
                    } else if (eventType == 0x01 || //text
                               eventType == 0x02 || //Copyright
                               eventType == 0x03 || //Sequence Track Name
                               eventType == 0x04 || //Instrument Name
                               eventType == 0x05) // Lylic
                    {
                        int eventLength = buf [readPoint++];
                        string text = "";
                        for (int i = 0; i < eventLength; i++)
                        {
                            byte[] eventData = new byte[1];
                            eventData [0] = buf [readPoint++];
                            text += System.Text.Encoding.ASCII.GetString(eventData);
                        }

                        DebugWrite.DebugWriteText(string.Format("time:{0:00000000} Text : {1}", ((float)time/(float)resolution*(float)tempo/1000f), text));
                    } else if (eventType == 0x51)
                    {
                        //  tempo 4byte マイクロ秒単位の４分音符の長さ
                        int eventLength = buf [readPoint++];
                        int eventData1 = buf [readPoint++];
                        int eventData2 = buf [readPoint++];
                        int eventData3 = buf [readPoint++];
                        tempo = (eventData1 << 16) + (eventData2 << 8) + eventData3; // 07 A1 20 = 500000

                        DebugWrite.DebugWriteText(string.Format("time:{0:00000000} Tempo (microSec) : {1}", ((float)time/(float)resolution*(float)tempo/1000f), tempo));
                    } else if (eventType == 0x58)
                    {
                        int eventLength = buf [readPoint++];
                        //  04  
                        //拍子 nn=分子
//                        dd=分子（2の負のべき乗で表す）
//                            cc=メトロノーム間隔（四分音符間隔なら18H）
//                            bb=四分音符あたりの三十二分音符の数
                        int eventData1 = buf [readPoint++];
                        int eventData2 = buf [readPoint++];
                        int eventData3 = buf [readPoint++];
                        int eventData4 = buf [readPoint++];

                        DebugWrite.DebugWriteText(string.Format("time:{0:00000000} {1}/{2} {3} {4}", ((float)time/(float)resolution*(float)tempo/1000f), eventData1, System.Math.Pow(2, eventData2), eventData3, eventData4));
                    } else if (eventType == 0x59)
                    {
                        int eventLength = buf [readPoint++];
                        //  Key Signature 2byte
                        int eventData1 = buf [readPoint++];
                        int eventData2 = buf [readPoint++];
                        DebugWrite.DebugWriteText(string.Format("time:{0:00000000} Key sf : {0} mi : {1} ", ((float)time/(float)resolution*(float)tempo/1000f), eventData1, eventData2));

                    } else
                    {

                        int eventLength = buf [readPoint++];

                        while (eventLength > 0)
                        {
                            int eventData = buf [readPoint++];
                            DebugWrite.DebugWriteText(string.Format("time:{0:00000000} EventData {1} {2}", ((float)time/(float)resolution*(float)tempo/1000f), eventType, eventData));
                            eventLength--;
                        }
                    }
                } else if ((data & 0xF0) == 0x90)
                {
                    //  note on
                    int ch = (data & 0x0F);
                    int note = buf [readPoint++];
                    int vel = buf [readPoint++];
                    DebugWrite.DebugWriteText(string.Format("time:{0:00000000} Note On ch : {1} note : {2} vel : {3} ", ((float)time/(float)resolution*(float)tempo/1000f), ch, note, vel));

                    if(vel > 0){
                        midiInfoList.Add(new MidiInfo((int)((float)time/(float)resolution*(float)tempo/1000f), note)); 
                    }

                } else if ((data & 0xF0) == 0x80)
                {
                    //  note off
                    int ch = (data & 0x0F);
                    int note = buf [readPoint++];
                    int vel = buf [readPoint++];
                    DebugWrite.DebugWriteText(string.Format("time:{0:00000000} Note Off ch : {1} note : {2} vel : {3} ", ((float)time/(float)resolution*(float)tempo/1000f), ch, note, vel));
                } else if ((data & 0xF0) == 0xB0)
                {
                    //  ctrl change
                    int ch = (data & 0x0F);
                    int cc = buf [readPoint++];
                    int vv = buf [readPoint++];
                    DebugWrite.DebugWriteText(string.Format("time:{0:00000000} Ctrl Change ch : {1} cc : {2} vel : {3} ", ((float)time/(float)resolution*(float)tempo/1000f), ch, cc, vv));
                } else {

                    Debug.LogError("Unkown");
                }

            }

            if (remain <= 0 || chunkSize == 0)
            {
                return;
            }
        } else
        {
            buf = new byte[chunkSize];
            readSize = fs.Read(buf, 0, chunkSize);
            remain -= readSize;

            string strData = "";
            if (chunkSize < 128)
            {
                strData = System.Text.Encoding.ASCII.GetString(buf);
            }
            DebugWrite.DebugWriteText(string.Format("{0:X8}-{1:X8} \'{2}\' : \"{3}\" remain:{4}", fs.Position - chunkSize, fs.Position - chunkSize + chunkSize, chunkName, strData, remain));

            if (remain <= 0 || chunkSize == 0)
            {
                return;
            }
        }


    }
}
