module SystemTests
open SystemTypes
open SystemUtils

open NUnit.Framework

/// Create a dummy file in the OS and return a .NET FileInfo object. Used as a mock for testing
let getFakeFileName() = 
    let tempColl = (new System.CodeDom.Compiler.TempFileCollection(System.AppDomain.CurrentDomain.BaseDirectory, false))
    tempColl.AddExtension("fakefileextension") |> ignore
    let rndPrefix = System.IO.Path.GetRandomFileName()
    let tempFileName = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, (rndPrefix + "_tester.bsx"))
    tempFileName
[<Test>]
let ``FILE: ReadAllNameValueLines with bad file returns empty sequence``() = 
    let badFileName = getFakeFileName()
    let ret = System.IO.File.ReadAllNameValueLines badFileName
    Assert.AreEqual(0, (ret |> Seq.length))
[<Test>]
let ``convertLinesIfPossibleToKVPair: Empty sequence returns empty sequence``() = 
    let ret=convertLinesIfPossibleToKVPair Seq.empty
    Assert.AreEqual(0, (ret |> Seq.length))
let initialTestFile = 
    seq [
        "a=9"
        ;"b=8"
        ;"c=10"
        ;"a=4"
        ;"b=4"
        ;"c=11"
        ;"#$%^"
        ;""
        ;"a=9"
        ;"   "
        ;"   asdf"
        ]
[<Test>]
let ``convertLinesIfPossibleToKVPair: Ignore bad lines``() = 
    let ret= convertLinesIfPossibleToKVPair initialTestFile
    Assert.AreEqual(7, (ret |> Seq.length))
        
