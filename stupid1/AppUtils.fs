module AppUtils
open SystemTypes
open SystemUtils
open AppTypes

let outputStuff (opts:OptionExampleProgramConfig) (outData:OptionExampleFileLinesType) =
    let outputText:string =
        match opts.outputFormat with
            |Html->outData.ToHtml()
            |WebPage->
                let webserverReturnPage = 
                    wrapFragmentIntoAnHtmlPageWebServerReturnString 
                        "MyPage"
                        "main.css"
                        "main.js"
                        (outData.ToHtml())
                webserverReturnPage
            |Text->(string outData)
    System.Console.WriteLine outputText


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
    let optionLines = OptionExampleFileLinesType.FromStringKVCollection keyValuesFromFile
    let processedLines = optionLines.groupAndSum |> OptionExampleFileLinesType.FromSeq
    outputStuff opts processedLines

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


