using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Security;

/// <summary>
/// MIDIからの情報
/// </summary>
public class MidiInfo
{
    public MidiInfo(int _time,int _noteNo)
    {
        time= _time;
        noteNo = _noteNo;
    }
    public int time = 0;        //  sequence開始からの時間msec
    public int noteNo = 60;     //  NoteNo
};

public class MakeAtomCraftDataFromMidi : MonoBehaviour
{
    //  キューに設定するカテゴリ名
    public string defaultGroupCategory = "";
    public int cuePriority = 64;
    //
    public float pos3dDistanceMin = 10.0f;
    public float pos3dDistanceMax = 50.0f;
    public float pos3dDopplerCoefficient = 0.0f;

    public void Make(string outputPath, string workunitName, List<MidiInfo> noteInfoList)
    {
        #region WorkUnitNameの名前修正
        workunitName = ChangeName(workunitName);
        #endregion

        MakeWorkUnit(outputPath, workunitName,noteInfoList);
       
        Debug.Log("<color=orange>Make Atom Craft Data Finish!</color> " + outputPath);
    }

    void MakeWorkUnit(string outputPath, string workunitName, List<MidiInfo> noteInfoList)
    {
        //  情報ファイル作成
        MakeAtmcunit(outputPath, workunitName,noteInfoList);
        MakeMaterialinfo(outputPath, workunitName);
    }

    void MakeAtmcunit(string outputPath, string workunitName,List<MidiInfo> noteInfoList)
    {
        string filePath = outputPath + "/" + workunitName + "/" + workunitName + ".atmcunit";

        Debug.Log("Make Workunit FilePath: " + filePath);

        if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        StreamWriter sw;
        FileInfo fi;
        fi = new FileInfo(filePath);
        sw = fi.AppendText();

        sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sw.WriteLine("<!-- Orca XML File Format -->");
        sw.WriteLine("<OrcaTrees ObjectTypeExpression=\"Full\" BinaryEncodingType=\"Base64\" FileVersion=\"3\" FileRevision=\"0\">");
        sw.WriteLine("  <OrcaTree OrcaName=\"(no name)\">");
        sw.WriteLine("    <Orca OrcaName=\"" + workunitName + "\" VersionInfo=\"Ver.2.19.02\" FormatVersion=\"Ver.1.00.04\" WorkUnitPath=\"WorkUnits/" + workunitName + "/" + workunitName + ".atmcunit\" UsedMaterialFlag=\"True\" Expand=\"True\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoWorkUnit\">");         
        sw.WriteLine("      <Orca OrcaName=\"References\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoReferenceFolder\">");
        sw.WriteLine("        <Orca OrcaName=\"AISAC\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoReferenceAisacFolder\" />");
        sw.WriteLine("      </Orca>");
        sw.WriteLine("      <Orca OrcaName=\"CueSheetFolder\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoCueSheetFolder\">");
        sw.WriteLine("        <Orca OrcaName=\"" + workunitName + "\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoCueSheetSubFolder\">");

        SF2ADX2LE.Adx2CueSheet cueSheet = new SF2ADX2LE.Adx2CueSheet();
        cueSheet.cueSheetName = "Midi";

        //foreach (var cuesheet in cueSheetList)
        {
            this.CreateCueSheetXML(sw, workunitName, cueSheet,noteInfoList);
        }

        sw.WriteLine("        </Orca>");
        sw.WriteLine("      </Orca>");
        sw.WriteLine("    </Orca>");
        sw.WriteLine("  </OrcaTree>");
        sw.WriteLine("</OrcaTrees>");
        sw.WriteLine("<!-- Copyright (c) CRI Middleware Co.,LTD. -->");
        sw.WriteLine("<!-- end of document -->");

        sw.Flush();
        sw.Close();

    }

    class Adx2Track
    {
        public string name;
        public string materialName;
        public bool loopFlag;
        public int pitch;
        public int pan;
    };

