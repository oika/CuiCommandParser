using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oika.Libs.CuiCommandParser
{
    /// <summary>
    /// コマンドのUsage表示文字列を生成するクラスです。
    /// このクラスのインスタンスは<see cref="CommandParser"/>の<see cref="CommandParser.NewUsageBuilder(string)"/>から生成することができます。
    /// </summary>
    public class CommandUsageBuilder
    {
        readonly string command;
        readonly IReadOnlyList<CommandOption> options;
        readonly List<CommandUseCase> useCaseList = new List<CommandUseCase>();

        /// <summary>
        /// コマンドの概要を取得または設定します。
        /// </summary>
        public string Summary { get; set; }
        /// <summary>
        /// オプションのキーと値を区切る文字を取得または設定します。
        /// 既定値は半角スペースです。
        /// </summary>
        public char OptionKeyValueSeparator { get; set; } = ' ';

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="command">コマンド文字列を指定します。</param>
        /// <param name="options">オプションリストを指定します。</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandUsageBuilder(string command, params CommandOption[] options)
        {
            if (options == null || options.Any(o => o == null))
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.command = command ?? "";
            this.options = options.Where(o => o.ShowsInUsage).ToArray();
        }

        /// <summary>
        /// コマンドのユースケースを追加します。
        /// </summary>
        /// <param name="useCase">ユースケースを指定します。
        /// ユースケースインスタンスは<see cref="NewUseCase"/>から生成することができます。</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddUseCase(CommandUseCase useCase)
        {
            if (useCase == null) throw new ArgumentNullException(nameof(useCase));

            this.useCaseList.Add(useCase);
        }

        /// <summary>
        /// コマンドのユースケースインスタンスを生成します。
        /// このインスタンスは<see cref="CommandUseCase.NewArg(string)"/>から生成することもできます。
        /// </summary>
        /// <returns></returns>
        public CommandUseCase NewUseCase()
        {
            return new CommandUseCase(this.command, this.OptionKeyValueSeparator);
        }

        /// <summary>
        /// コマンドユースケースのパラメータインスタンスを生成します。
        /// このインスタンスは
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CommandUseCase.Argument NewUseCaseArg(string name)
        {
            return new CommandUseCase.Argument(name, this.OptionKeyValueSeparator);
        }

        /// <summary>
        /// このインスタンスの文字列表現を返します。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(this.Summary))
            {
                sb.AppendLine(this.Summary);
                sb.AppendLine();
            }

            if (this.useCaseList.Any())
            {
                var isFirst = true;
                var leftWidth = this.useCaseList.Max(p => p.NeededLeftLength);

                foreach (var usecase in this.useCaseList)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        sb.AppendLine("Usage: " + usecase.ToString(leftWidth));
                    }
                    else
                    {
                        sb.AppendLine("   or: " + usecase.ToString(leftWidth));
                    }
                }
                sb.AppendLine();
            }

            if (this.options.Any())
            {
                sb.AppendLine("Arguments:");

                var leftWidth = this.options.Max(a => a.NeededLeftColumnLength);

                foreach (var opt in this.options)
                {
                    sb.AppendLine(opt.ToString(this.OptionKeyValueSeparator, leftWidth));
                }
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// コマンドのユースケースを表すクラスです。
    /// </summary>
    public class CommandUseCase
    {
        readonly char optionKeyValueSeparator;

        readonly List<string> argList = new List<string>();

        string summary;

        /// <summary>
        /// プロテクトコンストラクタです。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="optionKeyValueSeparator"></param>
        internal protected CommandUseCase(string command, char optionKeyValueSeparator)
        {
            if (!string.IsNullOrEmpty(command)) this.argList.Add(command);

            this.optionKeyValueSeparator = optionKeyValueSeparator;
        }

        /// <summary>
        /// ユースケースのパラメータを追加し、自身のインスタンスを返します。
        /// </summary>
        /// <param name="arg">パラメータインスタンスを指定します。
        /// このインスタンスは<see cref="NewArg"/>から生成することができます。</param>
        /// <returns></returns>
        public CommandUseCase AddArg(Argument arg)
        {
            if (arg == null) throw new ArgumentNullException("arg");

            argList.Add(arg.ToString());
            return this;
        }

        /// <summary>
        /// ユースケースのパラメータインスタンスを生成します。
        /// </summary>
        /// <param name="name">パラメータ名称を指定します。</param>
        /// <returns></returns>
        public Argument NewArg(string name)
        {
            return new Argument(name, optionKeyValueSeparator);
        }

        /// <summary>
        /// ユースケースのパラメータ情報を追加し、自身のインスタンスを返します。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isOptional"></param>
        /// <param name="isMultiple"></param>
        /// <param name="valueName"></param>
        /// <param name="isValueOptional"></param>
        /// <returns></returns>
        public CommandUseCase AddArg(string name, bool isOptional = false, bool isMultiple = false, string valueName = null, bool isValueOptional = false)
        {
            var arg = NewArg(name);
            if (isOptional) arg.AsOptional();
            if (isMultiple) arg.AsMultiple();
            if (!string.IsNullOrEmpty(valueName)) arg.Value(valueName, isValueOptional);

            return AddArg(arg);
        }

        /// <summary>
        /// ユースケースの概要を設定し、自身のインスタンスを返します。
        /// </summary>
        /// <param name="summary"></param>
        /// <returns></returns>
        public CommandUseCase WithSummary(string summary)
        {
            this.summary = summary;
            return this;
        }

        /// <summary>
        /// このユースケースを文字列表現で出力した際に、概要以外の情報で
        /// 必要となる半角文字数を取得します。
        /// </summary>
        internal protected int NeededLeftLength
        {
            get
            {
                return string.Join(" ", argList).Length;
            }
        }
        /// <summary>
        /// このユースケースの文字列表現を返します。
        /// </summary>
        /// <param name="leftWidth"></param>
        /// <returns></returns>
        internal protected string ToString(int leftWidth)
        {
            var sb = new StringBuilder();
            sb.Append(string.Join(" ", argList));

            var restLeft = leftWidth - sb.Length;
            if (0 < restLeft)
            {
                sb.Append(Enumerable.Repeat(' ', restLeft).ToArray());
            }

            return sb.ToString() + "  " + (this.summary ?? "");
        }

        /// <summary>
        /// ユースケースのパラメータを表すクラスです。
        /// このクラスのインスタンスは<see cref="CommandUseCase.NewArg(string)"/>または
        /// <see cref="CommandUsageBuilder.NewUseCaseArg(string)"/>から生成することができます。
        /// </summary>
        public class Argument
        {
            readonly string name;
            readonly char optionKeyValueSeparator;
            bool isOptional;
            bool isMultiple;
            string valueWithSeparator;

            /// <summary>
            /// プロテクトコンストラクタです。
            /// </summary>
            /// <param name="name"></param>
            /// <param name="optionKeyValueSeparator"></param>
            internal protected Argument(string name, char optionKeyValueSeparator)
            {
                if (name == null) throw new ArgumentNullException("name");
                this.name = name;
                this.optionKeyValueSeparator = optionKeyValueSeparator;
            }

            /// <summary>
            /// このパラメータにオプショナル表現を追加し、自身のインスタンスを返します。
            /// </summary>
            /// <returns></returns>
            public Argument AsOptional()
            {
                this.isOptional = true;
                return this;
            }
            /// <summary>
            /// このパラメータに複数指定可能の表現を追加し、自身のインスタンスを返します。
            /// </summary>
            /// <returns></returns>
            public Argument AsMultiple()
            {
                this.isMultiple = true;
                return this;
            }
            /// <summary>
            /// このパラメータに値の情報を追加し、自身のインスタンスを返します。
            /// </summary>
            /// <param name="valueName"></param>
            /// <param name="isOptional"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            public Argument Value(string valueName, bool isOptional = false)
            {
                if (valueName == null) throw new ArgumentNullException("valueName");

                var v = this.optionKeyValueSeparator + valueName;
                this.valueWithSeparator = isOptional ? ("[" + v + "]") : v;
                return this;
            }

            /// <summary>
            /// このユースケースの文字列表現を返します。
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var sb = new StringBuilder(name);

                if (valueWithSeparator != null)
                {
                    sb.Append(valueWithSeparator);
                }

                if (isOptional)
                {
                    sb.Insert(0, "[").Append("]");
                }

                //...は[]の外側としておく
                if (isMultiple)
                {
                    sb.Append("...");
                }

                return sb.ToString();
            }
        }
    }
}
