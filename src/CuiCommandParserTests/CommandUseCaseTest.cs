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
    class CommandUseCaseTest
    {
        [Test]
        public void BuildsUseCaseString()
        {
            var usecase = new ExUseCase("CMD", ':');
            usecase.WithSummary("概要です ..")
                   .AddArg(usecase.NewArg("Arg1").AsMultiple())
                   .AddArg(usecase.NewArg("Arg2").AsOptional())
                   .AddArg(usecase.NewArg("arg3").Value("A3Val"))
                   .AddArg("Arg4", true, true, "a4_val", true);

            Assert.That(usecase.ExToString(usecase.ExNeededLeftLen),
                        Is.EqualTo("CMD Arg1... [Arg2] arg3:A3Val [Arg4[:a4_val]]...  概要です .."));
        }
    }

    class ExUseCase : CommandUseCase
    {
        public ExUseCase(string command, char optionKeyValueSeparator)
            : base(command, optionKeyValueSeparator)
        {
        }

        public int ExNeededLeftLen
        {
            get
            {
                return base.NeededLeftLength;
            }
        }

        public string ExToString(int leftLen)
        {
            return base.ToString(leftLen);
        }
    }

}