    void CreateCueSheetXML(StreamWriter sw, string workunitName, SF2ADX2LE.Adx2CueSheet cueSheet,List<MidiInfo> noteInfoList)
    {
        Guid guid = Guid.NewGuid();

        string cueSheetName = ChangeName(cueSheet.cueSheetName);

        sw.WriteLine("          <Orca OrcaName=\"" + cueSheetName + "\" Expand=\"True\" OoUniqId=\"" + guid.ToString()
            + "\" CueSheetPaddingSize=\"2048\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoCueSheet\">");

        //  ------ CUE -------

        //  --------------------
        //  MIDI to Callback Cue
        //  --------------------
        if(noteInfoList.Count > 0){
            MakeCallbackCueXml(sw, noteInfoList);
        }
            
        //  --------------------

        sw.WriteLine("          </Orca>");


    }

    void CreateCueXML(StreamWriter sw, string workunitName, string cueName, int cueId, List<Adx2Track> trackList)
    {


        string cueString = "            <Orca OrcaName=\"" + cueName + "\" SynthType=\"SynthPolyphonic\" CueID=\"" + cueId.ToString() + "\" ";
        if (defaultGroupCategory != String.Empty)
        {
            //  CategoryGroup/CategoryName
            //  Category0="/CriAtomCraftV2Root/GlobalSettings/Categories/CategoryGroup_0/Category_0"
            cueString += "Category0=\"/CriAtomCraftV2Root/GlobalSettings/Categories/CategoryGroup_0/";
            cueString += defaultGroupCategory;
            cueString += "\" ";
        }
        cueString += "Pos3dDistanceMin=\"" + pos3dDistanceMin + "\" Pos3dDistanceMax=\"" + pos3dDistanceMax + "\" "; 
        cueString += "Pos3dDopplerCoefficient=\"" + pos3dDopplerCoefficient + "\" ";

        cueString += "CuePriority=\"" + cuePriority + "\" ";

        cueString += "DisplayUnit=\"Frame5994\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoCueSynthCue\">";

        sw.WriteLine(cueString);

        foreach (var track in trackList)
        {

            this.CreateTrackXML(sw, workunitName, track.materialName, track.loopFlag, track.pitch, track.pan);
        }

        sw.WriteLine("            </Orca>");

    }

    void CreateTrackXML(StreamWriter sw, string workunitName, string materialName, bool loopFlag, int pitch, int pan)
    {

        sw.WriteLine("              <Orca OrcaName=\"Track_" + materialName + "\" SynthType=\"Track\" Pitch=\"" + pitch + "\" SwitchRange=\"0.5\" DisplayUnit=\"Frame5994\" ObjectColor=\"30, 200, 100, 180\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoCueSynthTrack\">");

        string acOoCueSynthWaveformStr = "                <Orca OrcaName=\"" + materialName + ".wav\" ";

        if (loopFlag == false)
        {
            //  ループ無効化
            acOoCueSynthWaveformStr += "IgnoreLoop=\"True\" ";
        }

        acOoCueSynthWaveformStr += "Pan3dAngle=\"" + pan.ToString() + "\" ";

        ///CriAtomCraftV2Root/WorkUnits/CheapChip_MaterialInfo/　は省略も可だが一応
        acOoCueSynthWaveformStr += "LinkWaveform=\"CriAtomCraftV2Root/WorkUnits/" + workunitName + "_MaterialInfo/MaterialRootFolder/" + materialName + ".wav\" "
        //  Pan3D or Auto
        //+ " PanType=\"Auto\"";
        + " OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoCueSynthWaveform\" />";
        
        sw.WriteLine(acOoCueSynthWaveformStr);

        sw.WriteLine("              </Orca>");
    }

