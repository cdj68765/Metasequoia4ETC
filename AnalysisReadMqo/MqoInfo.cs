using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace AnalysisReadMqo
{
    public class MqoInfo

    {
        public List<MQOAttribute> Attribute = new List<MQOAttribute>();
        public List<MQOBackImage> BackImage = new List<MQOBackImage>();
        public List<MQOMaterial> Material = new List<MQOMaterial>();
        public List<MaterialEx2> MEx2 = new List<MaterialEx2>();
        public List<MqoBone> MQOBone = new List<MqoBone>();
        public List<MQOObject> Object = new List<MQOObject>();

        public class ColorPoint //Thumbnail
        {
            public Color color;
            public int x;
            public int y;

            public ColorPoint(int X, int y, Color color)
            {
                x = X;
                this.y = y;
                this.color = color;
            }
        }

        public class MQOAttribute
        {
            public string Name = "";
            public decimal[] Values = null;
        }

        public class MQOBackImage
        {
            public string Part, Path;
            public decimal X, Y, W, H;

            public MQOBackImage(string part, string path, decimal x, decimal y, decimal w, decimal h)
            {
                Part = part;
                Path = path;
                X = x;
                Y = y;
                W = w;
                H = h;
            }

            public void Dispose()
            {
                Part = null;
                Path = null;
            }
        }

        public class MQOMaterial
        {
            public MQOColor Color, proj_pos, proj_scale, proj_angle;
            public float Diffuse, Power, Ambient, Specular, Emission;

            public string Name;
            public string Tex, Alpha, Bump, shader, aplane, bump, Spa;
            public decimal vcol, proj_typ, uid, dbls;

            public MQOMaterial(string name)
            {
                Name = name;
                Tex = "";
                Alpha = "";
                Bump = "";
                Color = new MQOColor();
            }

            public void Dispose()
            {
                Name = null;
                Tex = null;
                Spa = null;
                Alpha = null;
                Bump = null;
                Color = null;
            }

            internal bool parseParams(string str)
            {
                var mc =
                    new Regex("(?<key>\\w+)\\((?:\"(?<val>.*)\"|(?<val>[^\\)]+))\\)", RegexOptions.Compiled).Matches(str);
                foreach (Match m in mc)
                {
                    var key = m.Groups["key"].Value;
                    var val = m.Groups["val"].Value;
                    switch (key)
                    {
                        case "shader":
                            shader = val;
                            break;

                        case "tex":
                            String[] TexTemp = val.Split('*');
                            try
                            {
                                Tex = TexTemp[0];
                                Spa = TexTemp[1];
                            }
                            catch (Exception)
                            {
                            }
                            //Tex = val;
                            break;

                        case "aplane":
                            Alpha = val;
                            break;

                        case "bump":
                            Bump = val;
                            break;

                        case "col":
                            var c = new Regex(@"(-?\d+(?:\.\d+)?)", RegexOptions.Compiled).Matches(val);
                            if (c.Count != 4) return false;
                            var Color = new MQOColor();
                            for (var i = 0; i < 4; i++) Color.SetValue(i, decimal.Parse(c[i].Groups[0].Value));
                            break;

                        case "dif":
                            Diffuse = float.Parse(val);
                            break;

                        case "amb":
                            Ambient = float.Parse(val);
                            break;

                        case "emi":
                            Emission = float.Parse(val);
                            break;

                        case "spc":
                            Specular = float.Parse(val);
                            break;

                        case "power":
                            Power = float.Parse(val);
                            break;

                        case "uid":
                            uid = decimal.Parse(val);
                            break;

                        case "dbls":
                            dbls = decimal.Parse(val);
                            break;

                        default:
                            Console.WriteLine();
                            break;
                    }
                }
                return true;
            }

            public class MQOColor
            {
                public decimal R, G, B, A;

                public void SetValue(int i, decimal v)
                {
                    switch (i)
                    {
                        case 0:
                            R = v;
                            break;

                        case 1:
                            G = v;
                            break;

                        case 2:
                            B = v;
                            break;

                        case 3:
                            A = v;
                            break;
                    }
                }
            }
        }

        public class MaterialEx2
        {
            public bool Edge;
            public string shadername;
            public string shadertype;
            public int Toon;
        }

        public class MQOObject
        {
            public List<MQOAttribute> Attribute = new List<MQOAttribute>();
            public List<MQOFace> MqoFace = new List<MQOFace>();
            public List<MQOVertex> Vertex = new List<MQOVertex>();
            public List<int> vertexattr = new List<int>();

            public class MQOVertex
            {
                public float X, Y, Z;

                public MQOVertex(float x, float y, float z)
                {
                    X = x;
                    Y = y;
                    Z = z;
                }
            }

            public class MQOFace
            {
                private static readonly List<MQOUV> UV = new List<MQOUV>();
                public int MatID;
                public int UID;
                public int[] UVID;
                public int[] VertexID;

                public static int getUVIndex(decimal u, decimal v)
                {
                    var idx = UV.FindLastIndex(uv => uv.U == u && uv.V == v);
                    if (true && idx < 0)
                    {
                        idx = UV.Count;
                        UV.Add(new MQOUV(u, v));
                    }
                    return idx;
                }

                public class MQOUV
                {
                    public decimal U, V;

                    public MQOUV(decimal u, decimal v)
                    {
                        U = u;
                        V = v;
                    }

                    public bool Equals(MQOUV other)
                    {
                        return U == other.U && V == other.V;
                    }
                }
            }
        }

        public class MqoBone
        {
            public List<int> bids = new List<int>();

            //子id
            public List<int> cids = new List<int>();

            public int id; //id in mqx file
            public Vector3 local_position;
            public string name;
            public int node_id; //id in bonearray used by your program

            //先端position
            public Vector3 p;

            public MqoBone parent;
            public string path;

            //親id
            //なければ0
            public int pid;

            //根本position
            public Vector3 q;

            public bool tail;
            public bool turned;
            public List<MqoWeit> Weight = new List<MqoWeit>();

            public List<MqoWeit> weits;

            public Vector3 world_position;
            public bool world_turned;

            public class Vector3
            {
                public float X, Y, Z;

                public Vector3(float x, float y, float z)
                {
                    X = x;
                    Y = y;
                    Z = z;
                }
            }

            public class MqoWeit
            {
                public int node_id; //idx of bones
                public int object_id;
                public int vertex_id;
                public float weit;
            }
        }

        #region IDisposable Support

        private bool disposedValue = true; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~MqoInfo() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}