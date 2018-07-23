module SystemUtils
open SystemTypes
open System

/// Are we running on linux?
let isLinuxFileSystem =
    let os = System.Environment.OSVersion
    let platformId = os.Platform
    match platformId with
        | PlatformID.Win32NT | PlatformID.Win32S | PlatformID.Win32Windows | PlatformID.WinCE | PlatformID.Xbox -> false
        | PlatformID.MacOSX | PlatformID.Unix -> true
        | _ ->false
type MajorTypeOfOS = Windows|Linux 
let MajorOSType = if isLinuxFileSystem then Linux else Windows

let inline tryParseGeneric<
    'a when ^a : (static member TryParse : string * ^a byref -> bool)
    and  ^a : (new:unit->'a)
    > text : 'a option =
    let n =  ref (new 'a())
    let ret = (^a : (static member TryParse: string * ^a byref -> bool) (text,&n.contents))
    if ret then Some (n.Value) else None

let createNewBaseOptions programName programTagLine programHelpText verbose =
    {
        programName = programName
        programTagLine = programTagLine
        programHelpText=programHelpText
        verbose = verbose
        interimProgress = {items=new System.Collections.Generic.Dictionary<string, System.Text.StringBuilder>()}
    }


let createNewConfigEntry commandlineSymbol commandlineParameterName parameterHelpText initialValue =
    {
        commandLineParameterSymbol=commandlineSymbol
        commandLineParameterName=commandlineParameterName
        parameterHelpText=parameterHelpText
        parameterValue=initialValue
    }

let defaultVerbosity  =
    {
        commandLineParameterSymbol="V"
        commandLineParameterName="Verbosity"
        parameterHelpText=[|"/V:[0-9]           -> Amount of trace info to report. 0=none, 5=normal, 9=max."|]           
        parameterValue=Verbosity.Minimum
    }

let programHelp = [|"This is an example program for talking about option types."|]
let defaultBaseOptions = createNewBaseOptions "optionExample" "Does some thing with some stuff" programHelp defaultVerbosity
let defaultFullFileName = System.IO.Path.Combine([|System.AppDomain.CurrentDomain.BaseDirectory; "OptionEssayExampleFile.txt"|])
let defaultInputFile = 
    createNewConfigEntry "I" "Input File (Optional)" 
        [|"/I:<filename> -> full name of the file to use for input."|]
        (defaultFullFileName, Some(System.IO.FileInfo(defaultFullFileName)))
let defaultOutputFormat =
    createNewConfigEntry "F" "Output Format (Optional"
        [|"/F:<TEXT|HTML|WEBPAGE> -> type of output desired"; "Defaults to TEXT"|]
        OutputFormat.Text
let loadConfigFromCommandLine (args:string []):OptionExampleProgramConfig =
    if args.Length>0 && (args.[0]="?"||args.[0]="/?"||args.[0]="-?"||args.[0]="--?"||args.[0]="help"||args.[0]="/help"||args.[0]="-help"||args.[0]="--help") then raise (UserNeedsHelp args.[0]) else
    let newVerbosity =ConfigEntry<_>.populateValueFromCommandLine(defaultVerbosity, args)
    let newConfigBase = {defaultBaseOptions with verbose=newVerbosity}
    let newVerbosity =ConfigEntry<_>.populateValueFromCommandLine(defaultVerbosity, args)
    let newInputFile = ConfigEntry<_>.populateValueFromCommandLine(defaultInputFile, args)
    let newOutputFormat = ConfigEntry<_>.populateValueFromCommandLine(defaultOutputFormat, args)
    {configBase = newConfigBase; inputFile=newInputFile; outputFormat=newOutputFormat.parameterValue}

let directoryExists (dir:ConfigEntry<DirectoryParm>) = (snd (dir.parameterValue)).IsSome
let fileExists (dir:ConfigEntry<FileParm>) = (snd (dir.parameterValue)).IsSome


/// Prints out the options for the command before it runs. Detail level is based on verbosity setting
let commandLinePrintWhileEnter (opts:ConfigBase) fnPrintMe =
            // Entering program command line report
        let nowString = string System.DateTime.Now
        match opts.verbose.parameterValue with
            | Verbosity.Silent ->
                ()
            | Verbosity.BatchMinimum ->
                printfn "%s" opts.programName
            | Verbosity.Minimum ->
                ()
                //printfn "Begin %s. %s" opts.programName opts.programTagLine
            | Verbosity.BatchNormal ->
                printfn "%s. %s" opts.programName opts.programTagLine
                printfn "Begin: %s" (nowString)
            | Verbosity.Normal ->
                printfn "%s. %s" opts.programName opts.programTagLine
                printfn "Begin: %s" (nowString)
                printfn "Verbosity: Normal (5)" 
            | Verbosity.BatchVerbose ->
                printfn "%s. %s" opts.programName opts.programTagLine
                printfn "Verbosity: BatchVerbose (6)"
                printfn "Begin: %s" (nowString)
            | Verbosity.Verbose ->
                printfn "%s. %s" opts.programName opts.programTagLine
                printfn "Verbosity: Verbose (7)"
                printfn "Begin: %s" (nowString)
                fnPrintMe()
                printfn ""
            |_ ->
                printfn "%s. %s" opts.programName opts.programTagLine
                printfn "Begin: %s" (nowString)
                fnPrintMe()

/// Exiting program command line report. Detail level is based on verbosity setting
let commandLinePrintWhileExit (baseOptions:ConfigBase) =
    let nowString = string System.DateTime.Now
    match baseOptions.verbose.parameterValue with
        | Verbosity.Silent ->
            ()
        | Verbosity.BatchMinimum ->
            ()
        | Verbosity.Minimum ->
            ()
            //printfn "End %s" baseOptions.programName
        | Verbosity.BatchNormal ->
            printfn "End:   %s" (nowString)
        | Verbosity.Normal ->
            printfn "End:   %s" (nowString)
        | Verbosity.BatchVerbose ->
            printfn "End:   %s" (nowString)
        | Verbosity.Verbose ->
            printfn "End:   %s" (nowString)
        |_ ->
            ()


