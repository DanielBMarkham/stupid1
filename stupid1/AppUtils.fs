module AppUtils
open SystemTypes
open SystemUtils
open AppTypes

/// Takes a name=value collection and sums by name
let groupAndSumKV (optionLines:seq<System.Collections.Generic.KeyValuePair<string,int>>) =
    optionLines
    |> Seq.groupBy(fun x->x.Key) 
    |> Seq.sortBy fst
    |> Seq.map(fun x->
        let sumOfGroupedData=snd x |> Seq.sumBy(fun x->x.Value)
        (fst x, sumOfGroupedData)
        )
let groupAndSum (optionLines:seq<NameNumberPairType>) =
    optionLines 
        |> Seq.map(fun x->x.ToKVPair) 
        |> groupAndSumKV
        |> Seq.map(fun x->
            OptionExampleFileLine.FromKVPairString 
                (System.Collections.Generic.KeyValuePair<string,int>(fst x, snd x))
            )
let doStuff (opts:OptionExampleProgramConfig) =
    let fileIsThere =System.IO.File.Exists(fst opts.inputFile.parameterValue)
    let keyValuesFromFile = 
        if fileIsThere = false 
        then 
            Seq.initInfinite (fun _ -> System.Console.ReadLine())
            |> convertLinesIfPossibleToKVPair
        else 
            System.IO.File.ReadAllNameValueLines (fst opts.inputFile.parameterValue)
    // we want kv lines with alpha for key and number for value
    let optionLines = OptionExampleFileLines.FromStringKVCollection keyValuesFromFile
    match opts.outputFormat with
        |Html->printfn "Hey there"
        |_->
            let g1 = groupAndSum optionLines
            g1 |> Seq.iter(fun x->printfn "%s" (string x))


let newMain (argv:string[]) doStuffFunction = 
    try
        let (opts:OptionExampleProgramConfig) = loadConfigFromCommandLine argv                
        commandLinePrintWhileEnter opts.configBase (opts.printThis)
        doStuffFunction opts
        0
    with
        | :? UserNeedsHelp as hex ->
            printfn "%s: %s" defaultBaseOptions.programName hex.Data0
            printfn "========================"
            printfn "Command Line Options:"
            // Manually list program config entries here 
            defaultInputFile.printHelp
            defaultOutputFormat.printHelp
            0
        | :? System.Exception as ex ->
            System.Console.WriteLine ("Program terminated abnormally " + ex.Message)
            System.Console.WriteLine (ex.StackTrace)
            if ex.InnerException = null
                then
                    1
                else
                    System.Console.WriteLine("---   Inner Exception   ---")
                    System.Console.WriteLine (ex.InnerException.Message)
                    System.Console.WriteLine (ex.InnerException.StackTrace)
                    1


