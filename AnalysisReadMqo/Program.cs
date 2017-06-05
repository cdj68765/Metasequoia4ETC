using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace AnalysisReadMqo
{
    internal class Program
    {
        //static List<MqoInfo.MQOAttribute> Attribute = new List<MqoInfo.MQOAttribute>();
        private static void Main(string[] args)
        {
            //var Filepath = @"C:\Users\Administrator\Desktop\Te.mqo";
            var havemqoxml = true;
            var mqo = new MqoInfo();
            //TextReader tr = new StreamReader(@"C:\test.mqo", Encoding.GetEncoding("Shift_JIS"));
            using (TextReader tr = new StreamReader(new MemoryStream(Resource1.hito_mmd_mqo), Encoding.GetEncoding(936)))
            {
                if ("Metasequoia Document" != tr.ReadLine())
                {
                }
                var Version =
                    decimal.Parse(new Regex(@"^Format Text Ver (\d+(?:\.\d+)?)$").Match(tr.ReadLine()).Groups[1].Value);
                var bmp = new Bitmap(128, 128);
                var map = new List<MqoInfo.ColorPoint>();

                while (true)
                {
                    try
                    {
                        var str = tr.ReadLine().Trim();
                        if (str.StartsWith("Eof"))
                        {
                            break;
                        }
                        if (str.StartsWith("IncludeXml"))
                        {
                            havemqoxml = true;
                        }
                        else if (str.StartsWith("Thumbnail"))
                        {
                            #region Thumbnail

                            var PosionY = 0;
                            while (true)
                            {
                                var PosionX = 0;
                                for (int j = 0; j < 4; j++)
                                {
                                    string ReadALine = tr.ReadLine().Trim();
                                    if (ReadALine.EndsWith("}"))
                                    {
                                        goto End;
                                    }
                                    for (int i = 0; i < ReadALine.Length; i += 6)
                                    {
                                        Color newcolor = Color.FromArgb(255,
                                            Convert.ToInt32(ReadALine[i].ToString() + ReadALine[i + 1].ToString(), 16),
                                            Convert.ToInt32(ReadALine[i + 2].ToString() + ReadALine[i + 3].ToString(), 16),
                                            Convert.ToInt32(ReadALine[i + 4].ToString() + ReadALine[i + 5].ToString(), 16));
                                        map.Add(new MqoInfo.ColorPoint(PosionX, PosionY, newcolor));
                                        PosionX++;
                                    }
                                }
                                PosionY++;
                            }
                            End:
                            map.ForEach(TE => { bmp.SetPixel(TE.x, TE.y, TE.color); });

                            #endregion Thumbnail
                        }
                        else if (str.StartsWith("Scene"))
                        {
                            #region Scene

                            var depth = 0;
                            while (true)
                            {
                                if (depth < 0)
                                {
                                    break;
                                }
                                ;
                                var Sstr = tr.ReadLine().Trim();
                                if (Sstr.EndsWith("{")) // ライト設定などは読み飛ばす
                                {
                                    depth++;
                                }
                                else if (Sstr.EndsWith("}"))
                                {
                                    if (depth > 0) depth--;
                                    else
                                    {
                                        break;
                                    }
                                    ;
                                }
                                else if (depth == 0) // depth=0 の属性値だけ読み込む
                                {
                                    var m = new Regex(@"^(\w+) (.+)$", RegexOptions.Compiled).Match(Sstr);
                                    if (!m.Success)
                                    {
                                        mqo.Attribute.Add(null);
                                        continue;
                                    }
                                    var ma = new MqoInfo.MQOAttribute();
                                    ma.Name = m.Groups[1].Value;
                                    var mc =
                                        new Regex(@"(-?\d+(?:\.\d+)?)", RegexOptions.Compiled).Matches(m.Groups[2].Value);
                                    ma.Values = new decimal[mc.Count];
                                    for (var i = 0; i < mc.Count; i++) ma.Values[i] = decimal.Parse(mc[i].Value);
                                    if (ma != null)
                                    {
                                        mqo.Attribute.Add(ma);
                                    }
                                }
                            }

                            #endregion Scene
                        }
                        else if (str.StartsWith("BackImage"))
                        {
                            #region BackImage

                            while (true)
                            {
                                var Bistr = tr.ReadLine().Trim();
                                var m =
                                    new Regex(
                                        "^(\\w+) \"(.*)\" " + @"(-?\d+(?:\.\d+)?)" + " " + @"(-?\d+(?:\.\d+)?)" + " " +
                                        @"(-?\d+(?:\.\d+)?)" + " " + @"(-?\d+(?:\.\d+)?)", RegexOptions.Compiled).Match(
                                            Bistr);
                                if (m.Success)
                                {
                                    mqo.BackImage.Add(new MqoInfo.MQOBackImage(
                                        m.Groups[1].Value, m.Groups[2].Value,
                                        decimal.Parse(m.Groups[3].Value), decimal.Parse(m.Groups[4].Value),
                                        decimal.Parse(m.Groups[5].Value), decimal.Parse(m.Groups[6].Value)));
                                }
                                else if (Bistr.EndsWith("}"))
                                {
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            #endregion BackImage
                        }
                        else if (str.StartsWith("MaterialEx2"))
                        {
                            #region MaterialEx2

                            var count = 0;
                            while (true)
                            {
                                var QMstr = tr.ReadLine().Trim();
                                if (QMstr.EndsWith("}"))
                                {
                                    break;
                                }
                                if (QMstr.StartsWith("material"))
                                {
                                    var TempEx2 = new MqoInfo.MaterialEx2();
                                    while (true)
                                    {
                                        if (QMstr.EndsWith("{"))
                                        {
                                            count++;
                                        }
                                        QMstr = tr.ReadLine().Trim();
                                        var QmDate = QMstr.Split(' ');
                                        if (QMstr.EndsWith("}"))
                                        {
                                            count--;
                                        }
                                        if (count == 0)
                                        {
                                            mqo.MEx2.Add(TempEx2);
                                            TempEx2 = new MqoInfo.MaterialEx2();
                                            break;
                                        }
                                        if (QMstr.StartsWith("shadertype"))
                                        {
                                            TempEx2.shadertype = QmDate[1];
                                        }
                                        else if (QMstr.StartsWith("shadername"))
                                        {
                                            TempEx2.shadername = QmDate[1];
                                        }
                                        else if (QMstr.StartsWith("Toon"))
                                        {
                                            TempEx2.Toon = int.Parse(QmDate[2]);
                                        }
                                        else if (QMstr.StartsWith("Edge"))
                                        {
                                            TempEx2.Edge = int.Parse(QmDate[2]) == 1 ? true : false;
                                        }
                                    }
                                }
                            }

                            #endregion MaterialEx2
                        }
                        else if (str.StartsWith("Material"))
                        {
                            #region Material

                            while (true)
                            {
                                var Mstr = tr.ReadLine().Trim().Replace('"', ' ');
                                if (Mstr.EndsWith("}"))
                                {
                                    break;
                                }
                                var ttm = Mstr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                //Match m = new Regex("^\"(.*)\" (.+)$", RegexOptions.Compiled).Match(Mstr);//分割名字和名字后面的
                                var mat = new MqoInfo.MQOMaterial(ttm[0]);
                                if (mat.parseParams(Mstr.Replace(ttm[0], "")))
                                {
                                    mqo.Material.Add(mat);
                                }
                                else
                                {
                                    mat.Dispose();
                                    break;
                                }
                            }

                            #endregion Material
                        }
                        else if (str.StartsWith("Object"))
                        {
                            #region Object

                            var obj = new List<StringBuilder>();
                            var te = new StringBuilder();
                            te.AppendLine(str);
                            while (true)
                            {
                                var Ostr = tr.ReadLine().Trim();
                                if (Ostr.StartsWith("Eof"))
                                {
                                    break;
                                }
                                if (Ostr.StartsWith("Object"))
                                {
                                    obj.Add(te);
                                    te = new StringBuilder();
                                }
                                te.AppendLine(Ostr);
                            }
                            //Parallel.ForEach(obj, OBJ =>{
                            foreach (var OBJ in obj)
                            {
                                var Objecttemp = new MqoInfo.MQOObject();
                                var Otr = new StringReader(OBJ.ToString());
                                while (true)
                                {
                                    var Ostr = Otr.ReadLine().Trim();
                                    if (Ostr.StartsWith("vertex "))
                                    {
                                        while (true)
                                        {
                                            var Vstr = Otr.ReadLine().Trim();
                                            if (str.EndsWith("}"))
                                            {
                                                break;
                                            }
                                            var mc = new Regex(@"(-?\d+(?:\.\d+)?)", RegexOptions.Compiled).Matches(Vstr);
                                            if (mc.Count != 3)
                                            {
                                                break;
                                            }
                                            Objecttemp.Vertex.Add(
                                                new MqoInfo.MQOObject.MQOVertex(float.Parse(mc[0].Groups[0].Value),
                                                    float.Parse(mc[1].Groups[0].Value),
                                                    float.Parse(mc[2].Groups[0].Value)));
                                        }
                                    }
                                    else if (Ostr.StartsWith("vertexattr "))
                                    {
                                        while (true)
                                        {
                                            var Vtstr = Otr.ReadLine().Trim();
                                            if (Vtstr.StartsWith("uid "))
                                            {
                                                while (true)
                                                {
                                                    var Tempo = Otr.ReadLine().Trim();
                                                    if (Tempo.StartsWith("}"))
                                                    {
                                                        break;
                                                    }
                                                    Objecttemp.vertexattr.Add(int.Parse(Tempo));
                                                }
                                            }
                                            if (Vtstr.StartsWith("}"))
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    else if (Ostr.StartsWith("face "))
                                    {
                                        while (true)
                                        {
                                            var Fstr = Otr.ReadLine().Trim();
                                            if (Fstr.EndsWith("}"))
                                            {
                                                break;
                                            }
                                            MatchCollection mc;
                                            var TempFace = new MqoInfo.MQOObject.MQOFace();
                                            var m = new Regex(@"([234]) (.+)$", RegexOptions.Compiled).Match(Fstr);
                                            var n = int.Parse(m.Groups[1].Value);
                                            TempFace.VertexID = new int[n];
                                            TempFace.UVID = new int[n];
                                            TempFace.MatID = -1;
                                            var noUV = true;
                                            foreach (
                                                Match p in
                                                    new Regex("(?<key>\\w+)\\((?:\"(?<val>.*)\"|(?<val>[^\\)]+))\\)",
                                                        RegexOptions.Compiled).Matches(m.Groups[2].Value))
                                            {
                                                switch (p.Groups["key"].Value)
                                                {
                                                    case "M":
                                                        TempFace.MatID = int.Parse(p.Groups["val"].Value);
                                                        break;

                                                    case "V":
                                                        mc =
                                                            new Regex(@"(-?\d+(?:\.\d+)?)", RegexOptions.Compiled)
                                                                .Matches(p.Groups["val"].Value);
                                                        if (mc.Count != n)
                                                        {
                                                        }
                                                        for (var i = 0; i < n; i++)
                                                            TempFace.VertexID[i] = int.Parse(mc[i].Value);
                                                        break;

                                                    case "UV":
                                                        mc =
                                                            new Regex(@"(-?\d+(?:\.\d+)?)", RegexOptions.Compiled)
                                                                .Matches(p.Groups["val"].Value);
                                                        if (mc.Count != 2 * n)
                                                        {
                                                        }
                                                        noUV = false;
                                                        for (var i = 0; i < n; i++)
                                                        {
                                                            TempFace.UVID[i] =
                                                                MqoInfo.MQOObject.MQOFace.getUVIndex(
                                                                    decimal.Parse(mc[2 * i].Value),
                                                                    decimal.Parse(mc[2 * i + 1].Value));
                                                        }
                                                        break;

                                                    case "UID":
                                                        TempFace.UID = int.Parse(p.Groups["val"].Value);
                                                        break;
                                                }
                                            }
                                            if (noUV)
                                                for (var i = 0; i < n; i++)
                                                    TempFace.UVID[i] = MqoInfo.MQOObject.MQOFace.getUVIndex(0, 0);
                                            Objecttemp.MqoFace.Add(TempFace);
                                        }
                                    }
                                    else if (Ostr.EndsWith("}"))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        var m = new Regex(@"^(\w+) (.+)$", RegexOptions.Compiled).Match(Ostr);
                                        if (!m.Success)
                                        {
                                            mqo.Attribute.Add(null);
                                            continue;
                                        }
                                        var ma = new MqoInfo.MQOAttribute();
                                        ma.Name = m.Groups[1].Value;
                                        var mc =
                                            new Regex(@"(-?\d+(?:\.\d+)?)", RegexOptions.Compiled).Matches(
                                                m.Groups[2].Value);
                                        ma.Values = new decimal[mc.Count];
                                        for (var i = 0; i < mc.Count; i++) ma.Values[i] = decimal.Parse(mc[i].Value);
                                        if (ma != null)
                                        {
                                            Objecttemp.Attribute.Add(ma);
                                        }
                                    }
                                }
                                lock (mqo)
                                {
                                    mqo.Object.Add(Objecttemp);
                                }
                                //});
                            }
                            break;

                            #endregion Object
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            if (havemqoxml)
            {
                /* var mqxpath = new FileInfo(Filepath).DirectoryName + @"/" +
                               new FileInfo(Filepath).Name.Replace("mqo", "") + "mqx";*/
                if (/*new FileInfo(mqxpath).Exists*/true)
                {
                    using (var reader = XmlReader.Create(new MemoryStream(Resource1.hito_mmd_mqx), null))
                    {
                        reader.Read();
                        reader.ReadStartElement("MetasequoiaDocument");
                        reader.ReadStartElement("IncludedBy");
                        var mqo_file = reader.ReadString();
                        Console.WriteLine(mqo_file);
                        reader.ReadEndElement(); //IncludedBy
                        try
                        {
                            reader.ReadStartElement("Plugin.56A31D20.71F282AB");
                        }
                        catch
                        {
                        }
                        reader.ReadStartElement("BoneSet");
                        //List<MqoInfo.MqoBone> boneList = new List<MqoInfo.MqoBone>();
                        //int len = 512;
                        //bones = new MqoBone[len];
                        //int i = 0;
                        while (reader.IsStartElement("Group"))
                        {
                            reader.Read();
                        }
                        while (reader.IsStartElement("Bone"))
                        {
                            var bonetemp = new MqoInfo.MqoBone();
                            bonetemp.id = int.Parse(reader.GetAttribute("id"));
                            bonetemp.name = reader.GetAttribute("name");
                            //maxAngB="180.00000" maxAngH="180.00000" maxAngP="180.00000" minAngB="-180.00000" minAngH="-180.00000" minAngP="-180.00000" IKChain="0" isIK="0" IKParent="0" IKParentIsIK="0" isDummy="0" isLock="0" isHide="0" isMirror="0" isMovable="0" name="upper body" tip_name="upper body_end" ik_name="upper body_IK" ik_tip_name="upper body_IK_end"

                            var rtX = float.Parse(reader.GetAttribute("rtX"));
                            var rtY = float.Parse(reader.GetAttribute("rtY"));
                            var rtZ = float.Parse(reader.GetAttribute("rtZ"));
                            bonetemp.world_position = new MqoInfo.MqoBone.Vector3(rtX, rtY, rtZ);

                            reader.Read(); //Bone

                            if (reader.IsStartElement("P"))
                            {
                                var id = int.Parse(reader.GetAttribute("id"));
                                reader.Read(); //P
                                bonetemp.pid = id;
                            }

                            while (reader.IsStartElement("C"))
                            {
                                var id = int.Parse(reader.GetAttribute("id"));
                                reader.Read(); //C
                                bonetemp.cids.Add(id);
                            }

                            while (reader.IsStartElement("B"))
                            {
                                //reader.Read();
                                var id = int.Parse(reader.GetAttribute("id"));
                                reader.Read(); //C
                                bonetemp.bids.Add(id);
                            }

                            while (reader.IsStartElement("L"))
                            {
                                reader.Read();
                                /* int id = int.Parse(reader.GetAttribute("id"));
                               reader.Read();//C
                               bonetemp.cids.Add(id);*/
                            }
                            while (reader.IsStartElement("W"))
                            {
                                var weit = new MqoInfo.MqoBone.MqoWeit();
                                weit.object_id = int.Parse(reader.GetAttribute("oi"));
                                weit.vertex_id = int.Parse(reader.GetAttribute("vi"));
                                weit.weit = float.Parse(reader.GetAttribute("w")) * 0.01f;
                                reader.Read(); //W
                                weit.node_id = bonetemp.node_id;
                                bonetemp.Weight.Add(weit);
                            }
                            mqo.MQOBone.Add(bonetemp);
                            reader.ReadEndElement();
                            //Bone
                            /*MqoBone bone = new MqoBone(i);
                            bone.Read(reader);
                            boneList.Add(bone);*/
                            //i++;
                        }
                        reader.ReadEndElement(); //BoneSet
                        while (reader.IsStartElement("Obj"))
                        {
                            reader.Read();
                            /* int id = int.Parse(reader.GetAttribute("id"));
                           reader.Read();//C
                           bonetemp.cids.Add(id);*/
                        }
                        while (reader.IsStartElement("Poses"))
                        {
                            reader.Read();
                            /* int id = int.Parse(reader.GetAttribute("id"));
                           reader.Read();//C
                           bonetemp.cids.Add(id);*/
                        }
                        reader.ReadEndElement(); //Plugin.56A31D20.71F282AB

                        try
                        {
                            reader.ReadStartElement("Plugin.56A31D20.C452C6DB");
                        }
                        catch
                        {
                        }
                        reader.ReadStartElement("MorphSet");
                        reader.ReadStartElement("PmdMode");
                        reader.IsStartElement("TargetList");

                        reader.Read();
                        while (reader.IsStartElement("Target"))
                        {
                            var a1 = reader.GetAttribute("name");
                            var a2 = reader.GetAttribute("param");
                            var a3 = reader.GetAttribute("type");
                            reader.Read();
                        }
                        reader.ReadEndElement(); //TargetList
                        reader.ReadEndElement(); //MorphSet
                        reader.ReadEndElement(); //Plugin.56A31D20.C452C6DB
                        reader.ReadEndElement(); //BoneSet
                    }
                }
            }
        }
    }
}