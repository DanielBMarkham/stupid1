module SystemUtils
open SystemTypes
open System

/// found on web
let rec readStdIn lines= 
    match System.Console.ReadLine() with
    | null -> List.rev lines
    | "" -> readStdIn lines
    | s -> readStdIn (s::lines)

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

/// Takes a name=value collection and sums by name
let groupAndSumKV (optionLines:seq<System.Collections.Generic.KeyValuePair<string,int>>) =
    optionLines
    |> Seq.groupBy(fun x->x.Key) 
    |> Seq.sortBy fst
    |> Seq.map(fun x->
        let sumOfGroupedData=snd x |> Seq.sumBy(fun x->x.Value)
        (fst x, sumOfGroupedData)
        )
/// Takes a title, css file, javascript file, and contents
/// Makes a basic html return string to send to the Apache controller
let wrapFragmentIntoAnHtmlPageWebServerReturnString 
    (title:string) 
    (cssFile:string) 
    (javascriptFile:string) 
    (contents:string) =
    let cssEntry =
        if cssFile.Length>0
            then OSNewLine + "<link rel='stylesheet' type='text/css' media='all' href='"
                + cssFile + "' />" 
            else ""
    let jsEntry =
        if javascriptFile.Length>0
            then OSNewLine + "	<script src='"
                + javascriptFile + "'></script>"
            else ""
    let ret  =  "Content-Type: text/html\r\n\r\n"
                + "<!DOCTYPE html>" 
                + OSNewLine + "<html>" 
                + OSNewLine + "<head>"
                + OSNewLine + "<title>"
                + title + "</title>"
                + cssEntry
                + jsEntry
                + OSNewLine + "</head>\n"
                + OSNewLine + "<body>"
                + OSNewLine + contents
                + OSNewLine + "</body></html>"
    ret




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
        [|"-I:<filename> -> full name of the file to use for input."|]
        (defaultFullFileName, Some(System.IO.FileInfo(defaultFullFileName)))
let defaultOutputFormat =
    createNewConfigEntry "OF" "Output Format (Optional"
        [|"-OF:<TEXT|HTML|WEBPAGE> -> type of output desired"; "Defaults to TEXT"|]
        OutputFormat.Text
let defaultInputFormat =
    createNewConfigEntry "IF" "Input Format (Optional"
        [|"-IF:<STREAM|FILE|CGI> -> type of stdin input being sent"; "Defaults to STREAM"|]
        InputFormat.Stream
let loadConfigFromCommandLine (args:string []):OptionExampleProgramConfig =
    if args.Length>0 && (args.[0]="?"||args.[0]="/?"||args.[0]="-?"||args.[0]="--?"||args.[0]="help"||args.[0]="/help"||args.[0]="-help"||args.[0]="--help") then raise (UserNeedsHelp args.[0]) else
    let inputFileParmOnLine = args |> Array.exists(fun x->x.IndexOf("-I:")<>(-1))
    let newVerbosity =ConfigEntry<_>.populateValueFromCommandLine(defaultVerbosity, args)
    let newConfigBase = {defaultBaseOptions with verbose=newVerbosity}
    let newVerbosity =ConfigEntry<_>.populateValueFromCommandLine(defaultVerbosity, args)
    let newInputFile = ConfigEntry<_>.populateValueFromCommandLine(defaultInputFile, args)
    let newInputFormat = ConfigEntry<_>.populateValueFromCommandLine(defaultInputFormat, args)
    let newOutputFormat = ConfigEntry<_>.populateValueFromCommandLine(defaultOutputFormat, args)
    if inputFileParmOnLine
        then
            {configBase = newConfigBase; inputFile=newInputFile; inputFormat=File; outputFormat=newOutputFormat.parameterValue}
        else
            {configBase = newConfigBase; inputFile=newInputFile; inputFormat=newInputFormat.parameterValue; outputFormat=newOutputFormat.parameterValue}

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

let processCGIStream incomingStream =
    let findVariables = incomingStream |> List.map(fun x->
        let goodLine = (lineContainsADelimiter '=' x) &&  (x.Split([|'='|]).Length>1)
        let multipleVariablesOnLine = lineContainsADelimiter '&' x
        if goodLine
            then
                let lineSplitByVariable = 
                    if multipleVariablesOnLine
                        then x.Split([|'&'|])
                        else [|x|]
                let variableValueMatch = 
                    lineSplitByVariable |> Array.map(fun x->
                        let lineSplitByEquals=x.Split([|'='|])
                        if lineSplitByEquals.Length<2
                            then None
                            else
                            let newKey=lineSplitByEquals.[0]
                            let codedVal=lineSplitByEquals.[1]
                            let newVal=
                                try
                                    let decodedVal = HttpUtility.UrlDecode(codedVal)
                                    decodedVal
                                with
                                    | :? System.Exception as ex ->
                                        //System.Console.WriteLine ("DOH " + codedVal)
                                        //System.Console.WriteLine ("EX " + ex.Message)
                                        //System.Console.WriteLine()
                                        //if ex.InnerException<>null
                                        //    then System.Console.WriteLine ("EXInner " + ex.Message) else ()
                                        "BAD VARIABLE VALUE"
                            Some (System.Collections.Generic.KeyValuePair<string,string>(newKey,newVal))
                        )
                    |>Array.choose id
                Some variableValueMatch
            else None
        )
    findVariables |> List.choose id |> Seq.concat

let processCGISteamIntoVariables() =
    let incomingStream = readStdIn []
    processCGIStream incomingStream