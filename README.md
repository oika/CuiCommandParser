# CuiCommandParser

A parser with usage builder for CUI command line parameters and options.

The command format is a kind of Unix style basically.

* Options should be specified with `-x` as short name, of `--xxx` as long name.
  * You can also use other symbols than hyphen by option.
* You can combine options using short name like `-xyz` .
* Option name and that value should be connected with `=` or space.
  * You can use other symbols by option, though it should be a single char.

These are the core classes of this lib.

* CommandParser
* CommandUsageBuilder

## CommandParser

A class to parse parameters passed to the application command.

You should register the option informations by `RegisterOption` at first.

Here is a sample to parse `mv` command.

```cs
// $ mv -S=_bu note.txt child_dir/

static void Main(string[] args)
{
    var parser = new CommandParser();
    var options = new[]
    {
        new CommandOption(null, "backup", "CONTROL",
                          "make a backup of each existing destination file",
                          CommandOptionKind.HasOptionalValue),
        new CommandOption('b', null, null,
                          "like --backup but does not accept an argument",
                          CommandOptionKind.NoValue),
        new CommandOption('f', "force", null,
                          "do not prompt before overwriting",
                          CommandOptionKind.NoValue),
        new CommandOption('S', "suffix", "SUFFIX",
                          "override the usual backup suffix",
                          CommandOptionKind.NeedsValue),
        //...
    };

    foreach (var opt in options)
    {
        parser.RegisterOption(opt);
    }

    var parsed = parser.Parse(args);

    if (parsed == null)
    {
        //In case invalid params.
        return;
    }
    
    //parameters except options
    var parameters = parsed.CommandParameters;   // -> [ "note.txt", "child_dir/" ]

    //check if it has an option. (all the same)
    var forced = parsed.HasOption('f');          // -> false
    // var forced = parsed.HasOption("force");
    // var forced = parsed.HasOption(options[2]);

    //get an option's value. (all the same)
    var suffix = parsed.GetOptionValue('S');     // -> "_bu"
    // var suffix = parsed.GetOptionValue("suffix");
    // var suffix = parsed.GetOptionValue(options[3]);
}
```

## CommandUsageBuilder

A class to generate help message of the command.

After calling `RegisterOption` , you can use `NewUsageBuilder` method of `CommandParser` class so that you can create a builder instance with the option informations.

Here is a sample to build `mv` command usage.

```cs
var builder = parser.NewUsageBuilder("mv");
builder.OptionKeyValueSeparator = '=';
builder.Summary = "Rename SOURCE to DEST, or move SOURCE(s) to DIRECTORY.";

builder.AddUseCase(builder.NewUseCase().AddArg(builder.NewUseCaseArg("OPTION").AsMultiple().AsOptional())
                                       .AddArg(builder.NewUseCaseArg("-T").AsOptional())
                                       .AddArg(builder.NewUseCaseArg("SOURCE"))
                                       .AddArg(builder.NewUseCaseArg("DEST")));

builder.AddUseCase(builder.NewUseCase().AddArg(builder.NewUseCaseArg("OPTION").AsMultiple().AsOptional())
                                       .AddArg(builder.NewUseCaseArg("SOURCE").AsMultiple())
                                       .AddArg(builder.NewUseCaseArg("DIRECTORY")));

builder.AddUseCase(builder.NewUseCase().AddArg(builder.NewUseCaseArg("OPTION").AsMultiple().AsOptional())
                                       .AddArg(builder.NewUseCaseArg("-t").Value("DIRECTORY"))
                                       .AddArg(builder.NewUseCaseArg("SOURCE").AsMultiple()));

Console.WriteLine(builder.ToString());
```

Output:

```
Rename SOURCE to DEST, or move SOURCE(s) to DIRECTORY.

Usage: mv [OPTION]... [-T] SOURCE DEST
   or: mv [OPTION]... SOURCE... DIRECTORY
   or: mv [OPTION]... -t=DIRECTORY SOURCE...

Arguments:
      --backup[=CONTROL]  make a backup of each existing destination file
  -b                      like --backup but does not accept an argument
  -f, --force             do not prompt before overwriting
  -S, --suffix=SUFFIX     override the usual backup suffix
```


