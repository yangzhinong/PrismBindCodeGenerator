using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PrismBindCodeGenerator
{
    internal partial class Program
    {
        private static void Main(string[] args)
        {
            if (args == null || args.Length < 2) { return; }

            var fileName = args[0];
            var key = args[1];
            if (key == "prop")
            {
                CreatePropertyFile(ref fileName);
            }
            else if (key == "cmd")
            {
                CreateCommandFile(ref fileName);
            }
        }

        private static void CreateCommandFile(ref string fileName)
        {
            if (!fileName.EndsWith(".cs")) return;

            var sbHead = new System.Text.StringBuilder();
            var sbTxt = new System.Text.StringBuilder();

            var lines = File.ReadLines(fileName).ToList();
            var regTag = new Regex(@"^\s*\[PrismBindCmd\]\s*$");
            var regClass = new Regex(@"(public|private|partial)\s+class\s+[A-Z]\w+");
            var regSplit = new Regex(@"\s+");
            var regSplit2 = new Regex(@"\(");
            var iClassLine = 0;

            bool isReadToClassDeclareLine = false;

            for (var i = 0; i < lines.Count - 1; i++)
            {
                var lineOne = lines[i];
                var lineTwo = lines[i + 1];
                if (!isReadToClassDeclareLine)
                {
                    sbHead.AppendLine(lineOne);
                }
                if (!isReadToClassDeclareLine && regClass.IsMatch(lineOne))
                {
                    isReadToClassDeclareLine = true;
                    iClassLine = i;
                }
                if (isReadToClassDeclareLine && regTag.IsMatch(lineOne))
                {
                    var keywords = regSplit.Split(lineTwo).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                    if (keywords != null && keywords.Count() >= 3)
                    {
                        var type = keywords[1];
                        var name = regSplit2.Split(keywords[2])[0];
                        var privateMethodName = name;
                        var Name = name;

                        if (name.StartsWith("m_"))
                        {
                            Name = name.Substring(2);
                        }
                        else if (name.StartsWith('_'))
                        {
                            Name = name.Substring(1);
                        }
                        if (name.StartsWith("Execute"))
                        {
                            Name = name.Substring("Execute".Length);
                        }
                        if (!Name.EndsWith("Command") && !Name.EndsWith("Cmd"))
                        {
                            Name += "Cmd";
                        }

                        Name = Name.Substring(0, 1).ToUpper() + Name.Substring(1);
                        name = "_" + Name.Substring(0, 1).ToLower() + Name.Substring(1);
                        sbTxt.AppendLine($"\tprivate DelegateCommand {name};");
                        sbTxt.AppendLine("\tpublic DelegateCommand " + Name + "=>\r\n" +
                                          $"\t\t {name} ?? ({name}= new DelegateCommand({privateMethodName}));");
                    }
                }
            }
            if (sbTxt.Length > 0)
            {
                fileName = fileName.Substring(0, fileName.Length - 3) + "_cmd.cs";

                File.WriteAllText(fileName, sbHead.ToString() + "{\r\n"
                                            + sbTxt.ToString() + "\r\n"
                                            + "}}");
            }
        }

        private static void CreatePropertyFile(ref string fileName)
        {
            if (!fileName.EndsWith(".cs")) return;

            var sbHead = new System.Text.StringBuilder();
            var sbTxt = new System.Text.StringBuilder();

            var lines = File.ReadLines(fileName).ToList();
            var regTag = new Regex(@"^\s*\[PrismBindProperty\]\s*$");
            var regClass = new Regex(@"(public|private|partial)\s+class\s+[A-Z]\w+");
            var regSplit = new Regex(@"\s+");
            var regSplit2 = new Regex("[=;]");
            var iClassLine = 0;

            bool isReadToClassDeclareLine = false;

            for (var i = 0; i < lines.Count - 1; i++)
            {
                var lineOne = lines[i];
                var lineTwo = lines[i + 1];
                if (!isReadToClassDeclareLine)
                {
                    sbHead.AppendLine(lineOne);
                }
                if (!isReadToClassDeclareLine && regClass.IsMatch(lineOne))
                {
                    isReadToClassDeclareLine = true;
                    iClassLine = i;
                }
                if (isReadToClassDeclareLine && regTag.IsMatch(lineOne))
                {
                    var keywords = regSplit.Split(lineTwo).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                    if (keywords != null && keywords.Count() >= 3)
                    {
                        var type = keywords[1];
                        var name = regSplit2.Split(keywords[2])[0];
                        var Name = name;
                        if (name.StartsWith("m_"))
                        {
                            Name = name.Substring(2);
                        }
                        else if (name.StartsWith('_'))
                        {
                            Name = name.Substring(1);
                        }
                        Name = Name.Substring(0, 1).ToUpper() + Name.Substring(1);
                        sbTxt.AppendLine($"\tpublic {type} {Name}");
                        sbTxt.AppendLine("\t{\r\n" +
                                          "\t\tget => 0;".Replace("0", name));
                        sbTxt.AppendLine("\t\tset { SetProperty(ref 0, value); }\r\n\t}"
                                          .Replace("0", name));
                    }
                }
            }
            if (sbTxt.Length > 0)
            {
                fileName = fileName.Substring(0, fileName.Length - 3) + "_prop.cs";

                File.WriteAllText(fileName, sbHead.ToString() + "\t{\r\n\r\n"
                                            + sbTxt.ToString() + "\r\n"
                                            + "\t}\r\n}");
            }
        }
    }
}