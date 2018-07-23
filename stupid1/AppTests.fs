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
    let ret=OptionExampleFileLinesType.FromStrings initialTestFile |> Seq.toArray
    Assert.AreEqual(7, ret.Length)
    Assert.AreEqual("a=9", ret.[0].ToString())
    Assert.AreEqual("a=9", ret.[6].ToString())
[<Test>]
let ``groupAndSum: Initial test file``() = 
    let input=OptionExampleFileLinesType.FromStrings initialTestFile |> Seq.toArray
    let ret=groupAndSum input |> Seq.toArray
    Assert.AreEqual(3, ret.Length)
    Assert.AreEqual("a=22", ret.[0].ToString())
    Assert.AreEqual("b=12", ret.[1].ToString())
    Assert.AreEqual("c=21", ret.[2].ToString())
[<Test>]
let ``NameNumberPairType: ToHtml``() = 
    let input=OptionExampleFileLinesType.FromStrings initialTestFile |> Seq.toArray
    let ret=groupAndSum input |> Seq.toArray
    let retHtml = ret.[0].ToHtml()
    let expectedString=
        match MajorOSType with
            |Windows->"<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>a</span>=<span class='NameNumberPairTypeItemNumber'>22</span></div>"
            |Linux->"<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>a</span>=<span class='NameNumberPairTypeItemNumber'>22</span></div>"
    Assert.AreEqual(expectedString, retHtml)
[<Test>]
let ``OptionExampleFileLines: ToHtml``() = 
    let input=OptionExampleFileLinesType.FromStrings initialTestFile |> Seq.toArray
    let ret=groupAndSum input |>OptionExampleFileLinesType.FromSeq
    let retHtml = ret.ToHtml()
    let expectedString=
        match MajorOSType with
            |Windows->"<div class='OptionExampleFileLines'><ul class='OptionExampleFileLinesList'>\r\n<li class='NameNumberPairTypeListItem'>\r\n<li class='OptionExampleFileLinesItem'>\r\n<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>a</span>=<span class='NameNumberPairTypeItemNumber'>22</span></div></li>\r\n\r\n</li>\r\n<li class='NameNumberPairTypeListItem'>\r\n<li class='OptionExampleFileLinesItem'>\r\n<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>b</span>=<span class='NameNumberPairTypeItemNumber'>12</span></div></li>\r\n\r\n</li>\r\n<li class='NameNumberPairTypeListItem'>\r\n<li class='OptionExampleFileLinesItem'>\r\n<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>c</span>=<span class='NameNumberPairTypeItemNumber'>21</span></div></li>\r\n\r\n</li>\r\n</ul></div>\r\n"
            |Linux->"<div class='OptionExampleFileLines'><ul class='OptionExampleFileLinesList'>\n<li class='NameNumberPairTypeListItem'>\n<li class='OptionExampleFileLinesItem'>\n<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>a</span>=<span class='NameNumberPairTypeItemNumber'>22</span></div></li>\n</li>\n<li class='NameNumberPairTypeListItem'>\n<li class='OptionExampleFileLinesItem'>\n<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>b</span>=<span class='NameNumberPairTypeItemNumber'>12</span></div></li>\n</li>\n<li class='NameNumberPairTypeListItem'>\n<li class='OptionExampleFileLinesItem'>\n<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>c</span>=<span class='NameNumberPairTypeItemNumber'>21</span></div></li>\n</li>\n</ul></div>\n"
    Assert.AreEqual(expectedString, retHtml)


