using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class SF2ADX2LE : MonoBehaviour
{
    #region SF2Format

    enum SFSampleLink
    {
        monoSample = 1,
        rightSample = 2,
        leftSample = 4,
        linkedSample = 8,
        RomMonoSample = 0x8001,
        RomRightSample = 0x8002,
        RomLeftSample = 0x8004,
        RomLinkedSample = 0x8008}

    ;

    class sfSample
    {
        public string achSampleName = "";
        //char[20]
        public uint dwStart = 0;
        //24
        public uint dwEnd = 0;
        //28
        public uint dwStartloop = 0;
        //  32
        public uint dwEndloop = 0;
        //  36
        public uint dwSampleRate = 0;
        //  40
        public byte byOriginalKey = 0;
        //  41
        public int chCorrection = 0;
        //  42
        public ushort wSampleLink = 0;
        //  44
        public SFSampleLink sfSampleType = SFSampleLink.monoSample;
        //  46

        public override string ToString()
        {
            return string.Format("\"{1:X2}-{2:X2}\" lp:\"{3:X2}-{4:X2}\" rate:\"{5}\" key:\"{6:X2}\" ch:\"{7:X2}\" link:\"{8:X2}\" type:\"{9}\" \"{0}\" ",
                achSampleName,
                dwStart,
                dwEnd,
                dwStartloop,
                dwEndloop,
                dwSampleRate,
                byOriginalKey,
                chCorrection,
                wSampleLink,
                sfSampleType.ToString());

        }
    };

    class sfPresetHeader
    {
        public  string achPresetName = "";
        //char[20]
        public int wPreset = 0;
        //22
        public int wBank = 0;
        //24
        public int wPresetBagNdx = 0;
        //36
        public int dwLibrary = 0;
        //40
        public int dwGenre = 0;
        //44
        public int dwMorphology = 0;
        //48

        public override string ToString()
        {
            return string.Format("preset:\"{1:X2}\" bank:\"{2:X2}\" bagNdx:\"{3:X2}\" lib:\"{4:X2}\" genre:\"{5}\" morph:\"{6:X2}\" \"{0}\" ",
                achPresetName,
                wPreset,
                wBank,
                wPresetBagNdx,
                dwLibrary,
                dwGenre,
                dwMorphology);

        }
    };

    class sfPresetBag
    {
        public int wGenNdx = 0;
        public int wModNdx = 0;

        public override string ToString()
        {
            return string.Format("wGenNdx:\"{0:X2}\" wModNdx:\"{1:X2}\" ",
                wGenNdx,
                wModNdx);
        }
    };

    class sfInst
    {
        public string achInstName = "";
        //char[20]
        public int wInstBagNdx = 0;

        public override string ToString()
        {
            return string.Format("wInstBagNdx:\"{1:X2}\" \"{0}\" ",
                achInstName,
                wInstBagNdx);
        }
    };

    class sfInstBag
    {
        public int wInstGenNdx = 0;
        public int wInstModNdx = 0;

        public override string ToString()
        {
            return string.Format("wInstGenNdx:\"{0:X2}\" wInstModNdx:\"{1:X2}\" ",
                wInstGenNdx,
                wInstModNdx);
        }
    };

    class sfMod
    {
        public int sfModSrcOper = 0;
        public int sfModDestOper = 0;
        public int modAmount = 0;
        public int sfModAmtSrcOper = 0;
        public int sfModTransOper = 0;

        public override string ToString()
        {
            return string.Format("sfModSrcOper:\"{0:X2}\" sfModDestOper:\"{1:X2}\" modAmount:\"{2:X2}\" sfModAmtSrcOper:\"{3:X2}\" sfModTransOper:\"{4:X2}\" ",
                sfModSrcOper,
                sfModDestOper,
                modAmount,
                sfModAmtSrcOper,
                sfModTransOper); 
        }
    };

    class sfInstMod
    {
        public int sfModSrcOper = 0;
        public int sfModDestOper = 0;
        public int modAmount = 0;
        public int sfModAmtSrcOper = 0;
        public int sfModTransOper = 0;

        public override string ToString()
        {
            return string.Format("sfModSrcOper:\"{0:X2}\" sfModDestOper:\"{1:X2}\" modAmount:\"{2:X2}\" sfModAmtSrcOper:\"{3:X2}\" sfModTransOper:\"{4:X2}\" ",
                sfModSrcOper,
                sfModDestOper,
                modAmount,
                sfModAmtSrcOper,
                sfModTransOper); 
        }
    };

    static string GenPo2Str(int gen)
    {
        string outStr = gen.ToString("D2");
        switch (gen)
        {
            case 0:
                outStr += " startAddrsOffset+           ";
                break;
            case 1:
                outStr += " endAddrsOffset+             ";
                break;
            case 2:
                outStr += " startloopAddrsOffset+       ";
                break;
            case 3:
                outStr += " endloopAddrsOffset+         ";
                break;
            case 4:
                outStr += " startAddrsCoarseOffset+     ";
                break;
            case 5:
                outStr += " modLfoToPitch               ";
                break;
            case 6:
                outStr += " vibLfoToPitch               ";
                break;
            case 7:
                outStr += " modEnvToPitch               ";
                break; 
            case 8:
                outStr += " initialFilterFc             ";
                break;
            case 9:
                outStr += " initialFilterQ              ";
                break;
            case 10:
                outStr += " modLfoToFilterFc            ";
                break;
            case 11:
                outStr += " modEnvToFilterFc            ";
                break;
            case 12:
                outStr += " endAddrsCoarseOffset+       ";
                break;
            case 13:
                outStr += " modLfoToVolume              ";
                break;
                
            case 15:
                outStr += " chorusEffectsSend           ";
                break;
            case 16:
                outStr += " reverbEffectsSend           ";
                break;
            case 17:
                outStr += " pan                         ";
                break;
            case 21:
                outStr += " delayModLFO                 ";
                break;
            case 22:
                outStr += " freqModLFO                  ";
                break;
            case 23:
                outStr += " delayVibLFO                 ";
                break;
            case 24:
                outStr += " freqVibLFO                  ";
                break;
            case 25:
                outStr += " delayModEnv                 ";
                break;
            case 26:
                outStr += " attackModEnv                ";
                break;
            case 27:
                outStr += " holdModEnv                  ";
                break;
            case 28:
                outStr += " decayModEnv                 ";
                break;
            case 29:
                outStr += " sustainModEnv               ";
                break;
            case 30:
                outStr += " releaseModEnv               ";
                break;
            case 31:
                outStr += " keynumToModEnvHold          ";
                break;
            case 32:
                outStr += " keynumToModEnvDecay         ";
                break;
            case 33:
                outStr += " delayVolEnv                 ";
                break;
            case 34:
                outStr += " attackVolEnv                ";
                break;
            case 35:
                outStr += " holdVolEnv                  ";
                break;
            case 36:
                outStr += " decayVolEnv                 ";
                break;
            case 37:
                outStr += " sustainVolEnv               ";
                break;
            case 38:
                outStr += " releaseVolEnv               ";
                break;
            case 39:
                outStr += " keynumToVolEnvHold          ";
                break;
            case 40:
                outStr += " keynumToVolEnvDecay         ";
                break;
            case 41:
                outStr += " instrument                  ";
                break;
            case 43:
                outStr += " keyRange @                  ";
                break;
            case 44:
                outStr += " velRange @                  ";
                break;
            case 45:
                outStr += " startloopAddrsCoarseOffset+ ";
                break;
            case 46:
                outStr += " keynum+@                    ";
                break;
            case 47:
                outStr += " velocity +@                 ";
                break;
            case 48:
                outStr += " initialAttenuation          ";
                break;
            case 50:
                outStr += " endloopAddrsCoarseOffset +  ";
                break;
            case 51:
                outStr += " coarseTune                  ";
                break;
            case 52:
                outStr += " fineTune                    ";
                break;
            case 53:
                outStr += " sampleID                    ";
                break;
            case 54:
                outStr += " sampleModes +@              ";
                break;
            case 56:
                outStr += " scaleTuning @               ";
                break;
            case 57:
                outStr += " exclusiveClass+             ";
                break;
            case 58:
                outStr += " overridingRootKey +@        ";
                break;
                
        }    
        return outStr;
    }


    class sfGen
    {
        public int sfGenOper = 0;
        public int genAmount = 0;

        public override string ToString()
        {
            return string.Format("sfGenOper:\"{0:X2}\" genAmount:\"{1:X2}\" ",
                GenPo2Str(sfGenOper),
                genAmount);
        }
    };

    class sfInstGen
    {
        public int sfGenOper = 0;
        public int genAmount = 0;

        public override string ToString()
        {
            return string.Format("sfGenOper:\"{0:X2}\" genAmount:\"{1:X2}\" ",
                GenPo2Str(sfGenOper),
                genAmount);
        }
    };

    #endregion


    public class Zone
    {
        public string name = "";
        public int sampleId = -1;
        public int keyLow = 0;
        public int keyHi = 127;
        //  sampleMode(Loop)
        public int sampleMode = 0;
        public int rootKey = 0;
        public int pan = 0;

        public override string ToString()
        {
            return string.Format("\"{6}\" key:{0:X2}-{1:X2} sampleId:\"{2}\" lp:\"{3}\" key:\"{4}\" pan:\"{5}\"", keyLow, keyHi, sampleId, sampleMode, rootKey, pan, name);
        }

        public string sampleFileName = "";
    }

    #region ADX2LE

    public class Adx2CueSheet
    {
        public string cueSheetName = "";
        public List<Zone> zoneList = new List<Zone>();
    }

    #endregion



    public string inputSfPath = "SF/Famicom.sf2";
    string outputpath = "output_wav";
    // Use this for initialization
    void Start()
    {
        DebugWrite.DebugWriteTextReset(Path.GetDirectoryName(Application.dataPath) + "/" + "output_wav/");

        string inputPath = Path.GetDirectoryName(Application.dataPath) + "/" + inputSfPath;

        #region SoundFont読む
        ReadSf2(inputPath);
        #endregion

        List<string> wavefilePathList = new List<string>();
        #region 波形生成

        MakeWave makewave = this.gameObject.AddComponent<MakeWave>();
        foreach (var shdr in shdrList)
        {
            if (shdr.dwEnd == 0)
                continue;

            string filePath = Path.GetDirectoryName(Application.dataPath) + "/" + outputpath + "/" + shdr.achSampleName + ".wav";
            wavefilePathList.Add(filePath);

            if ((int)(shdr.dwStartloop) > 0)
            {

                uint loopPre = (uint)((shdr.dwStartloop - shdr.dwStart));
                uint loopLength = (uint)((shdr.dwEndloop - shdr.dwStartloop));
                uint loopPost = (uint)((shdr.dwEnd - shdr.dwEndloop));

                #region 書き込み
                if (loopLength < 512)
                {

                    byte[] samples = new byte[(loopPre + loopLength * 32 + loopPost) * 2];

                    System.Array.Copy(smplData, (shdr.dwStart) * 2, samples, 0, (loopPre) * 2);
                    for (int i = 0; i < 32; i++)
                    {
                        System.Array.Copy(smplData, (shdr.dwStartloop) * 2, samples, (loopPre) * 2 + (loopLength) * 2 * i, (loopLength) * 2);
                    }
                    System.Array.Copy(smplData, (shdr.dwEndloop) * 2, samples, (loopPre) * 2 + (loopLength) * 2 * 32, (loopPost) * 2);

                    //  ループかどうかはigen Oper 54 sampleModesが1の時
                    //  もしループ区間が短すぎる場合倍にする

                    makewave.WriteWaveFile(filePath, samples, shdr.dwSampleRate,
                        (uint)((shdr.dwStartloop - shdr.dwStart)), 
                        (uint)((shdr.dwStartloop - shdr.dwStart) + loopLength * 32) - 1);
                    
                }
                else if (loopLength < 1024)
                {

                    byte[] samples = new byte[(loopPre + loopLength * 16 + loopPost) * 2];

                    System.Array.Copy(smplData, (shdr.dwStart) * 2, samples, 0, (loopPre) * 2);
                    for (int i = 0; i < 16; i++)
                    {
                        System.Array.Copy(smplData, (shdr.dwStartloop) * 2, samples, (loopPre) * 2 + (loopLength) * 2 * i, (loopLength) * 2);
                    }
                    System.Array.Copy(smplData, (shdr.dwEndloop) * 2, samples, (loopPre) * 2 + (loopLength) * 2 * 16, (loopPost) * 2);

                    //  ループかどうかはigen Oper 54 sampleModesが1の時
                    //  もしループ区間が短すぎる場合倍にする

                    makewave.WriteWaveFile(filePath, samples, shdr.dwSampleRate,
                        (uint)((shdr.dwStartloop - shdr.dwStart)), 
                        (uint)((shdr.dwStartloop - shdr.dwStart) + loopLength * 16) - 1);

                }
                else
                {

                    byte[] samples = new byte[(shdr.dwEnd - shdr.dwStart) * 2];

                    System.Array.Copy(smplData, shdr.dwStart * 2, samples, 0, (shdr.dwEnd - shdr.dwStart) * 2);

                    //  ループかどうかはigen Oper 54 sampleModesが1の時

                    makewave.WriteWaveFile(filePath, samples, shdr.dwSampleRate,
                        (uint)((shdr.dwStartloop - shdr.dwStart)), 
                        (uint)((shdr.dwStartloop - shdr.dwStart)) + (uint)(shdr.dwEndloop - shdr.dwStartloop));
                }

                #endregion
            }
            else
            {
                
                byte[] samples = new byte[(shdr.dwEnd - shdr.dwStart) * 2];

                System.Array.Copy(smplData, shdr.dwStart * 2, samples, 0, (shdr.dwEnd - shdr.dwStart) * 2);

                //  ループかどうかはigen Oper 54 sampleModesが1の時

                makewave.WriteWaveFile(filePath, samples, shdr.dwSampleRate,
                    0, 0);

            }

        }
    
        #endregion
    

        //  集めたデータからワークユニット、キューシート、キューを作る
        //  ワークユニット名=filename
        //  キューシート名=プリセット名
        string workUnitName = Path.GetFileNameWithoutExtension(inputSfPath);

        #region Zone作成

        int phdrReadNo = 0;
        int pbagReadNo = 0;
        int pgenReadNo = 0;
        int instReadNo = 0;
        int ibagReadNo = 0;
        int igenReadNo = 0;

        List<Adx2CueSheet> cueSheetList = new List<Adx2CueSheet>();

        if (phdrReadNo < phdrList.Count)
        {
            for (; phdrReadNo < phdrList.Count; phdrReadNo++)
            {
                var phdr = phdrList[phdrReadNo];
                if (phdr.achPresetName == "EOP")
                    continue;

                Adx2CueSheet newCueSheet = new Adx2CueSheet();
                newCueSheet.cueSheetName = phdr.achPresetName;
                cueSheetList.Add(newCueSheet);  //  phdrの数だけキューシートを作る

                pbagReadNo = phdr.wPresetBagNdx;    //  プリセットが参照しているpbagの番号

                if (pbagReadNo < pbagList.Count)
                    for (; pbagReadNo < phdrList[phdrReadNo + 1].wPresetBagNdx; pbagReadNo++)   //  次のphdrの番号まで繰り返す
                    {
                        
                        pgenReadNo = pbagList[pbagReadNo].wGenNdx;  //  pbagが参照しているpgenの番号

                        if (pgenReadNo < pgenList.Count)
                            for (; pgenReadNo < pbagList[pbagReadNo + 1].wGenNdx; pgenReadNo++)  //  次のpbagの番号まで繰り返す
                            {
                                
                                instReadNo = pgenList[pgenReadNo].genAmount;    //  pgenが参照しているinstの番号
                                

                                DebugWrite.DebugWriteText("--instReadNo--");
                                DebugWrite.DebugWriteText(string.Format("phdrReadNo {0}",phdrReadNo));
                                DebugWrite.DebugWriteText(string.Format("pbagReadNo {0}",pbagReadNo));
                                DebugWrite.DebugWriteText(string.Format("pgenReadNo {0}",pgenReadNo));
                                DebugWrite.DebugWriteText(string.Format("instReadNo {0}",instReadNo));
                                DebugWrite.DebugWriteText(string.Format("ibagReadNo {0}",ibagReadNo));
                                DebugWrite.DebugWriteText(string.Format("igen ReadNo{0}",igenReadNo));

                                //  ここまできてる

                                if (instReadNo < instList.Count)
                                {
                                    if(instList[instReadNo].achInstName == "EOI")continue;
                                   

                                    //for (; instReadNo < pgenList[instReadNo + 1].genAmount; instReadNo++)//  次のpgenの番号まで繰り返す
                                    {
                                        
                                        ibagReadNo = instList[instReadNo].wInstBagNdx;  //  instが参照しているibagの番号

                                        DebugWrite.DebugWriteText("--ibagReadNo--");
                                        DebugWrite.DebugWriteText(string.Format("phdrReadNo {0}",phdrReadNo));
                                        DebugWrite.DebugWriteText(string.Format("pbagReadNo {0}",pbagReadNo));
                                        DebugWrite.DebugWriteText(string.Format("pgenReadNo {0}",pgenReadNo));
                                        DebugWrite.DebugWriteText(string.Format("instReadNo {0}",instReadNo));
                                        DebugWrite.DebugWriteText(string.Format("ibagReadNo {0}",ibagReadNo));
                                        DebugWrite.DebugWriteText(string.Format("igen ReadNo{0}",igenReadNo));

                                        if (ibagReadNo < ibagList.Count)
                                            for (; ibagReadNo < instList[instReadNo + 1].wInstBagNdx; ibagReadNo++)//  次のinstの番号まで繰り返す
                                            {
                            
                                                {
                                                    igenReadNo = ibagList[ibagReadNo].wInstGenNdx; //  ibagが参照しているigenの番号

                                                    if (igenReadNo < igenList.Count)
                                                    {
                                                        DebugWrite.DebugWriteText("--igenReadNo--");
                                                        DebugWrite.DebugWriteText(string.Format("phdrReadNo {0}",phdrReadNo));
                                                        DebugWrite.DebugWriteText(string.Format("pbagReadNo {0}",pbagReadNo));
                                                        DebugWrite.DebugWriteText(string.Format("pgenReadNo {0}",pgenReadNo));
                                                        DebugWrite.DebugWriteText(string.Format("instReadNo {0}",instReadNo));
                                                        DebugWrite.DebugWriteText(string.Format("ibagReadNo {0}",ibagReadNo));
                                                        DebugWrite.DebugWriteText(string.Format("igen ReadNo{0}",igenReadNo));

                                                        Zone zone = new Zone();

                                                        for (; igenReadNo < ibagList[ibagReadNo + 1].wInstGenNdx; igenReadNo++)//  次のiの番号まで繰り返す
                                                        {
                                                            switch (igenList[igenReadNo].sfGenOper)
                                                            {
                                                            //pan
                                                                case 17:
                                                                    zone.pan = (int)((short)(igenList[igenReadNo].genAmount));
                                                                    break;
                                                            //keyRange
                                                                case 43: 
                                                                    zone.keyHi = ((int)(igenList[igenReadNo].genAmount >> 8) & 0x00FF);
                                                                    zone.keyLow = (igenList[igenReadNo].genAmount & 0x00FF);
                                                                    break;
                                                            //  sampleID
                                                                case 53:
                                                                    zone.sampleId = igenList[igenReadNo].genAmount;
                                                                    break;
                                                            //  sampleMode(Loop)
                                                                case 54:
                                                                    zone.sampleMode = igenList[igenReadNo].genAmount;
                                                                    break;
                                                            //overridingRootKey
                                                                case 58:
                                                                    zone.rootKey = igenList[igenReadNo].genAmount;
                                                                    break;
                                                                default:
                                                                    DebugWrite.DebugWriteText(string.Format("Unkown igen oper {3} val:{1:X4} ({2})", igenList[igenReadNo].sfGenOper, igenList[igenReadNo].genAmount,
                                                                        (int)((short)(igenList[igenReadNo].genAmount)),GenPo2Str(igenList[igenReadNo].sfGenOper)));
                                                                    break;
                                                            }
                                                        }

                                                        //if (zone.sampleId != -1)
                                                        {
                                                            int instrumentNo = pgenList[pgenReadNo].genAmount;
                                                            string instrumentName = instList[instrumentNo].achInstName;
                                                            //DebugWrite.DebugWriteText(string.Format("\"instNo:\"{0}\" : \"{1}\"",instrumentNo,instrumentName));


                                                            zone.name = newCueSheet.cueSheetName;
                                                            if(zone.sampleId >= 0){
                                                                zone.sampleFileName = shdrList[zone.sampleId].achSampleName;

                                                                newCueSheet.zoneList.Add(zone);
                                                            }

                                                            DebugWrite.DebugWriteText(
                                                                string.Format("{2} \"instNo:\"{0}\" : \"{1}\" \"{3}\""
                                                                    , instrumentNo, instrumentName, zone.ToString(),zone.sampleFileName));
                                                            
                                                        }
                                                    }
                                                }
                                            }
                                    }
                                }
                            }
                    }
                //  
                    
            }
        }

        #endregion

        #region ワークユニット作成

        MakeAtomCraftData makeAtomCraftData = this.gameObject.AddComponent<MakeAtomCraftData>();


        string matelialsPath = Path.GetDirectoryName(Application.dataPath) + outputpath;

        makeAtomCraftData.Make(Path.GetDirectoryName(inputPath), workUnitName, cueSheetList, wavefilePathList, matelialsPath);

        #endregion
    }

    void Update()
    {
	
    }
        

    void ReadSf2(string path)
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
            // 4byte // RIFF
            readSize = fs.Read(buf, 0, 4);
            DebugWrite.DebugWriteText(string.Format("ReadSf2:{0:X2}{1:X2}{2:X2}{3:X2}", buf[0], buf[1], buf[2], buf[3]));
            string chunkName = "" + (char)(buf[0]) + (char)(buf[1]) + (char)(buf[2]) + (char)(buf[3]);
            remain -= readSize;

            //  size
            readSize = fs.Read(buf, 0, 4);
            DebugWrite.DebugWriteText(string.Format("ReadSf2:{0:X2}{1:X2}{2:X2}{3:X2}", buf[0], buf[1], buf[2], buf[3]));
            int riffSize = (int)(buf[3] << 24) + (int)(buf[2] << 16) + (int)(buf[1] << 8) + (int)buf[0];
            DebugWrite.DebugWriteText(string.Format("ReadSf2:riffSize = {0:X8}", riffSize));
            remain -= readSize;

            if (chunkName == "RIFF")
            {
                ReadRIFF(fs, ref riffSize);
            }
        }

        fs.Dispose();

        Debug.Log("Read End");
    }

    void ReadRIFF(FileStream fs, ref int remain)
    {

        int readSize;
        
        byte[] buf = new byte[4];
        readSize = fs.Read(buf, 0, 4);
        string chunkName = "" + (char)(buf[0]) + (char)(buf[1]) + (char)(buf[2]) + (char)(buf[3]);
        DebugWrite.DebugWriteText(string.Format("{0:X8} ReadRIFF:chunk '{1}'", fs.Position, chunkName));
        remain -= readSize;

        if (chunkName == "sfbk" ||
            chunkName == "sbnk" ||
            chunkName == "sdta" ||
            chunkName == "pdta")
        {
            while (remain > 0)
            {
                ReadChunk(fs, ref remain);
            }
        }

    }

    List<sfSample> shdrList = new List<sfSample>();
    List<sfPresetHeader> phdrList = new List<sfPresetHeader>();
    List<sfPresetBag> pbagList = new List<sfPresetBag>();
    List<sfMod> pmodList = new List<sfMod>();
    List<sfGen> pgenList = new List<sfGen>();
    List<sfInst> instList = new List<sfInst>();
    List<sfInstBag> ibagList = new List<sfInstBag>();
    List<sfInstMod> imodList = new List<sfInstMod>();
    List<sfInstGen> igenList = new List<sfInstGen>();
    byte[] smplData;


    void ReadChunk(FileStream fs, ref int remain)
    {

        int readSize;

        byte[] buf = new byte[4];
        readSize = fs.Read(buf, 0, 4);
        string chunkName = "" + (char)(buf[0]) + (char)(buf[1]) + (char)(buf[2]) + (char)(buf[3]);
        DebugWrite.DebugWriteText(string.Format("{0:X8} ReadChunk: '{1}' remain:{2:X8}", fs.Position - 4, chunkName, remain));
        remain -= readSize;

        if (chunkName == "INFO" ||
            chunkName == "sbnk" ||
            chunkName == "sdta" ||
            chunkName == "pdta")
        {
                
            ReadChunk(fs, ref remain);
        }
        else
        {

            readSize = fs.Read(buf, 0, 4);
            remain -= readSize;

            int chunkSize = (int)(buf[3] << 24) + (int)(buf[2] << 16) + (int)(buf[1] << 8) + (int)buf[0];
            DebugWrite.DebugWriteText(string.Format("{0:X8} ReadChunk: '{1}' {2:X8} remain:{3:X8}", fs.Position - 4, chunkName, chunkSize, remain));

            if (chunkName == "LIST")
            {
                ReadChunk(fs, ref chunkSize);

            }
            else
            {
                if (chunkName == "shdr")
                {
                    int remainShdr = chunkSize;
                    while (remainShdr > 0)
                    {
                        
                        sfSample shdr = new sfSample();
                        buf = new byte[20];
                        readSize = fs.Read(buf, 0, 20);
                        remainShdr -= readSize;

                        shdr.achSampleName = System.Text.Encoding.ASCII.GetString(buf);
                        {
                            int endStr = shdr.achSampleName.IndexOf('\0');
                            if (endStr > 0)
                            {
                                shdr.achSampleName = shdr.achSampleName.Remove(endStr);
                            }
                        }

                        buf = new byte[26];
                        readSize = fs.Read(buf, 0, 26);
                        remainShdr -= readSize;

                        shdr.dwStart = (uint)((int)(buf[3] << 24) + (int)(buf[2] << 16) + (int)(buf[1] << 8) + (int)buf[0]);
                        //24
                        shdr.dwEnd = (uint)((int)(buf[7] << 24) + (int)(buf[6] << 16) + (int)(buf[5] << 8) + (int)buf[4]);
                        //28
                        shdr.dwStartloop = (uint)((int)(buf[11] << 24) + (int)(buf[10] << 16) + (int)(buf[9] << 8) + (int)buf[8]);
                        //  32
                        shdr.dwEndloop = (uint)((int)(buf[15] << 24) + (int)(buf[14] << 16) + (int)(buf[13] << 8) + (int)buf[12]);
                        //  36
                        shdr.dwSampleRate = (uint)((int)(buf[19] << 24) + (int)(buf[18] << 16) + (int)(buf[17] << 8) + (int)buf[16]);
                        //  40
                        shdr.byOriginalKey = (byte)buf[20];
                        //  41
                        shdr.chCorrection = (int)buf[21];
                        //  42
                        shdr.wSampleLink = (ushort)((int)(buf[23] << 8) + (int)buf[22]);
                        //  44
                        shdr.sfSampleType = (SFSampleLink)((int)(buf[25] << 8) + (int)buf[24]);
                        //  46

                        DebugWrite.DebugWriteText(string.Format("{0:X8}-{1:X8} \'{2}\' : {3}", fs.Position - 46, fs.Position, chunkName, shdr.ToString()));

                        shdrList.Add(shdr);

                        if (shdr.achSampleName.StartsWith("EOS"))
                        {
                            //break;
                            remain = 0;
                            return;
                        }

                    }
                    remain -= readSize;
                }
                else if (chunkName == "phdr")
                {
                    int remainShdr = chunkSize;
                    while (remainShdr > 0)
                    {

                        sfPresetHeader phdr = new sfPresetHeader();
                        buf = new byte[20];
                        readSize = fs.Read(buf, 0, 20);
                        remainShdr -= readSize;

                        phdr.achPresetName = System.Text.Encoding.ASCII.GetString(buf);
                        {
                            int endStr = phdr.achPresetName.IndexOf('\0');
                            if (endStr > 0)
                            {
                                phdr.achPresetName = phdr.achPresetName.Remove(endStr);
                            }
                        }

                        buf = new byte[18];
                        readSize = fs.Read(buf, 0, 18);
                        remainShdr -= readSize;

                        phdr.wPreset = (int)((int)(buf[1] << 8) + (int)buf[0]);
                        //24
                        phdr.wBank = (int)((int)(buf[3] << 8) + (int)buf[2]);
                        //28
                        phdr.wPresetBagNdx = (int)((int)(buf[5] << 8) + (int)buf[4]);
                        //  32
                        phdr.dwLibrary = (int)((int)(buf[9] << 24) + (int)(buf[8] << 16) + (int)(buf[7] << 8) + (int)buf[6]);
                        //  36
                        phdr.dwGenre = (int)((int)(buf[13] << 24) + (int)(buf[12] << 16) + (int)(buf[11] << 8) + (int)buf[10]);
                        //  40
                        phdr.dwMorphology = (int)((int)(buf[17] << 24) + (int)(buf[16] << 16) + (int)(buf[15] << 8) + (int)buf[14]);
                        //  41


                        DebugWrite.DebugWriteText(string.Format("{0:X8}-{1:X8} \'{2}\' : {3}", fs.Position - 46, fs.Position, chunkName, phdr.ToString()));

                        phdrList.Add(phdr);

                        if (phdr.achPresetName.StartsWith("EOP"))
                        {
                            //break;
                            remain = 0;
                            return;
                        }

                    }
                    remain -= readSize;
                }
                else if (chunkName == "pbag")
                {
                    int remainPbag = chunkSize;
                    while (remainPbag > 0)
                    {

                        sfPresetBag pbag = new sfPresetBag();

                        buf = new byte[4];
                        readSize = fs.Read(buf, 0, 4);
                        remainPbag -= readSize;

                        pbag.wGenNdx = (int)((int)(buf[1] << 8) + (int)buf[0]);
                        pbag.wModNdx = (int)((int)(buf[3] << 8) + (int)buf[2]);


                        DebugWrite.DebugWriteText(string.Format("{0:X8}-{1:X8} \'{2}\' : {3}", fs.Position - 4, fs.Position, chunkName, pbag.ToString()));

                        pbagList.Add(pbag);

                        if (remainPbag <= 0)
                        {
                            break;
                            //remain = 0;
                            //return;
                        }

                    }
                    remain -= readSize;
                }
                else if (chunkName == "pmod")
                {
                    int remainPmod = chunkSize;
                    while (remainPmod > 0)
                    {
                        sfMod pmod = new sfMod();

                        buf = new byte[10];
                        readSize = fs.Read(buf, 0, 10);
                        remainPmod -= readSize;

                        pmod.sfModSrcOper = (int)((int)(buf[1] << 8) + (int)buf[0]);
                        pmod.sfModDestOper = (int)((int)(buf[3] << 8) + (int)buf[2]);
                        pmod.modAmount = (int)((int)(buf[5] << 8) + (int)buf[4]);
                        pmod.sfModAmtSrcOper = (int)((int)(buf[7] << 8) + (int)buf[6]);
                        pmod.sfModTransOper = (int)((int)(buf[9] << 8) + (int)buf[8]);

                        DebugWrite.DebugWriteText(string.Format("{0:X8}-{1:X8} \'{2}\' : {3}", fs.Position - 10, fs.Position, chunkName, pmod.ToString()));

                        pmodList.Add(pmod);

                        if (remainPmod <= 0)
                        {
                            break;
                            //remain = 0;
                            //return;
                        }

                    }
                    remain -= readSize;
                }
                else if (chunkName == "pgen")
                {
                    int remainPgen = chunkSize;
                    while (remainPgen > 0)
                    {
                        sfGen pgen = new sfGen();

                        buf = new byte[4];
                        readSize = fs.Read(buf, 0, 4);
                        remainPgen -= readSize;

                        pgen.sfGenOper = (int)((int)(buf[1] << 8) + (int)buf[0]);
                        pgen.genAmount = (int)((int)(buf[3] << 8) + (int)buf[2]);

                        DebugWrite.DebugWriteText(string.Format("{0:X8}-{1:X8} \'{2}\' : {3}", fs.Position - 4, fs.Position, chunkName, pgen.ToString()));

                        pgenList.Add(pgen);

                        if (remainPgen <= 0)
                        {
                            break;
                            //remain = 0;
                            //return;
                        }

                    }
                    remain -= readSize;
                }
                else if (chunkName == "inst")
                {
                    int remainInst = chunkSize;
                    while (remainInst > 0)
                    {

                        sfInst inst = new sfInst();
                        buf = new byte[20];
                        readSize = fs.Read(buf, 0, 20);
                        remainInst -= readSize;

                        inst.achInstName = System.Text.Encoding.ASCII.GetString(buf);
                        {
                            int endStr = inst.achInstName.IndexOf('\0');
                            if (endStr > 0)
                            {
                                inst.achInstName = inst.achInstName.Remove(endStr);
                            }
                        }

                        buf = new byte[2];
                        readSize = fs.Read(buf, 0, 2);
                        remainInst -= readSize;

                        inst.wInstBagNdx = (int)((int)(buf[1] << 8) + (int)buf[0]);

                        DebugWrite.DebugWriteText(string.Format("{0:X8}-{1:X8} \'{2}\' : {3}", fs.Position - 46, fs.Position, chunkName, inst.ToString()));

                        instList.Add(inst);

                        if (inst.achInstName.StartsWith("EOI"))
                        {
                            break;
                            //remain = 0;
                            //return;
                        }

                    }
                    remain -= readSize;
                }
                else if (chunkName == "ibag")
                {
                    int remainIbag = chunkSize;
                    while (remainIbag > 0)
                    {
                        sfInstBag ibag = new sfInstBag();

                        buf = new byte[4];
                        readSize = fs.Read(buf, 0, 4);
                        remainIbag -= readSize;

                        ibag.wInstGenNdx = (int)((int)(buf[1] << 8) + (int)buf[0]);
                        ibag.wInstModNdx = (int)((int)(buf[3] << 8) + (int)buf[2]);

                        DebugWrite.DebugWriteText(string.Format("{0:X8}-{1:X8} \'{2}\' : {3}", fs.Position - 4, fs.Position, chunkName, ibag.ToString()));

                        ibagList.Add(ibag);

                        if (remainIbag <= 0)
                        {
                            break;
                            //remain = 0;
                            //return;
                        }

                    }
                    remain -= readSize;
                }
                else if (chunkName == "imod")
                {
                    int remainImod = chunkSize;
                    while (remainImod > 0)
                    {
                        sfInstMod imod = new sfInstMod();

                        buf = new byte[10];
                        readSize = fs.Read(buf, 0, 10);
                        remainImod -= readSize;

                        imod.sfModSrcOper = (int)((int)(buf[1] << 8) + (int)buf[0]);
                        imod.sfModDestOper = (int)((int)(buf[3] << 8) + (int)buf[2]);
                        imod.modAmount = (int)((int)(buf[5] << 8) + (int)buf[4]);
                        imod.sfModAmtSrcOper = (int)((int)(buf[7] << 8) + (int)buf[6]);
                        imod.sfModTransOper = (int)((int)(buf[9] << 8) + (int)buf[8]);

                        DebugWrite.DebugWriteText(string.Format("{0:X8}-{1:X8} \'{2}\' : {3}", fs.Position - 10, fs.Position, chunkName, imod.ToString()));

                        imodList.Add(imod);

                        if (remainImod <= 0)
                        {
                            break;
                            //remain = 0;
                            //return;
                        }

                    }
                    remain -= readSize;
                }
                else if (chunkName == "igen")
                {
                    int remainIgen = chunkSize;
                    while (remainIgen > 0)
                    {
                        sfInstGen igen = new sfInstGen();

                        buf = new byte[4];
                        readSize = fs.Read(buf, 0, 4);
                        remainIgen -= readSize;

                        igen.sfGenOper = (int)((int)(buf[1] << 8) + (int)buf[0]);
                        igen.genAmount = (int)((int)(buf[3] << 8) + (int)buf[2]);

                        DebugWrite.DebugWriteText(string.Format("{0:X8}-{1:X8} \'{2}\' : {3}", fs.Position - 4, fs.Position, chunkName, igen.ToString()));

                        igenList.Add(igen);

                        if (remainIgen <= 0)
                        {
                            break;
                            //remain = 0;
                            //return;
                        }

                    }
                    remain -= readSize;
                }
                else if (chunkName == "smpl")
                {
                    smplData = new byte[chunkSize];
                    readSize = fs.Read(smplData, 0, chunkSize);
                    remain -= readSize;

                    string strData = "";
                    DebugWrite.DebugWriteText(string.Format("{0:X8}-{1:X8} \'{2}\' : \"{3}\" remain:{4}", fs.Position - chunkSize, fs.Position - chunkSize + chunkSize, chunkName, strData, remain));

                    if (remain <= 0)
                    {
                        return;
                    }
                }
                else
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
                
                    if (remain <= 0)
                    {
                        return;
                    }
                }

            }
        }
    }

}
