﻿module SystemTypes

open Newtonsoft.Json

let OSNewLine=System.Environment.NewLine
let inline tryParseGeneric<
                            'a when ^a : (static member TryParse : string * ^a byref -> bool)
                            and  ^a : (new:unit->'a)
                            > text : 'a option =
    let n =  ref (new 'a())
    let ret = (^a : (static member TryParse: string * ^a byref -> bool) (text,&n.contents))
    if ret then Some (n.Value) else None

let lineContainsADelimiter (delimiter:char) (line:string) = 
    if line=null then false else line.Contains(string delimiter)
let lineOnlyHasTwoPieces (delimiter:char) (line:string) =
    if line=null then false else line.Split([|delimiter|]).Length=2
let splitLineIfPossibleIntoTwoPieces (delimiter:char) (line:string)  =
    if lineContainsADelimiter delimiter line && lineOnlyHasTwoPieces delimiter line
        then
            let split=line.Split([|delimiter|])
            Some (split.[0],split.[1])
        else None
let convertLinesIfPossibleToKVPair (stringSequence:string seq) =
    stringSequence
        |> Seq.map(fun x->
            splitLineIfPossibleIntoTwoPieces '=' x
            )
        |> Seq.choose id 
        |> Seq.map(fun x->
            new System.Collections.Generic.KeyValuePair<string,string>(fst x, snd x)
            )
type System.IO.File with    
    static member ReadAllNameValueLines s =
        let textLines = 
            try System.IO.File.ReadAllLines s |> Array.toSeq
            with |_ ->Seq.empty
        convertLinesIfPossibleToKVPair textLines

/// Command-line parameters for this particular (OptionExample) program

//
// Common Exception Types
//
exception UserNeedsHelp of string
exception ExpectedResponseFail of string
type Verbosity =
    | Silent            = 1
    | BatchMinimum      = 2
    | Minimum           = 3
    | BatchNormal       = 4
    | Normal            = 5
    | BatchVerbose      = 6
    | Verbose           = 7
//
// Program Command Line Config Settings
//
let getMatchingParameters (args:string []) (symbol:string) = 
    args |> Array.filter(fun x->
                let argParms = x.Split([|':'|],2)
                let parmName = (argParms.[0]).Substring(1).ToUpper()
                if argParms.Length > 0 then parmName=symbol.ToUpper() else false
                )
let getValuePartOfMostRelevantCommandLineMatch (args:string []) (symbol:string) =
    let matchingParms = getMatchingParameters args symbol
    if matchingParms.Length > 0
        then
            // if there are multiple entries, last one overrides the rest
            let commandLineParm = matchingParms.[matchingParms.Length-1]
            let parmSections=commandLineParm.Split([|':'|], 2)
            if parmSections.Length<2 then Some "" else Some parmSections.[1]
        else
            None
type FileParm = string*System.IO.FileInfo option
type DirectoryParm = string*System.IO.DirectoryInfo option
type SortOrder = Ascending | Descending
                    static member ToList()=[Ascending;Descending]
                    override this.ToString()=
                        match this with
                            | Ascending->"Ascending"
                            | Descending->"Descending"
                    static member TryParse(stringToParse:string) =
                        match stringToParse with
                            |"a"|"asc"|"ascending"|"A"|"ASC"|"Ascending"|"Asc"|"ASCENDING"->true,SortOrder.Ascending
                            |"d"|"desc"|"descending"|"D"|"DESC"|"Descending"|"Desc"|"DESCENDING"->true,SortOrder.Descending
                            |_->false, SortOrder.Ascending
                    static member Parse(stringToParse:string) =
                        match stringToParse with
                            |"a"|"asc"|"ascending"|"A"|"ASC"|"Ascending"|"Asc"|"ASCENDING"->SortOrder.Ascending
                            |"d"|"desc"|"descending"|"D"|"DESC"|"Descending"|"Desc"|"DESCENDING"->SortOrder.Descending
                            |_->raise(new System.ArgumentOutOfRangeException("Sort Order","The string value provided for Sort Order is not in the Sort Order enum"))

type OutputFormat = Text | Html | WebPage |JSON with
        static member ToList() =
            [Text;Html;WebPage;JSON]
        override self.ToString() =
            match self with
            | Text->"Text"
            | Html->"Html"
            | WebPage->"WebPage"
            | JSON->"Json"
        static member TryParse(stringToParse:string) =
            match stringToParse.ToUpper() with
                |"TEXT"|"T"->true,OutputFormat.Text
                |"HTML"|"H"->true,OutputFormat.Html
                |"WEBPAGE"|"W"->true,OutputFormat.WebPage
                |"JSON"|"J"->true,OutputFormat.JSON
                |_->(false, OutputFormat.Text)
        static member Parse(stringToParse:string) =
            match stringToParse.ToUpper() with
                |"TEXT"|"T"->OutputFormat.Text
                |"HTML"|"H"->OutputFormat.Html
                |"WEBPAGE"|"W"->OutputFormat.WebPage
                |"JSON"|"J"->OutputFormat.JSON
                |_->OutputFormat.Text
                //raise(new System.ArgumentOutOfRangeException("OutputFormat","The string value provided for Output format doesn't exist in the enum"))
type InputFormat = CGI | JSON | Text with
        static member ToList() =
            [CGI;JSON;Text]
        override self.ToString() =
            match self with
            | CGI->"Cgi"
            | JSON->"Json"
            | Text->"Text"
        static member TryParse(stringToParse:string) =
            match stringToParse.ToUpper() with
                |"CGI"|"C"->true,InputFormat.CGI
                |"JSON"|"J"->true,InputFormat.JSON
                |"TEXT"|"T"->true,InputFormat.Text
                |_->(false, InputFormat.Text)
        static member Parse(stringToParse:string) =
            match stringToParse.ToUpper() with
                |"TEXT"|"T"->InputFormat.Text
                |"CGI"|"C"->InputFormat.CGI
                |"JSON"|"J"->InputFormat.JSON
                |_->InputFormat.Text
                //raise(new System.ArgumentOutOfRangeException("InputFormat","The string value provided for Output format doesn't exist in the enum"))

/// Parameterized type to allow command-line argument processing without a lot of extra coder work
/// Instantiate the type with the type of value you want. Make a default entry in case nothing is found
/// Then call the populate method. Will pull from args and return a val and args with the found value (if any consumed)
type ConfigEntry<'A> =
    {
        commandLineParameterSymbol:string
        commandLineParameterName:string
        parameterHelpText:string[]
        parameterValue:'A
    } with
        member this.printVal =
            printfn "%s: %s" this.commandLineParameterName (this.parameterValue.ToString())
        member this.printHelp =
            printfn "%s" this.commandLineParameterName
            this.parameterHelpText |> Seq.iter(System.Console.WriteLine)
        member this.swapInNewValue x =
            {this with parameterValue=x}
        static member populateValueFromCommandLine ((defaultConfig:ConfigEntry<Verbosity>), (args:string[])):ConfigEntry<Verbosity>  =
            let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
            if parmValue.IsSome
                then
                    let parsedNumValue = System.Int32.Parse("0" + parmValue.Value)
                    let parsedVerbosityValue = enum<Verbosity>(parsedNumValue)
                    defaultConfig.swapInNewValue parsedVerbosityValue
                else
                    defaultConfig
        static member populateValueFromCommandLine ((defaultConfig:ConfigEntry<string>), (args:string[])):ConfigEntry<string>  =
            let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
            if parmValue.IsSome
                then
                    defaultConfig.swapInNewValue parmValue.Value
                else
                    defaultConfig
        static member populateValueFromCommandLine ((defaultConfig:ConfigEntry<DirectoryParm>), (args:string[])):ConfigEntry<DirectoryParm>  =
            let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
            if parmValue.IsSome
                then
                    if System.IO.Directory.Exists(parmValue.Value)
                        then 
                            let tempDirectoryInfo = Some(System.IO.DirectoryInfo(parmValue.Value))
                            defaultConfig.swapInNewValue (parmValue.Value, tempDirectoryInfo)
                        else defaultConfig.swapInNewValue (parmValue.Value, Option.None)
                else
                    defaultConfig
        static member populateValueFromCommandLine ((defaultConfig:ConfigEntry<FileParm>), (args:string[])):ConfigEntry<FileParm>  =
            let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
            if parmValue.IsSome
                then
                    if System.IO.File.Exists(parmValue.Value)
                        then
                            let tempFileInfo = Some(System.IO.FileInfo(parmValue.Value))
                            defaultConfig.swapInNewValue (parmValue.Value, tempFileInfo)
                        else
                            defaultConfig.swapInNewValue (parmValue.Value, Option.None)
                else
                    defaultConfig
        static member populateValueFromCommandLine ((defaultConfig:ConfigEntry<bool>), (args:string[])):ConfigEntry<bool> =
            let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
            if parmValue.IsSome
                then
                    if parmValue.Value.ToUpper() = "FALSE" || parmValue.Value = "0" || parmValue.Value.ToUpper() = "F" || parmValue.Value.ToUpper() = "NO"
                        then
                            defaultConfig.swapInNewValue false
                        else
                            defaultConfig.swapInNewValue true
                else
                    defaultConfig
        static member populateValueFromCommandLine ((defaultConfig:ConfigEntry<int>), (args:string[])):ConfigEntry<int>  =
            let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
            if parmValue.IsSome
                then
                    let parmInt = System.Int32.Parse("0" + parmValue.Value)
                    defaultConfig.swapInNewValue parmInt
                else
                    defaultConfig
        static member populateValueFromCommandLine ((defaultConfig:ConfigEntry<System.Uri>), (args:string[])):ConfigEntry<System.Uri>  =
            let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
            if parmValue.IsSome
                then
                    defaultConfig.swapInNewValue (new System.Uri(parmValue.Value))
                else
                    defaultConfig
        static member populateValueFromCommandLine ((defaultConfig:ConfigEntry<System.DateTime>), (args:string[])):ConfigEntry<System.DateTime>  =
            let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
            if parmValue.IsSome
                then
                    defaultConfig.swapInNewValue (System.DateTime.Parse(parmValue.Value))
                else
                    defaultConfig
        static member populateValueFromCommandLine ((defaultConfig:ConfigEntry<SortOrder>), (args:string[])):ConfigEntry<SortOrder>  =
            let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
            let newVal=if parmValue.IsNone then defaultConfig.parameterValue else
                        let tp=SortOrder.TryParse parmValue.Value
                        if fst tp=true then snd tp else defaultConfig.parameterValue
            defaultConfig.swapInNewValue newVal
        static member populateValueFromCommandLine ((defaultConfig:ConfigEntry<OutputFormat>), (args:string[])):ConfigEntry<OutputFormat>  =
            let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
            let newVal=if parmValue.IsNone then defaultConfig.parameterValue else
                        let tp=OutputFormat.TryParse parmValue.Value
                        if fst tp=true then snd tp else defaultConfig.parameterValue
            defaultConfig.swapInNewValue newVal
        static member populateValueFromCommandLine ((defaultConfig:ConfigEntry<InputFormat>), (args:string[])):ConfigEntry<InputFormat>  =
            let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
            let newVal=if parmValue.IsNone then defaultConfig.parameterValue else
                        let tp=InputFormat.TryParse parmValue.Value
                        if fst tp=true then snd tp else defaultConfig.parameterValue
            defaultConfig.swapInNewValue newVal
/// A type so that programs can report what they're doing as they do it
// This was the programmer can decide what to do with it instead of the OS
[<NoComparison>]
type InterimProgress =
    {
        items:System.Collections.Generic.Dictionary<string, System.Text.StringBuilder>
    } with
    member this.addItem key (vl:string) =
        let lookup = 
            if this.items.ContainsKey key then this.items.Item(key)
                else
                    let newItem = new System.Text.StringBuilder(65535)
                    this.items.Add(key,newItem)
                    newItem
        lookup.Append("\r\n" + vl) |> ignore
    member this.getItem key  =
        if this.items.ContainsKey key
            then
                this.items.Item(key).ToString()
            else
                ""
// All programs have at least this configuration on the command line
[<NoComparison>]
type ConfigBase =
    {
        programName:string
        programTagLine:string
        programHelpText:string[]
        verbose:ConfigEntry<Verbosity>
        interimProgress:InterimProgress
    }
    member this.printProgramDescription =
        this.programHelpText |> Seq.iter(System.Console.WriteLine)
    member this.printThis =
        printfn "%s" this.programName
        this.programHelpText |> Seq.iter(System.Console.WriteLine)
/// Command-line parameters for this particular (easyam) program
[<NoComparison>]
type OptionExampleProgramConfig =
    {
        configBase:ConfigBase
        //inputFile:ConfigEntry<FileParm>
        inputFormat:InputFormat
        outputFormat:OutputFormat
    }
    member this.printThis() =
        printfn "EasyAMConfig Parameters Provided"
        this.configBase.verbose.printVal
        //this.inputFile.printVal
        printfn "%s: %s" "InputFormat : %s" (string this.inputFormat)
        printfn "%s: %s" "OutputFormat : %s" (string this.outputFormat)
