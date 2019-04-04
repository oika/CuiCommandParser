using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Oika.Libs.CuiCommandParser
{
    /// <summary>
    /// コマンドのオプション情報を格納するクラスです。
    /// </summary>
    public class CommandOption
    {
        /// <summary>
        /// 短いオプション識別子を取得します。
        /// </summary>
        public char? ShortName { get; private set; }
        /// <summary>
        /// 長いオプション識別子を取得します。
        /// </summary>
        public string LongName { get; private set; }
        /// <summary>
        /// 値をとるオプションであれば、その値の名称を取得します。
        /// </summary>
        public string ValueName { get; private set; }
        /// <summary>
        /// このオプションの説明文を取得します。
        /// </summary>
        public string Expression { get; private set; }
        /// <summary>
        /// オプション種別を取得します。
        /// </summary>
        public CommandOptionKind OptionKind { get; private set; }
        /// <summary>
        /// このオプション情報をUsageに表示するかどうかを取得または設定します。
        /// 既定値はTrueです。
        /// </summary>
        public bool ShowsInUsage { get; set; } = true;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="shortName">短いオプション識別子を指定します。</param>
        /// <param name="longName">長いオプション識別子を指定します。</param>
        /// <param name="valueName">値をとるオプションであれば、その値の名称を指定します。</param>
        /// <param name="expression">このオプションの説明文を指定します。</param>
        /// <param name="kind">オプション種別を指定します。</param>
        /// <exception cref="ArgumentOutOfRangeException">kindに未定義の値が指定された場合にスローされます。</exception>
        /// <exception cref="ArgumentException">shortNameとlongNameが両方とも指定されていない場合にスローされます。</exception>
        public CommandOption(char? shortName, string longName, string valueName, string expression, CommandOptionKind kind)
        {
            if (!Enum.IsDefined(typeof(CommandOptionKind), kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }

            if (shortName == null && string.IsNullOrEmpty(longName))
            {
                throw new ArgumentException("You should set at least either shortName or longName.");
            }

            this.ShortName = shortName;
            this.LongName = longName;
            this.ValueName = valueName;
            this.Expression = expression ?? "";
            this.OptionKind = kind;
        }

        /// <summary>
        /// Usage出力時に必要となる、コマンド部の半角文字数を取得します。
        /// </summary>
        internal protected int NeededLeftColumnLength
        {
            get
            {
                var rtn = "-x".Length;

                if (!string.IsNullOrEmpty(this.LongName))
                {
                    rtn += ", ".Length;
                    rtn += ("--" + this.LongName).Length;
                }

                if (!string.IsNullOrEmpty(this.ValueName))
                {
                    rtn += (' ' + this.ValueName).Length;
                    if (this.OptionKind == CommandOptionKind.HasOptionalValue)
                    {
                        rtn += ("[]").Length;
                    }
                }

                return rtn;
            }
        }

        /// <summary>
        /// Usageの出力使用される文字列表現を取得します。
        /// </summary>
        /// <param name="keyValSeparator"></param>
        /// <param name="leftColumnWidth"></param>
        /// <returns></returns>
        internal protected string ToString(char keyValSeparator, int leftColumnWidth)
        {
            var sb = new StringBuilder();

            if (ShortName.HasValue)
            {
                sb.Append("-").Append(ShortName.Value);
                if (!string.IsNullOrEmpty(this.LongName))
                {
                    sb.Append(", ");
                }
            }
            else
            {
                Debug.Assert(!string.IsNullOrEmpty(LongName));
                sb.Append("    "); //"-x, ".Length
            }
            if (!string.IsNullOrEmpty(LongName))
            {
                sb.Append("--").Append(LongName);
            }
            if (!string.IsNullOrEmpty(ValueName))
            {
                if (char.IsWhiteSpace(keyValSeparator))
                {
                    // "KEY [VAL]" となるように
                    sb.Append(keyValSeparator);
                    if (OptionKind == CommandOptionKind.HasOptionalValue) sb.Append("[");
                }
                else
                {
                    // "KEY[=VAL]" となるように
                    if (OptionKind == CommandOptionKind.HasOptionalValue) sb.Append("[");
                    sb.Append(keyValSeparator);
                }
                sb.Append(ValueName);
                if (OptionKind == CommandOptionKind.HasOptionalValue) sb.Append("]");
            }

            var restLeft = leftColumnWidth - sb.Length;
            if (0 < restLeft) sb.Append(Enumerable.Repeat(' ', restLeft).ToArray());

            return string.Format("  {0}  {1}", sb.ToString(), Expression);
        }
    }

    /// <summary>
    /// コマンドオプション種別を表す列挙型です。
    /// </summary>
    public enum CommandOptionKind
    {
        /// <summary>
        /// 値を取らないオプションを表します。
        /// </summary>
        NoValue,
        /// <summary>
        /// 値を必要とするオプションを表します。
        /// </summary>
        NeedsValue,
        /// <summary>
        /// 任意で値を指定できるオプションを表します。
        /// </summary>
        HasOptionalValue,
    }
}
