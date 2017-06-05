using System.IO;
using System.IO.Compression;

namespace Metasequoia_4_CS2Bin
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            /* using (FileStream originalFileStream = new FileStream(@"C:\Program", FileMode.Open))
             {
                 using (FileStream compressedFileStream = File.Create(@"C:\text"))
                 {
                     using (
                         DeflateStream compressionStream = new DeflateStream(compressedFileStream,
                             CompressionMode.Compress))
                     {
                         originalFileStream.CopyTo(compressionStream);
                     }
                 }
             }*/
            using (var decompressedFileStream = new MemoryStream())
            {
                using (DeflateStream decompressionStream = new DeflateStream(new MemoryStream(Resource1.text), CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(decompressedFileStream);
                    using (var file = new FileStream(@"C:\t.txt", FileMode.OpenOrCreate))
                    {
                        file.Write(decompressedFileStream.ToArray(), 0, (int)decompressedFileStream.Length);
                    }
                }
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