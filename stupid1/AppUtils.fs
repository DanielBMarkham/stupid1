module AppUtils
open SystemTypes
open SystemUtils
open AppTypes



/// Outside of the onion coming in
let inputStuff (opts:OptionExampleProgramConfig)= 
    let stringsToProcess= readStdIn []
    let incomingData = 
        match opts.inputFormat with
            |JSON->OptionExampleFileLinesType.FromJsonStrings stringsToProcess
            |Text->OptionExampleFileLinesType.FromStrings stringsToProcess
            |CGI->
                let cgiVariables=processCGIStream stringsToProcess
                if cgiVariables |> Seq.exists(fun x->x.Key="myInput")
                    then
                        let varListFromCgiForm=
                            let cgiVarList = cgiVariables |> Seq.map(fun x->x.Key, x.Value)
                            let var=snd (cgiVarList |> Seq.find(fun x->fst x="myInput"))
                            var.Split([|OSNewLine|], System.StringSplitOptions.None) 
                        OptionExampleFileLinesType.FromStrings varListFromCgiForm
                    else OptionExampleFileLinesType.FromStrings []
    (opts,incomingData)
/// Outside of the onion going out
let outputStuff ((opts:OptionExampleProgramConfig),(outgoingData:OptionExampleFileLinesType)) =
    let outputText:string =
        match opts.outputFormat with
            |OutputFormat.JSON->outgoingData.ToJson()
            |Html->outgoingData.ToHtml()
            |WebPage->
                let webserverReturnPage = 
                    wrapFragmentIntoAnHtmlPageWebServerReturnString 
                        "MyPage"
                        "main.css"
                        "main.js"
                        (outgoingData.ToHtml())
                webserverReturnPage
            |OutputFormat.Text->(string outgoingData)
    System.Console.WriteLine outputText
    0

/// Mother of all functions where things start and then get factored out
let doStuff ((opts:OptionExampleProgramConfig),(inData:OptionExampleFileLinesType)) =
    let processedLines = inData.groupAndSum |> OptionExampleFileLinesType.FromSeq
    (opts,processedLines)


#nowarn "0067"
let newMain (argv:string[]) doStuffFunction = 
    try
        let opts = loadConfigFromCommandLine argv
        commandLinePrintWhileEnter opts.configBase (opts.printThis)
        opts |> inputStuff |> doStuff |> outputStuff 
    with
        | :? UserNeedsHelp as hex ->
            printfn "%s: %s" defaultBaseOptions.programName hex.Data0
            printfn "========================"
            printfn "Command Line Options:"
            // Manually list program config entries here 
            //defaultInputFile.printHelp
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


