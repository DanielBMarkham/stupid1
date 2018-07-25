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
        
[<Test>]
let ``Parse Sample CGI Input``()=
    let sampleStream=["myInput=a%3D9%0D%0Ab%3D4%0D%0Ac%3D10%0D%0Aa%3D3&blub=dfg+asdf"]
    let procInput=processCGIStream sampleStream |>Seq.toArray
    Assert.AreEqual(2, (procInput |> Seq.length))
    Assert.AreEqual("myInput", procInput.[0].Key)
    Assert.AreEqual("a=9\r\nb=4\r\nc=10\r\na=3", procInput.[0].Value)
    Assert.AreEqual("blub", procInput.[1].Key)
    Assert.AreEqual("dfg asdf", procInput.[1].Value)