using UnityEngine;
using System.Collections;
using System.IO;

public class MakeWave : MonoBehaviour
{
    struct WavHeader
    {
        public byte[] riffID;
        // "riff"
        public uint size;
        // ファイルサイズ-8
        public byte[] wavID;
        // "WAVE"
        public byte[] fmtID;
        // "fmt "
        public uint fmtSize;
        // fmtチャンクのバイト数
        public ushort format;
        // フォーマット
        public ushort channels;
        // チャンネル数
        public uint sampleRate;
        // サンプリングレート
        public uint bytePerSec;
        // データ速度
        public ushort blockSize;
        // ブロックサイズ
        public ushort bit;
        // 量子化ビット数
        public byte[] dataID;
        // "data"
        public uint dataSize;
        // 波形データのバイト数
    }

    struct SmplHeader
    {
        public byte[] smplID;
        public uint size;
        public uint manunfacturer;
        public uint product;
        public uint samplePeriod;
        public uint midiUnityNote;
        public uint midiPitchFraction;
        public uint smpteFormat;
        public uint smpteOffset;
        public uint numSampleLoops;
        public uint samplerData;

        public uint cuePointId;
        public uint lpType;
        public uint lpStart;
        public uint lpEnd;
        public uint lpFraction;
        public uint lpPlayCount;
    }


    public void WriteWaveFile(string filePath, byte[] waveSamples, uint rate, uint lpStart, uint lpEnd)
    {
        #region もしフォルダがなければ作成
        if(Directory.Exists(Path.GetDirectoryName(filePath)) == false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }
        #endregion

        WavHeader Header = new WavHeader();
        Header.riffID = new byte[4];
        Header.riffID[0] = (byte)'R';
        Header.riffID[1] = (byte)'I';
        Header.riffID[2] = (byte)'F';
        Header.riffID[3] = (byte)'F';
        Header.wavID = new byte[4];
        Header.wavID[0] = (byte)'W';
        Header.wavID[1] = (byte)'A';
        Header.wavID[2] = (byte)'V';
        Header.wavID[3] = (byte)'E';
        Header.fmtID = new byte[4];
        Header.fmtID[0] = (byte)'f';
        Header.fmtID[1] = (byte)'m';
        Header.fmtID[2] = (byte)'t';
        Header.fmtID[3] = (byte)' ';
        Header.fmtSize = 0x10;
        Header.format = 1;
        Header.channels = 1;
        Header.sampleRate = rate;
        Header.bytePerSec = 2;
        Header.blockSize = 4;
        Header.bit = 16;
        Header.dataID = new byte[4];
        Header.dataID[0] = (byte)'d';
        Header.dataID[1] = (byte)'a';
        Header.dataID[2] = (byte)'t';
        Header.dataID[3] = (byte)'a';


        Header.dataSize = (uint)waveSamples.Length;
        Header.size = (uint)Header.dataSize + 16 + 4;

        if (lpEnd != 0)
        {
            Header.size += 0x44;
        }

        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            try
            {
                bw.Write(Header.riffID);
                bw.Write(Header.size);
                bw.Write(Header.wavID);
                bw.Write(Header.fmtID);
                bw.Write(Header.fmtSize);
                bw.Write(Header.format);
                bw.Write(Header.channels);
                bw.Write(Header.sampleRate);
                bw.Write(Header.bytePerSec);
                bw.Write(Header.blockSize);
                bw.Write(Header.bit);
                bw.Write(Header.dataID);
                bw.Write(Header.dataSize);

                bw.Write(waveSamples);

                if (lpEnd != 0)
                {
                    SmplHeader smplHeader = new SmplHeader(); 
                    smplHeader.smplID = new byte[4];
                    smplHeader.smplID[0] = (byte)'s';
                    smplHeader.smplID[1] = (byte)'m';
                    smplHeader.smplID[2] = (byte)'p';
                    smplHeader.smplID[3] = (byte)'l';

                    smplHeader.size = 0x3c;
                    smplHeader.manunfacturer = 0;
                    smplHeader.product = 0;
                    smplHeader.samplePeriod = 0;
                    smplHeader.midiUnityNote = 0x3C;
                    smplHeader.midiPitchFraction = 0;
                    smplHeader.smpteFormat = 0;
                    smplHeader.smpteOffset = 0;
                    smplHeader.numSampleLoops = 1;
                    smplHeader.samplerData = 0;

                    smplHeader.cuePointId = 0;
                    smplHeader.lpType = 0;
                    smplHeader.lpStart = lpStart;
                    smplHeader.lpEnd = lpEnd;
                    smplHeader.lpFraction = 0;
                    smplHeader.lpPlayCount = 0;

                    bw.Write(smplHeader.smplID);
                    bw.Write(smplHeader.size);
                    bw.Write(smplHeader.manunfacturer);
                    bw.Write(smplHeader.product);
                    bw.Write(smplHeader.samplePeriod);
                    bw.Write(smplHeader.midiUnityNote);
                    bw.Write(smplHeader.midiPitchFraction);
                    bw.Write(smplHeader.smpteFormat);
                    bw.Write(smplHeader.smpteOffset);
                    bw.Write(smplHeader.numSampleLoops);
                    bw.Write(smplHeader.samplerData);
                    bw.Write(smplHeader.cuePointId);
                    bw.Write(smplHeader.lpType);
                    bw.Write(smplHeader.lpStart);
                    bw.Write(smplHeader.lpEnd);
                    bw.Write(smplHeader.lpFraction);
                    bw.Write(smplHeader.lpPlayCount);

                }

            }
            finally
            {
                if (bw != null)
                {
                    bw.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }
    }
}