    void MakeMaterialinfo(string outputPath, string workunitName)
    {
        string filePath = outputPath + "/" + workunitName + "/" + workunitName + ".materialinfo";

        Debug.Log("Make MaterialInfo FilePath: " + filePath);

        if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        StreamWriter sw;
        FileInfo fi;
        fi = new FileInfo(filePath);
        sw = fi.AppendText();

        sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sw.WriteLine("<!-- Orca XML File Format -->");
        sw.WriteLine("<OrcaTrees ObjectTypeExpression=\"Full\" BinaryEncodingType=\"Base64\" FileVersion=\"3\" FileRevision=\"0\">");
        sw.WriteLine("  <OrcaTree OrcaName=\"(no name)\">");
        sw.WriteLine("    <Orca OrcaName=\"WorkUnit_" + workunitName + "_MaterialInfo\" VersionInfo=\"Ver.2.19.02\" FormatVersion=\"Ver.1.00.02\" MaterialInfoPath=\"\" MaterialRootPath=\"Materials\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoMaterialInfoFile\">");
        sw.WriteLine("      <Orca OrcaName=\"MaterialRootFolder\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoWaveformFolder\">");

        //  ----- WAV ------


        //  --------------------

        sw.WriteLine("      </Orca>");
        sw.WriteLine("    </Orca>");
        sw.WriteLine("  </OrcaTree>");
        sw.WriteLine("</OrcaTrees>");
        sw.WriteLine("<!-- Copyright (c) CRI Middleware Co.,LTD. -->");
        sw.WriteLine("<!-- end of document -->");

        sw.Flush();
        sw.Close();
    }

    string ChangeName(string name)
    {
        name = name.Replace('(', '_');
        name = name.Replace(')', '_');
        name = name.Replace('&', '_');
        name = name.Replace(':', '_');
        name = name.Replace('/', '_');
        name = name.Replace('.', '_');
        name = name.Replace(' ', '_');
        return name;
    }



    /// <summary>
    /// MIDIからNoteのタイミングのCallbackのキューを作成
    /// </summary>
    void MakeCallbackCueXml(StreamWriter sw, List<MidiInfo> noteInfoList)
    {
//    <Orca OrcaName="Cue_Note" CueID="1000" SynthType="SynthPolyphonic" OrcaType="CriMw.CriAtomCraft.AcCore.AcOoCueSynthCue">
//    <Orca OrcaName="SeqEnd" EventEnableFlag="True" EventEndTime="0" EventStartTime="1200" OrcaType="CriMw.CriAtomCraft.AcCore.AcOoEventOff" />
//    <Orca OrcaName="Callback" CallbackId="0" CallbackTag="60" EventEnableFlag="True" EventEndTime="0" EventStartTime="0" OrcaType="CriMw.CriAtomCraft.AcCore.AcOoEventCallback" />
//    <Orca OrcaName="Callback" CallbackId="1" CallbackTag="62" EventEnableFlag="True" EventEndTime="0" EventStartTime="400" OrcaType="CriMw.CriAtomCraft.AcCore.AcOoEventCallback" />
//    <Orca OrcaName="Callback" CallbackId="2" CallbackTag="65" EventEnableFlag="True" EventEndTime="0" EventStartTime="800" OrcaType="CriMw.CriAtomCraft.AcCore.AcOoEventCallback" />
//    <Orca OrcaName="Track" ObjectColor="200, 40, 120, 200" OrcaType="CriMw.CriAtomCraft.AcCore.AcOoCueSynthTrack" />
//    </Orca>

        sw.WriteLine("          <Orca OrcaName=\"Cue_Note\" CueID=\"1000\" SynthType=\"SynthPolyphonic\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoCueSynthCue\">");

        int callbackId = 0;
        foreach(MidiInfo noteInfo in noteInfoList)
        {
            //  コールバック
            sw.WriteLine("            <Orca OrcaName=\"Callback\"" +
                " CallbackId=\""+ callbackId +"\" CallbackTag=\"" + noteInfo.noteNo + "\" EventStartTime=\"" + noteInfo.time + "\" " +
                "EventEnableFlag=\"True\" EventEndTime=\"0\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoEventCallback\" />");
        }
        //  空のトラック
        sw.WriteLine("      <Orca OrcaName=\"Track\" ObjectColor=\"200, 40, 120, 200\" OrcaType=\"CriMw.CriAtomCraft.AcCore.AcOoCueSynthTrack\" />");

        sw.WriteLine("          </Orca>");
    }
}
