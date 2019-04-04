using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oika.Libs.CuiCommandParser
{
    /// <summary>
    /// 解析されたコマンドの情報を格納するクラスです。
    /// </summary>
    public class ParsedCommandInfo
    {
        readonly IReadOnlyDictionary<CommandOptionKey, string> optionDic;

        /// <summary>
        /// 大文字小文字の違いが無視されているかどうかを取得します。
        /// </summary>
        public bool IgnoresCase { get; private set; }
        /// <summary>
        /// コマンドに指定されたオプション以外の引数を取得します。
        /// </summary>
        public IReadOnlyList<string> CommandParameters { get; private set; }
        /// <summary>
        /// 指定した長い名称のオプションが指定されたかどうかを取得します。
        /// </summary>
        /// <param name="longName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">longNameに空文字列を指定した際にスローされます。</exception>
        public bool HasOption(string longName)
        {
            if (longName == null) throw new ArgumentNullException("longName");
            if (longName == "") throw new ArgumentException("longNameを空にすることはできません");

            return this.optionDic.Keys.Any(k => k.MatchesWithLongName(longName));
        }
        /// <summary>
        /// 指定した短い名称のオプションが指定されたかどうかを取得します。
        /// </summary>
        /// <param name="shortName"></param>
        /// <returns></returns>
        public bool HasOption(char shortName)
        {
            return this.optionDic.Keys.Any(k => k.MatchesWithShortName(shortName));
        }
        /// <summary>
        /// 指定したオプションが指定されたかどうかを取得します。
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool HasOption(CommandOption option)
        {
            if (option == null) throw new ArgumentNullException("option");

            return this.optionDic.Keys.Any(k => k.Matches(option));
        }

        /// <summary>
        /// 指定した長い名称のオプションとその値が指定されていれば、その値を取得します。
        /// </summary>
        /// <param name="longName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">longNameに空文字列を指定した際にスローされます。</exception>
        public string GetOptionValue(string longName)
        {
            if (longName == null) throw new ArgumentNullException("longName");
            if (longName == "") throw new ArgumentException("longNameを空にすることはできません");

            return this.optionDic.FirstOrDefault(p => p.Key.MatchesWithLongName(longName)).Value;
        }
        /// <summary>
        /// 指定した短い名称のオプションとその値が指定されていれば、その値を取得します。
        /// </summary>
        /// <param name="shortName"></param>
        /// <returns></returns>
        public string GetOptionValue(char shortName)
        {
            return this.optionDic.FirstOrDefault(p => p.Key.MatchesWithShortName(shortName)).Value;
        }
        /// <summary>
        /// 指定したオプションとその値が指定されていれば、その値を取得します。
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetOptionValue(CommandOption option)
        {
            if (option == null) throw new ArgumentNullException("option");

            return this.optionDic.FirstOrDefault(p => p.Key.Matches(option)).Value;
        }

        /// <summary>
        /// プロテクトコンストラクタです。
        /// </summary>
        /// <param name="ignoresCase"></param>
        /// <param name="commandParams"></param>
        /// <param name="optionDic"></param>
        /// <exception cref="ArgumentNullException"></exception>
        internal protected ParsedCommandInfo(bool ignoresCase, IReadOnlyList<string> commandParams, Dictionary<CommandOptionKey, string> optionDic)
        {
            if (commandParams == null) throw new ArgumentNullException("commandParams");
            if (optionDic == null) throw new ArgumentNullException("optionDic");

            this.IgnoresCase = ignoresCase;
            this.CommandParameters = commandParams;
            this.optionDic = optionDic;
        }

    }
}
