using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Oika.Libs.CuiCommandParser
{
    /// <summary>
    /// コマンドライン実行時のパラメータ解析を行うクラスです。
    /// </summary>
    public class CommandParser
    {
        /// <summary>
        /// 大文字小文字の違いを無視するかどうかを取得します。
        /// </summary>
        public bool IgnoresCase { get; private set; }
        /// <summary>
        /// 長い名前でオプションを指定する際のプレフィックスを取得または設定します。
        /// 既定値は"--"です。
        /// </summary>
        public string LongNameOptionSymbol { get; set; } = "--";
        /// <summary>
        /// 短い名前でオプションを指定する際のプレフィックスを取得または設定します。
        /// 既定値は"-"です。
        /// </summary>
        public string ShortNameOptionSymbol { get; set; } = "-";
        /// <summary>
        /// 不明なオプションを無視するかどうかを取得または設定します。
        /// この値がFalseの場合、解析対象に不明なオプションが含まれていれば例外をスローします。
        /// 既定値はFalseです。
        /// </summary>
        public bool IgnoresUnknownOptions { get; set; } //TODO 例外スローされる？nullが返るだけ？確認
        /// <summary>
        /// オプションのキーと値を区切る文字のリストを取得または設定します。
        /// 既定では'='および半角スペースを含みます。
        /// </summary>
        public IReadOnlyList<char> OptionKeyValueSeparators { get; set; } = new[] { ' ', '=' };

        private readonly Dictionary<CommandOptionKey, CommandOption> optionDic = new Dictionary<CommandOptionKey, CommandOption>();

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="ignoresCase">大文字小文字の違いを無視するかどうかを指定します。</param>
        public CommandParser(bool ignoresCase = false)
        {
            this.IgnoresCase = ignoresCase;
        }

        /// <summary>
        /// オプション情報を追加します。
        /// </summary>
        /// <param name="option"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">キーの重複するオプションが登録されています。</exception>
        public void RegisterOption(CommandOption option)
        {
            if (option == null) throw new ArgumentNullException("option");

            var key = new CommandOptionKey(option.ShortName, option.LongName, this.IgnoresCase);

            //重複チェック
            var registered = this.optionDic.Keys.FirstOrDefault(k => k.Matches(option));
            if (registered != null)
            {
                throw new ArgumentException(registered.ToString(this.ShortNameOptionSymbol, this.LongNameOptionSymbol) + "のオプションが既に登録されています");
            }

            this.optionDic.Add(key, option);
        }

        /// <summary>
        /// Usage文字列を生成するためのインスタンスを生成します。
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public CommandUsageBuilder NewUsageBuilder(string command)
        {
            return new CommandUsageBuilder(command, this.optionDic.Values.ToArray());
        }

        /// <summary>
        /// 実行中アプリケーションのコマンドライン引数を解析します。
        /// 解析不能な場合はnullを返します。
        /// </summary>
        /// <returns></returns>
        public ParsedCommandInfo ParseCommandLineArgs()
        {
            return Parse(Environment.GetCommandLineArgs().Skip(1).ToArray());
        }

        /// <summary>
        /// 指定したコマンドライン引数を解析します。
        /// 解析不能な場合はnullを返します。
        /// </summary>
        /// <param name="args">コマンドライン引数（実行コマンド名の指定部を除く）を指定します。</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ParsedCommandInfo Parse(string[] args)
        {
            if (args == null || args.Any(a => a == null))
            {
                throw new ArgumentNullException("args");
            }

            var optDic = new Dictionary<CommandOptionKey, string>();
            CommandOptionKey currentOptionKey = null;
            var paramList = new List<string>();

            foreach (var arg in args)
            {
                if (arg == "") continue;

                var optKeyRes = tryParseOptionKey(arg);
                if (optKeyRes == null) return null;

                //ひとつまえのオプションを確定
                if (currentOptionKey != null)
                {
                    var preOptKind = this.optionDic[currentOptionKey].OptionKind;
                    Debug.Assert(preOptKind != CommandOptionKind.NoValue);

                    if (optKeyRes.IsKey)
                    {
                        //値が必要なオプションで値が指定されていなかったとき
                        if (preOptKind == CommandOptionKind.NeedsValue) return null;

                        optDic.Add(currentOptionKey, null);
                        currentOptionKey = null;
                    }
                    else
                    {
                        optDic.Add(currentOptionKey, arg);
                        currentOptionKey = null;
                        continue;
                    }
                }

                //キーでないとき
                if (!optKeyRes.IsKey)
                {
                    paramList.Add(arg);
                    continue;
                }

                //キーのとき
                for (int i = 0; i < optKeyRes.Keys.Count; i++)
                {
                    var key = optKeyRes.Keys[i];

                    //重複確認
                    if (optDic.ContainsKey(key))
                    {
                        return null;
                    }

                    var isLast = i == (optKeyRes.Keys.Count - 1);
                    var kind = this.optionDic[key].OptionKind;

                    if (!isLast)
                    {
                        //値が必要なオプションが最後に指定されていない場合
                        if (kind == CommandOptionKind.NeedsValue) return null;

                        optDic.Add(key, null);
                        continue;
                    }

                    if (kind == CommandOptionKind.NoValue)
                    {
                        optDic.Add(key, null);
                        continue;
                    }

                    //スペース以外の区切り文字で値が指定されていれば確定
                    if (optKeyRes.Value != null)
                    {
                        optDic.Add(key, optKeyRes.Value);
                        continue;
                    }

                    //値をとりうるオプションに値が指定されていないとき
                    if (this.OptionKeyValueSeparators.Contains(' '))
                    {
                        //スペースを区切り文字に使用できる場合は確定を保留
                        currentOptionKey = key;
                        continue;
                    }
                    else
                    {
                        if (kind == CommandOptionKind.NeedsValue) return null;
                        optDic.Add(key, null);
                    }
                }
            }

            //未確定のオプションがあれば確定させる
            if (currentOptionKey != null)
            {
                var preOptKind = this.optionDic[currentOptionKey].OptionKind;
                Debug.Assert(preOptKind != CommandOptionKind.NoValue);

                if (preOptKind == CommandOptionKind.NeedsValue) return null;

                optDic.Add(currentOptionKey, null);
            }

            return new ParsedCommandInfo(this.IgnoresCase, paramList, optDic);
        }



        private OptionKeyParseResult tryParseOptionKey(string arg)
        {
            //longname
            if (arg.StartsWith(LongNameOptionSymbol))
            {
                string longName;
                string value;

                var rmSym = arg.Substring(LongNameOptionSymbol.Length);
                var longNamePrms = rmSym.Split(OptionKeyValueSeparators.ToArray());
                if (3 <= longNamePrms.Length)
                {
                    //※"--hoge=huga=piyo" みたいな形は許可しないものとする
                    return null;
                }
                if (longNamePrms.Length == 2)
                {
                    longName = longNamePrms[0];
                    value = longNamePrms[1];
                }
                else
                {
                    longName = longNamePrms.Single();
                    value = null;
                }

                //オプションを探す
                var opt = this.optionDic.FirstOrDefault(p => p.Key.MatchesWithLongName(longName));
                // //登録されてないとき
                if (opt.Key == null)
                {
                    //TODO LongNameOptionSymbolとShortNameOptionSymbolが同じケースへの対応
                    return this.IgnoresUnknownOptions ? new OptionKeyParseResult(true, new CommandOptionKey[0], null) : null;
                }

                //値をとらないオプションで値が指定されているとき
                if (opt.Value.OptionKind == CommandOptionKind.NoValue && value != null) return null;

                return new OptionKeyParseResult(true, new[] { opt.Key }, value);
            }

            //shortname
            if (arg.StartsWith(ShortNameOptionSymbol))
            {
                string shortNames;
                string value;

                var rmSym = arg.Substring(ShortNameOptionSymbol.Length);
                var shortNamePrms = rmSym.Split(OptionKeyValueSeparators.ToArray());
                if (3 <= shortNamePrms.Length)
                {
                    //※"-x=huga=piyo" みたいな形は許可しないものとする
                    return null;
                }
                if (shortNamePrms.Length == 2)
                {
                    shortNames = shortNamePrms[0];
                    value = shortNamePrms[1];
                }
                else
                {
                    shortNames = shortNamePrms.Single();
                    value = null;
                }

                //オプションを探す
                var opts = shortNames.Select(s => this.optionDic.FirstOrDefault(p => p.Key.MatchesWithShortName(s))).ToArray();
                if (opts.Any(o => o.Key == null))
                {
                    if (!this.IgnoresUnknownOptions) return null;
                    opts = opts.Where(o => o.Key != null).ToArray();
                }

                //値をとらないオプションで値が指定されているとき
                if (opts.Any() && opts.Last().Value.OptionKind == CommandOptionKind.NoValue && value != null) return null;

                return new OptionKeyParseResult(true, opts.Select(o => o.Key).ToArray(), value);
            }

            return OptionKeyParseResult.Blank;
        }

        /// <summary>
        /// オプションのキーとなる文字列の解析結果を格納するクラス
        /// </summary>
        private class OptionKeyParseResult
        {
            public static readonly OptionKeyParseResult Blank = new OptionKeyParseResult(false, null, null);

            public IReadOnlyList<CommandOptionKey> Keys { get; private set; }
            public string Value { get; private set; }
            public bool IsKey { get; private set; }

            public OptionKeyParseResult(bool isKey, CommandOptionKey[] keys, string value)
            {
                this.IsKey = isKey;
                this.Keys = keys ?? new CommandOptionKey[0];
                this.Value = value;
            }
        }

    }

    /// <summary>
    /// コマンドのオプションを識別するための情報を格納するクラスです。
    /// </summary>
    public sealed class CommandOptionKey
    {
        readonly char? shortName;
        readonly string longName;
        readonly bool ignoresCase;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="shortName"></param>
        /// <param name="longName"></param>
        /// <param name="ignoresCase"></param>
        /// <exception cref="ArgumentException"></exception>
        public CommandOptionKey(char? shortName, string longName, bool ignoresCase)
        {
            if (shortName == null && string.IsNullOrEmpty(longName))
            {
                throw new ArgumentException("You should set at least either shortName or longName.");
            }
            this.ignoresCase = ignoresCase;
            if (shortName.HasValue) this.shortName = asKey(shortName.Value);
            if (!string.IsNullOrEmpty(longName)) this.longName = asKey(longName);
        }

        /// <summary>
        /// 指定した短い名称がこのインスタンスと一致するかどうかを取得します。
        /// </summary>
        /// <param name="shortName"></param>
        /// <returns></returns>
        public bool MatchesWithShortName(char shortName)
        {
            if (this.shortName == null) return false;

            return this.shortName.Value == asKey(shortName);
        }

        /// <summary>
        /// 指定した長い名称がこのインスタンスと一致するかどうかを取得します。
        /// </summary>
        /// <param name="longName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">longNameに空文字を指定した際にスローされます。</exception>
        public bool MatchesWithLongName(string longName)
        {
            if (longName == null) throw new ArgumentNullException("longName");
            if (longName == "") throw new ArgumentException("longNameに空の文字列を指定することはできません");

            if (string.IsNullOrEmpty(this.longName)) return false;

            return this.longName == asKey(longName);
        }

        /// <summary>
        /// 指定したオプションのキーがこのインスタンスと一致するかどうかを取得します。
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Matches(CommandOption option)
        {
            if (option == null) throw new ArgumentNullException("option");

            if (option.ShortName.HasValue && MatchesWithShortName(option.ShortName.Value)) return true;
            if (!string.IsNullOrEmpty(option.LongName) && MatchesWithLongName(option.LongName)) return true;

            return false;
        }

        private char asKey(char shortName)
        {
            return this.ignoresCase ? char.ToLower(shortName) : shortName;
        }
        private string asKey(string longName)
        {
            return this.ignoresCase ? longName.ToLower() : longName;
        }

        /// <summary>
        /// 指定したオブジェクトがこのオブジェクトと等価かどうかを取得します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as CommandOptionKey;
            if (other == null) return false;

            return this.ignoresCase == other.ignoresCase
                && this.shortName == other.shortName
                && this.longName == other.longName;
        }
        /// <summary>
        /// ハッシュコードを取得します。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (this.shortName ?? ' ').GetHashCode()
                ^ (this.longName ?? "").GetHashCode();
        }
        /// <summary>
        /// このオブジェクトの文字列表現を返します。
        /// </summary>
        /// <param name="shortNameSymbol"></param>
        /// <param name="longNameSymbol"></param>
        /// <returns></returns>
        public string ToString(string shortNameSymbol, string longNameSymbol)
        {
            if (shortName.HasValue)
            {
                if (!string.IsNullOrEmpty(longName))
                {
                    return string.Format("{0}{1},{2}{3}", shortNameSymbol, shortName.Value, longNameSymbol, longName);
                }
                return shortNameSymbol + shortName.Value;
            }
            return longNameSymbol + longName;
        }
    }
}
