module AppTests
open SystemTypes
open SystemUtils
open AppTypes
open AppUtils
open NUnit.Framework

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
let ``OptionExampleFileLines: Initial test file``() = 
    let ret=OptionExampleFileLines.FromStrings initialTestFile |> Seq.toArray
    Assert.AreEqual(7, ret.Length)
    Assert.AreEqual({Name="a";Number=9}, ret.[0])
    Assert.AreEqual({Name="a";Number=9}, ret.[6])
[<Test>]
let ``groupAndSum: Initial test file``() = 
    let input=OptionExampleFileLines.FromStrings initialTestFile |> Seq.toArray
    let ret=groupAndSum input |> Seq.toArray
    Assert.AreEqual(3, ret.Length)
    Assert.AreEqual({Name="a";Number=22}, ret.[0])
    Assert.AreEqual({Name="b";Number=12}, ret.[1])
    Assert.AreEqual({Name="c";Number=21}, ret.[2])
[<Test>]
let ``NameNumberPairType: ToHtml``() = 
    let input=OptionExampleFileLines.FromStrings initialTestFile |> Seq.toArray
    let ret=groupAndSum input |> Seq.toArray
    Assert.AreEqual
        ("<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>\
        a</span>=<span class='NameNumberPairTypeItemNumber'>22</span></div>"
        , ret.[0].ToHtml())
[<Test>]
let ``OptionExampleFileLines: ToHtml``() = 
    let input=OptionExampleFileLines.FromStrings initialTestFile |> Seq.toArray
    let ret=groupAndSum input |> Seq.toArray
    Assert.AreEqual
        ("<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>\
        a</span>=<span class='NameNumberPairTypeItemNumber'>22</span></div>"
        , ret.ToHtml())


