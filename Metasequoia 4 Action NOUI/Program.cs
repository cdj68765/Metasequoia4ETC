using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Metasequoia_4_Action_NOUI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            byte[] read = new byte[102473];
            new GZipStream(new MemoryStream((byte[])new BinaryFormatter().Deserialize(new MemoryStream(Resource1.Data))), CompressionMode.Decompress).Read(read, 0, 102473);
            CompilerResults Coderes = new CSharpCodeProvider().CreateCompiler().CompileAssemblyFromSource(new CompilerParameters(new string[] { "System.dll", "System.Core.dll" }), System.Text.Encoding.ASCII.GetString(read).Replace("???", ""));
            if (!Coderes.Errors.HasErrors)
            {
                Coderes.CompiledAssembly.CreateInstance("Reg.Program").GetType().GetMethod("Main").Invoke(Coderes.CompiledAssembly.CreateInstance("Reg.Program"), null);
                MessageBox.Show("激活完毕");
            }
            /*MemoryStream ms = new MemoryStream();
            DeflateStream zip = new DeflateStream(ms, CompressionMode.Compress, true);
            zip.Write(Resource1.Program, 0, Resource1.Program.Length);
            zip.Close();
            byte[] buffer = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buffer, 0, buffer.Length);
            ms.Close();
            FileStream fs = new FileStream(@"C:\text.txt", FileMode.OpenOrCreate);
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
            IFormatter formatter2 = new BinaryFormatter();
            Stream Filestream = new FileStream(Environment.CurrentDirectory + @"\Data.dat", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter2.Serialize(Filestream, buffer);
            Filestream.Close();*/

            /*CSharpCodeProvider NewCode = new CSharpCodeProvider();
            ICodeCompiler CodeTree = NewCode.CreateCompiler();
            CompilerParameters CodeParameters = new CompilerParameters();
            CodeParameters.ReferencedAssemblies.Add("System.dll");
            CodeParameters.ReferencedAssemblies.Add("System.Core.dll");
            CodeParameters.GenerateExecutable = false;//不生成实例程序
            CodeParameters.GenerateInMemory = true;//在内存中生成
            CompilerResults Coderes = CodeTree.CompileAssemblyFromSource(CodeParameters, Resource1.Program);
            if (!Coderes.Errors.HasErrors)
            {
                Assembly OpenCode = Coderes.CompiledAssembly;
                object code = OpenCode.CreateInstance("Reg.Program");
                MethodInfo CodeInfo = code.GetType().GetMethod("Main");
                CodeInfo.Invoke(code, null);
            }*/
        }
    }
}