module AppUtils
open SystemTypes
open SystemUtils
open AppTypes

/// Outside of the onion coming in
let inputStuff (opts:OptionExampleProgramConfig):OptionExampleFileLinesType = 
    let fileIsThere =System.IO.File.Exists(fst opts.inputFile.parameterValue)
    let inputStrings:string seq =
        match opts.inputFormat with
            |Stream->
                readStdIn [] |> List.toSeq
            |File-> 
                System.IO.File.ReadAllLines(fst opts.inputFile.parameterValue)
                    |> Array.toSeq
            |CGI->
                // This is specific to the test html harness form and should go away
                let cgiVariables=processCGISteamIntoVariables() 
                                |> Seq.toArray
                if cgiVariables |> Array.exists(fun x->x.Key="myInput")
                    then
                        let var=(cgiVariables 
                            |> Array.find(fun x->x.Key="myInput")).Value
                        var.Split([|OSNewLine|], System.StringSplitOptions.None) 
                            |> Array.toSeq
                    else Seq.empty
    OptionExampleFileLinesType.FromStrings inputStrings

/// Outside of the onion going out
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

/// Mother of all functions where things start and then get factored out
let doStuff (opts:OptionExampleProgramConfig) =
    let incomingData = inputStuff opts 
    let processedLines = incomingData.groupAndSum |> OptionExampleFileLinesType.FromSeq
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
            defaultInputFormat.printHelp
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


