module AppTypes
open SystemTypes
open SystemUtils
open Newtonsoft.Json.Converters

type OptionExampleFileLineType = 
    private { Name:string;Number:int} with
    static member FromKVPairString (kv:System.Collections.Generic.KeyValuePair<string,int>) =
        {Name=kv.Key;Number=kv.Value}
    static member FromNameAndNumber (name:string) (number:int) =
        {Name=name; Number=number}
    static member FromKVPair 
        (pair:System.Collections.Generic.KeyValuePair<string,int>) =
        {Name=pair.Key;Number=pair.Value}
    static member FromString (s:string) = 
        let split=splitLineIfPossibleIntoTwoPieces '=' s
        if split.IsNone then None
        else
            let tryParseInt = tryParseGeneric<int> (snd split.Value)
            if tryParseInt.IsNone then None
            else Some {Name=fst split.Value; Number=tryParseInt.Value}
    static member FromJson (s:string) = 
        try Some(Newtonsoft.Json.JsonConvert.DeserializeObject<OptionExampleFileLineType>(s))
        with |_ ->None
    member self.ToJson()=
        Newtonsoft.Json.JsonConvert.SerializeObject(self,Newtonsoft.Json.Formatting.Indented)    
    override self.ToString() =
        self.Name + "=" + self.Number.ToString()
    member self.ToHtml() =
        "<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>"
        + self.Name + "</span>="
        + "<span class='NameNumberPairTypeItemNumber'>"
        + self.Number.ToString() + "</span></div>"
    member self.ToKVPair = 
        System.Collections.Generic.KeyValuePair<string,int>(self.Name, self.Number)
type OptionExampleFileLinesType = 
    private {OptionExampleFileLines:OptionExampleFileLineType[]} with
    static member FromSeq (lines:seq<OptionExampleFileLineType>)=
        {OptionExampleFileLines=lines|>Seq.toArray}
    static member FromTypedCollection 
        (keyValueCollection:System.Collections.Generic.KeyValuePair<string,int> seq) =
            keyValueCollection 
            |> Seq.map(fun x->OptionExampleFileLineType.FromKVPair x)
            |> Seq.toArray
            |> OptionExampleFileLinesType.FromSeq
    static member FromStringKVCollection 
        (keyValueCollection:System.Collections.Generic.KeyValuePair<string,string> seq) =
        /// Process anything with alpha=number, ignore the rest
        let tryParsingKeyIntoAnInteger 
            (pair:System.Collections.Generic.KeyValuePair<string,string>)
                =System.Int32.TryParse pair.Key
        let tryParsingValueIntoAnInteger 
            (pair:System.Collections.Generic.KeyValuePair<string,string>)
                =System.Int32.TryParse pair.Value
        let doesKVWorkForUs (pair:System.Collections.Generic.KeyValuePair<string,string>) =
            (fst (tryParsingKeyIntoAnInteger pair) = false)
            && (fst (tryParsingValueIntoAnInteger pair) = true)
        let optionLines = keyValueCollection 
                        |> Seq.filter(fun x->doesKVWorkForUs x)
                        |> Seq.map(fun x->OptionExampleFileLineType.FromNameAndNumber
                                            x.Key (snd (tryParsingValueIntoAnInteger x)))
                        |> Seq.toArray
                        |> OptionExampleFileLinesType.FromSeq
        optionLines
    static member FromStrings (strings:seq<string>) =
        strings |> Seq.map(fun x-> OptionExampleFileLineType.FromString x) |> Seq.choose id |> Seq.toArray |> OptionExampleFileLinesType.FromSeq
    static member FromJsonString (s:string) =
        try Newtonsoft.Json.JsonConvert.DeserializeObject<OptionExampleFileLinesType>(s)
        with |_ ->{OptionExampleFileLines=[||]}
    static member FromJsonStrings (strings:seq<string>) =
        OptionExampleFileLinesType.FromJsonString(
            strings |> String.concat "")
    member self.ToJson()=
        Newtonsoft.Json.JsonConvert.SerializeObject(self,Newtonsoft.Json.Formatting.Indented)    
    override self.ToString() =
        self.OptionExampleFileLines |> Array.map(fun x->string x) |> String.concat OSNewLine
    member self.ToHtml() =
        let makeItemIntoHtmlLIItem (item:OptionExampleFileLineType) =
            "<li class='OptionExampleFileLinesItem'>" + OSNewLine
            + item.ToHtml()
            + "</li>" + OSNewLine
        let makeItemsIntoHtmlItemList (items:OptionExampleFileLineType[]) = 
            items |> Array.map(fun x->
                (makeItemIntoHtmlLIItem x) 
                ) |> String.concat OSNewLine   
        "<div class='OptionExampleFileLines'><ul class='OptionExampleFileLinesList'>" + OSNewLine
        + (makeItemsIntoHtmlItemList self.OptionExampleFileLines)
            + "</ul></div>" + OSNewLine
    member self.groupAndSum =
        self.OptionExampleFileLines 
            |> Seq.map(fun x->x.ToKVPair) 
            |> groupAndSumKV
            |> Seq.map(fun x->
                OptionExampleFileLineType.FromKVPairString 
                    (System.Collections.Generic.KeyValuePair<string,int>(fst x, snd x))
                )
    member self.TEST = self.OptionExampleFileLines

//type OnionProcessing = OptionExampleProgramConfig->OptionExampleFileLinesType->int 
//type OnionOutgoing = OptionExampleProgramConfig->OptionExampleFileLinesType->int 