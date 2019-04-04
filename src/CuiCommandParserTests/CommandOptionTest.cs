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
    class CommandOptionTest
    {
        [Test]
        public void CreatesNewInstance()
        {
            var opt = new CommandOption('n', "long", "value name", "オプション です", CommandOptionKind.HasOptionalValue);

            Assert.That(opt.ShortName, Is.EqualTo('n'));
            Assert.That(opt.LongName, Is.EqualTo("long"));
            Assert.That(opt.ValueName, Is.EqualTo("value name"));
            Assert.That(opt.Expression, Is.EqualTo("オプション です"));
            Assert.That(opt.OptionKind, Is.EqualTo(CommandOptionKind.HasOptionalValue));
            Assert.That(opt.ShowsInUsage, Is.EqualTo(true));
        }

        [TestCase('s', "", "", CommandOptionKind.NoValue, ExpectedResult = 2, TestName = "左カラムの幅計算：最小")]
        [TestCase(null, "hh", "VALUE", CommandOptionKind.NeedsValue, ExpectedResult = 14/* "    --hh VALUE" */, TestName = "左カラムの幅計算：短い名前なし")]
        [TestCase('s', "hoge", "VAL", CommandOptionKind.HasOptionalValue, ExpectedResult = 16/* "-s, --hoge [VAL]" */, TestName = "左カラムの幅計算：全て指定")]
        public int CalculatesColumnWidthUsedInUsage(char? sname, string lname, string value, CommandOptionKind optionKind)
        {
            var opt = new ExCommandOption(sname, lname, value, "", optionKind);

            return opt.ExNeededLeftColumnLength;
        }

        [TestCase('s', "", "", "", CommandOptionKind.NoValue, '=', 2, ExpectedResult = "  -s  ", TestName = "Usage生成：最小")]
        [TestCase('s', "hoge", "VAL", "hogeオプション", CommandOptionKind.HasOptionalValue, '=', 19,
                  ExpectedResult = "  -s, --hoge[=VAL]     hogeオプション", TestName = "Usage生成：'='でオプション値を指定する")]
        [TestCase('s', "hoge", "VAL", "hogeオプション", CommandOptionKind.HasOptionalValue, ' ', 19,
                  ExpectedResult = "  -s, --hoge [VAL]     hogeオプション", TestName = "Usage生成：' 'でオプション値を指定する")]
        [TestCase('s', "hoge", "VAL", "hogeオプション", CommandOptionKind.NeedsValue, ' ', 14,
                  ExpectedResult = "  -s, --hoge VAL  hogeオプション", TestName = "Usage生成：オプションの必須値を指定する")]
        public string Usageの文字列を生成する(char? sname, string lname, string value, string expression,
                                             CommandOptionKind optionKind, char keyValSeparator, int leftWidth)
        {
            var opt = new ExCommandOption(sname, lname, value, expression, optionKind);

            return opt.ExToString(keyValSeparator, leftWidth);
        }
    }

    class ExCommandOption : CommandOption
    {
        public ExCommandOption(char? sname, string lname, string value, string expression, CommandOptionKind kind)
            : base(sname, lname, value, expression, kind)
        {
        }

        public string ExToString(char keyValSeparator, int leftClmWidth)
        {
            return base.ToString(keyValSeparator, leftClmWidth);
        }

        public int ExNeededLeftColumnLength
        {
            get
            {
                return base.NeededLeftColumnLength;
            }
        }

    }
}
