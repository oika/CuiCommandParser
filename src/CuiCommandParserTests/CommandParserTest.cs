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
    class CommandParserTest
    {
        private static IEnumerable<TestCaseData> GetCommandArgsSource
        {
            get
            {
                var optInfos = new[]
                {
                    Tuple.Create<char?, string, CommandOptionKind>('o', null, CommandOptionKind.NoValue),
                    Tuple.Create<char?, string, CommandOptionKind>('t', "opt-two", CommandOptionKind.HasOptionalValue),
                    Tuple.Create<char?, string, CommandOptionKind>(null, "opt-three", CommandOptionKind.NeedsValue),
                };

                yield return new TestCaseData(false, optInfos, new string[0], new string[0]).SetName("引数なし");
                yield return new TestCaseData(false, optInfos, new[] { "hoge" }, new[] { "hoge" }).SetName("引数ひとつ");
                yield return new TestCaseData(false, optInfos, new[] { "hoge", "ほげ" }, new[] { "hoge", "ほげ" }).SetName("引数ふたつ");
                yield return new TestCaseData(false, optInfos, new[] { "hoge", "-T", "ふが" }, new[] { "hoge", "ふが" }).SetName("不明なオプションを無視する");
                yield return new TestCaseData(false, optInfos, new[] { "hoge", "-t", "ふが", "dd" }, new[] { "hoge", "dd" }).SetName("任意で値をとるオプションを除外 短名");
                yield return new TestCaseData(false, optInfos, new[] { "hoge", "--opt-three", "ふが", "dd" }, new[] { "hoge", "dd" }).SetName("必須の値をとるオプションを除外 長名");
                yield return new TestCaseData(false, optInfos, new[] { "hoge", "-o", "ふが", "dd" }, new[] { "hoge", "ふが", "dd" }).SetName("値をとらないオプションを除外");
            }
        }

        [TestCaseSource(nameof(GetCommandArgsSource))]
        public void GetsCommandArgs(bool ignoresCase, Tuple<char?, string, CommandOptionKind>[] optInfos, string[] args, string[] expected)
        {
            var parser = new CommandParser(ignoresCase);

            foreach (var opt in optInfos)
            {
                parser.RegisterOption(new CommandOption(opt.Item1, opt.Item2, "", "", opt.Item3));
            }

            parser.IgnoresUnknownOptions = true;
            var res = parser.Parse(args);

            Assert.That(res.CommandParameters, Is.EqualTo(expected));
        }

        [Test]
        public void ReturnsNullWhenUnknownOptionFound()
        {
            var parser = new CommandParser();

            Assert.That(parser.Parse(new[] { "-s" }), Is.Null);
            Assert.That(parser.Parse(new[] { "--symbol" }), Is.Null);
        }

        [Test]
        public void ReturnsNullWhenRequiredValueNotFound()
        {
            var parser = new CommandParser();
            parser.RegisterOption(new CommandOption('s', "symbol", "VAL", "", CommandOptionKind.NeedsValue));

            Assert.That(parser.Parse(new[] { "-s" }), Is.Null);
            Assert.That(parser.Parse(new[] { "--symbol" }), Is.Null);
        }


        private static IEnumerable<TestCaseData> HasOptionSource
        {
            get
            {
                var optInfos = new[]
                {
                    Tuple.Create<char?, string, CommandOptionKind>('o', null, CommandOptionKind.NoValue),
                    Tuple.Create<char?, string, CommandOptionKind>('t', "opt-two", CommandOptionKind.HasOptionalValue),
                    Tuple.Create<char?, string, CommandOptionKind>(null, "opt-three", CommandOptionKind.NeedsValue),
                };

                yield return new TestCaseData(false, optInfos, new string[0], "hoge").Returns(false).SetName("引数無し");
                yield return new TestCaseData(false, optInfos, new[] { "hoge", "huga" }, "hoge").Returns(false).SetName("オプション無し");
                yield return new TestCaseData(false, optInfos, new[] { "-h", "hoge" }, 'h').Returns(false).SetName("不明なオプションのみ");
                yield return new TestCaseData(false, optInfos, new[] { "-h", "-o" }, 'o').Returns(true).SetName("不明なオプションと対象のオプション");
                yield return new TestCaseData(false, optInfos, new[] { "-ho" }, 'o').Returns(true).SetName("不明なオプションの後ろに対象のオプションを連結");
                yield return new TestCaseData(false, optInfos, new[] { "-oh" }, 'o').Returns(true).SetName("不明なオプションの前に対象のオプションを連結");
                yield return new TestCaseData(false, optInfos, new[] { "-ot" }, 'o').Returns(true).SetName("有効なオプションの前に対象のオプションを連結");
                yield return new TestCaseData(false, optInfos, new[] { "-ot" }, 't').Returns(true).SetName("有効なオプションの後ろに対象のオプションを連結");
                yield return new TestCaseData(false, optInfos, new[] { "-o" }, 'o').Returns(true).SetName("短名・値なし");
                yield return new TestCaseData(false, optInfos, new[] { "--opt-three", "x" }, "opt-three").Returns(true).SetName("長名・値あり");
                yield return new TestCaseData(false, optInfos, new[] { "--opt-two" }, "opt-two").Returns(true).SetName("長名・値なし");
                yield return new TestCaseData(false, optInfos, new[] { "-t", "え" }, 't').Returns(true).SetName("短名・値あり");
                yield return new TestCaseData(false, optInfos, new[] { "--opt-two" }, 't').Returns(true).SetName("長名で指定されたオプションを短名で取得");
                yield return new TestCaseData(false, optInfos, new[] { "-O" }, 'o').Returns(false).SetName("大文字小文字を区別する 短名");
                yield return new TestCaseData(true, optInfos, new[] { "-O" }, 'o').Returns(true).SetName("大文字小文字を区別しない 短名");
                yield return new TestCaseData(false, optInfos, new[] { "--Opt-Two" }, "OPT-TWO").Returns(false).SetName("大文字小文字を区別する 長名");
                yield return new TestCaseData(true, optInfos, new[] { "--Opt-Two" }, "OPT-TWO").Returns(true).SetName("大文字小文字を区別しない 長名");
            }
        }


        [TestCaseSource("HasOptionSource")]
        public bool GetsWeatherOptionContained(bool ignoresCase, Tuple<char?, string, CommandOptionKind>[] optInfos, string[] args, dynamic key)
        {
            var parser = new CommandParser(ignoresCase);
            parser.IgnoresUnknownOptions = true;

            foreach (var item in optInfos)
            {
                parser.RegisterOption(new CommandOption(item.Item1, item.Item2, "", "", item.Item3));
            }

            return parser.Parse(args).HasOption(key);
        }


        private static IEnumerable<TestCaseData> GetOptionValSource
        {
            get
            {
                var optInfos = new[]
                {
                    Tuple.Create<char?, string, CommandOptionKind>('o', null, CommandOptionKind.NoValue),
                    Tuple.Create<char?, string, CommandOptionKind>('t', "opt-two", CommandOptionKind.HasOptionalValue),
                    Tuple.Create<char?, string, CommandOptionKind>(null, "opt-three", CommandOptionKind.NeedsValue),
                };

                yield return new TestCaseData(false, null, optInfos, new[] { "--opt-three", "ooo" }, 't').Returns(null).SetName("オプション無し 短名");
                yield return new TestCaseData(false, null, optInfos, new[] { "--opt-three", "ooo" }, "opt-two").Returns(null).SetName("オプション無し 長名");
                yield return new TestCaseData(false, null, optInfos, new[] { "-t" }, 't').Returns(null).SetName("オプションの値無し 短名");
                yield return new TestCaseData(false, null, optInfos, new[] { "--opt-two" }, "opt-two").Returns(null).SetName("オプションの値無し 長名");

                yield return new TestCaseData(false, null, optInfos, new[] { "ほげ", "-t", "012" }, 't').Returns("012").SetName("任意で値をとるオプション 短名");
                yield return new TestCaseData(false, null, optInfos, new[] { "ほげ", "--opt-three", "ee" }, "opt-three").Returns("ee").SetName("必須の値をとるオプション 長名");
                yield return new TestCaseData(false, null, optInfos, new[] { "--opt-two", "ee" }, 't').Returns("ee").SetName("長名で指定したオプションを短名で取得");
                yield return new TestCaseData(false, null, optInfos, new[] { "ga", "-t", "ee" }, "opt-two").Returns("ee").SetName("短名で指定したオプションを長名で取得");

                yield return new TestCaseData(false, null, optInfos, new[] { "-h", "-t", "xx" }, 't').Returns("xx").SetName("不明なオプションと対象のオプション");
                yield return new TestCaseData(false, null, optInfos, new[] { "-ht", "xx" }, 't').Returns("xx").SetName("不明なオプションの後ろに対象のオプションを連結");
                yield return new TestCaseData(false, null, optInfos, new[] { "-ot", "え！" }, 't').Returns("え！").SetName("有効なオプションの後ろに対象のオプションを連結");

                yield return new TestCaseData(true, null, optInfos, new[] { "-T", "エエ" }, 't').Returns("エエ").SetName("大文字と小文字を区別しない 短名");
                yield return new TestCaseData(true, null, optInfos, new[] { "--Opt-Three", "ga" }, "OPT-THREE").Returns("ga").SetName("大文字と小文字を区別しない 長名");

                var separators1 = new[] { ':', ' ' };
                yield return new TestCaseData(false, separators1, optInfos, new[] { "-t:tes" }, 't').Returns("tes").SetName("任意の区切り文字 短名");
                yield return new TestCaseData(false, separators1, optInfos, new[] { "--opt-two:tes" }, "opt-two").Returns("tes").SetName("任意の区切り文字 長名");

                var separators2 = new[] { '=' };
                yield return new TestCaseData(false, separators2, optInfos, new[] { "-t", "tes" }, 't').Returns(null).SetName("スペースを区切り文字とみなさない");
            }
        }


        [TestCaseSource("GetOptionValSource")]
        public string GetsOptionValue(bool ignoresCase, char[] separators, Tuple<char?, string, CommandOptionKind>[] optInfos, string[] args, dynamic key)
        {
            var parser = new CommandParser(ignoresCase);
            parser.IgnoresUnknownOptions = true;
            if (separators != null) parser.OptionKeyValueSeparators = separators;
            foreach (var opt in optInfos)
            {
                parser.RegisterOption(new CommandOption(opt.Item1, opt.Item2, "", "", opt.Item3));
            }

            var res = parser.Parse(args);

            return res.GetOptionValue(key);
        }
    }
}
