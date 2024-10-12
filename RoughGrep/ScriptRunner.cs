using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Schemy;
using TrivialBehind;

namespace RoughGrep
{
    public class ScriptRunner
    {
        public static void RunScript(string pth)
        {
            var basedir = Path.GetDirectoryName(pth);
            Interpreter.CreateSymbolTableDelegate extension = _ => new Dictionary<Symbol, object>()
            {
                // generic stuff
                {
                    Symbol.FromString("sprintf"),
                    NativeProcedure.Create<string, List<object>, string>(
                        (fmt, parts) => Sprintf(fmt, parts)
                    )
                },
                // roughgrep stuff
                {
                    Symbol.FromString("make-runner"),
                    NativeProcedure.Create<string, string, string, object>(
                        (bin, arg, workdir) => MakeRunner(bin, arg, workdir)
                    )
                },
                {
                    Symbol.FromString("add-command"),
                    NativeProcedure.Create<string, CmdRunner, object>(
                        (pat, cmspec) => AddCommand(pat, cmspec)
                    )
                },
                {
                    Symbol.FromString("path-rel"),
                    NativeProcedure.Create<string, string>(s => Path.Combine(basedir, s))
                },
                {
                    Symbol.FromString("set-arg"),
                    NativeProcedure.Create<string, object>(s => SetArg(s))
                },
                {
                    Symbol.FromString("set-tutorial"),
                    NativeProcedure.Create<string, object>(s => SetTutorial(s))
                },
            };

            var interpreter = new Interpreter(
                new[] { extension },
                new ReadOnlyFileSystemAccessor()
            );
            using (Stream script = File.OpenRead(pth))
            using (TextReader reader = new StreamReader(script))
            {
                var res = interpreter.Evaluate(reader);
                if (res.Error != null)
                    Logic.Tutorial = res.Error.Message;
            }
        }

        private static object SetTutorial(string s)
        {
            Logic.Tutorial = s;
            return None.Instance;
        }

        private static string Sprintf(string fmt, List<object> parts)
        {
            return string.Format(fmt, parts.ToArray());
        }

        private static object MakeRunner(string bin, string arg, string workdir)
        {
            return new CmdRunner
            {
                Arg = arg,
                Bin = bin,
                Workdir = workdir,
            };
        }

        private static object AddCommand(string pat, CmdRunner cmspec)
        {
            Logic.ExternalCommands.Add(new ExternalCommand { Pattern = pat, Runner = cmspec });
            return None.Instance;
        }

        static object SetArg(string s)
        {
            Logic.RgExtraArgs = s;
            return None.Instance;
        }

        // finds the ini script
        public static string FindScript()
        {
            var curDir = Logic.WorkDir;
            while (curDir != null)
            {
                var trie = Path.Combine(curDir, "RoughGrep.ss");

                if (File.Exists(trie))
                {
                    return trie;
                }
                curDir = Path.GetDirectoryName(curDir);
            }

            return null;
        }
    }
}
