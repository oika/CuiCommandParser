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
    class CommandUsageBuilderTest
    {

        private static IEnumerable<TestCaseData> ToStringSource
        {
            get
            {
                yield return new TestCaseData("", "", new CommandOption[0], new CommandUseCase[0])
                                    .Returns("")
                                    .SetName("最小");
                yield return new TestCaseData("cmd", "コマンド概要です", new CommandOption[0], new CommandUseCase[0])
                                    .Returns("コマンド概要です" + Environment.NewLine + Environment.NewLine)
                                    .SetName("コマンド名と概要のみ");

                yield return new TestCaseData("cmd", "概要desu",
                                new[]
                                {
                                    new CommandOption('s', "saas", "SAASVAL", "SAAS値を指定", CommandOptionKind.NeedsValue),
                                    new CommandOption('x', "xxxx", null, "表示しない", CommandOptionKind.NoValue) { ShowsInUsage = false },
                                    new CommandOption(null, "very-long-option-name", null, "ほげ", CommandOptionKind.NoValue)
                                },
                                new[]
                                {
                                    new ExUseCase("cmd", ' ').WithSummary("一般的な使い方").AddArg("OPTIONS", true, true),
                                    new ExUseCase("cmd", ' ').WithSummary("ヘルプ").AddArg("--help")
                                })
                                .Returns(new StringBuilder("概要desu")
                                               .AppendLine()
                                               .AppendLine()
                                               .AppendLine("Usage: cmd [OPTIONS]...  一般的な使い方")
                                               .AppendLine("   or: cmd --help        ヘルプ")
                                               .AppendLine()
                                               .AppendLine("Arguments:")
                                               .AppendLine("  -s, --saas SAASVAL           SAAS値を指定")
                                               .AppendLine("      --very-long-option-name  ほげ")
                                               .ToString())
                                .SetName("フル指定");
            }
        }

        [TestCaseSource("ToStringSource")]
        public string BuildsUsageString(string command, string summary, CommandOption[] options, CommandUseCase[] useCases)
        {
            var builder = new CommandUsageBuilder(command, options);
            builder.Summary = summary;

            foreach (var usecase in useCases)
            {
                builder.AddUseCase(usecase);
            }

            return builder.ToString();
        }

    }
}
