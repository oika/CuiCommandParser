using NUnit.Framework;
using Oika.Libs.CuiCommandParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuiCommandParserTests
{
    [TestFixture]
    class ParsedCommandInfoTest
    {
        private static Dictionary<CommandOptionKey, string> buildOptDic()
        {
            var dic = new Dictionary<CommandOptionKey, string>();
            dic[new CommandOptionKey('s', "saas", false)] = null;
            dic[new CommandOptionKey('p', "paas", false)] = "PAAS_VAL";
            dic[new CommandOptionKey('i', "iaas", true)] = null;
            dic[new CommandOptionKey(null, "caas", true)] = "CAAS_VAL";
            dic[new CommandOptionKey('t', null, false)] = null;
            dic[new CommandOptionKey('l', null, true)] = "LAAS_VAL";
            return dic;
        }

        private static IEnumerable<TestCaseData> HasOptionSource
        {
            get
            {
                var dic = buildOptDic();

                yield return new TestCaseData(false, dic, new CommandOption('s', null, "", "", CommandOptionKind.NoValue))
                                    .Returns(true).SetName("短名一致");
                yield return new TestCaseData(false, dic, new CommandOption(null, "saas", "", "", CommandOptionKind.NoValue))
                                    .Returns(true).SetName("長名一致");
                yield return new TestCaseData(false, dic, new CommandOption(null, "s", "", "", CommandOptionKind.NoValue))
                                    .Returns(false).SetName("短名を長名として指定");
                yield return new TestCaseData(false, dic, new CommandOption('S', null, "", "", CommandOptionKind.NoValue))
                                    .Returns(false).SetName("短名ケース不一致（非許容）");
                yield return new TestCaseData(true, dic, new CommandOption('I', null, "", "", CommandOptionKind.NoValue))
                                    .Returns(true).SetName("短名ケース不一致（許容）");
                yield return new TestCaseData(false, dic, new CommandOption(null, "saaS", "", "", CommandOptionKind.NoValue))
                                    .Returns(false).SetName("長名ケース不一致（非許容）");
                yield return new TestCaseData(true, dic, new CommandOption(null, "iaaS", "", "", CommandOptionKind.NoValue))
                                    .Returns(true).SetName("長名ケース不一致（許容）");
                yield return new TestCaseData(false, dic, new CommandOption('s', "none", "", "", CommandOptionKind.NoValue))
                                    .Returns(true).SetName("短名のみ一致");
                yield return new TestCaseData(false, dic, new CommandOption('x', "saas", "", "", CommandOptionKind.NoValue))
                                    .Returns(true).SetName("長名のみ一致");
            }
        }

        [TestCaseSource("HasOptionSource")]
        public bool オプションが指定されているかどうかを調べる(bool ignoresCase, Dictionary<CommandOptionKey, string> optDic, CommandOption target)
        {
            var info = new ExParsedInfo(ignoresCase, new string[0], optDic);
            return info.HasOption(target);
        }

        private static IEnumerable<TestCaseData> GetValueSource
        {
            get
            {
                var dic = buildOptDic();

                yield return new TestCaseData(false, dic, new CommandOption('x', null, "", "", CommandOptionKind.HasOptionalValue))
                                    .Returns(null).SetName("指定されていないオプション");
                yield return new TestCaseData(false, dic, new CommandOption('s', null, "", "", CommandOptionKind.NoValue))
                                    .Returns(null).SetName("値のないオプション");
                yield return new TestCaseData(false, dic, new CommandOption('p', null, "", "", CommandOptionKind.HasOptionalValue))
                                    .Returns("PAAS_VAL").SetName("短名一致");
                yield return new TestCaseData(false, dic, new CommandOption(null, "paas", "", "", CommandOptionKind.HasOptionalValue))
                                    .Returns("PAAS_VAL").SetName("長名一致");
                yield return new TestCaseData(false, dic, new CommandOption('p', "none", "", "", CommandOptionKind.HasOptionalValue))
                                    .Returns("PAAS_VAL").SetName("短名のみ一致");
                yield return new TestCaseData(false, dic, new CommandOption('x', "paas", "", "", CommandOptionKind.HasOptionalValue))
                                    .Returns("PAAS_VAL").SetName("長名のみ一致");
                yield return new TestCaseData(false, dic, new CommandOption('P', null, "", "", CommandOptionKind.HasOptionalValue))
                                    .Returns(null).SetName("短名ケース不一致（非許容）");
                yield return new TestCaseData(false, dic, new CommandOption(null, "paaS", "", "", CommandOptionKind.HasOptionalValue))
                                    .Returns(null).SetName("長名ケース不一致（非許容）");
                yield return new TestCaseData(true, dic, new CommandOption('L', null, "", "", CommandOptionKind.HasOptionalValue))
                                    .Returns("LAAS_VAL").SetName("短名ケース不一致（許容）");
                yield return new TestCaseData(true, dic, new CommandOption(null, "caaS", "", "", CommandOptionKind.HasOptionalValue))
                                    .Returns("CAAS_VAL").SetName("長名ケース不一致（許容）");
            }
        }


        [TestCaseSource("GetValueSource")]
        public string オプションの値を取得する(bool ignoresCase, Dictionary<CommandOptionKey, string> optDic, CommandOption target)
        {
            var info = new ExParsedInfo(ignoresCase, new string[0], optDic);
            return info.GetOptionValue(target);
        }

    }

    class ExParsedInfo : ParsedCommandInfo
    {
        public ExParsedInfo(bool ignoresCase, IReadOnlyList<string> commandParams, Dictionary<CommandOptionKey, string> optionDic)
            : base(ignoresCase, commandParams, optionDic)
        {
        }
    }
}
