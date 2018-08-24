module AppTypes
open SystemTypes
open SystemUtils
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open Newtonsoft.Json.Serialization
open Newtonsoft.Json.Linq


type NameIntPairType = {Name:string;Number:int}
// type shim
type NameIntPairTypeJson = {Name:string;Number:string} with
    member self.ToTextLine() = self.Name + "=" + self.Number + OSNewLine
type OptionExampleFileLineType = 
    private {[<JsonPropertyAttribute>] NameIntPair:NameIntPairType} with
    static member FromKVPairString (kv:System.Collections.Generic.KeyValuePair<string,int>) =
        {NameIntPair={Name=kv.Key;Number=kv.Value}}
    static member FromNameAndNumber (name:string) (number:int) =
        {NameIntPair={Name=name; Number=number}}
    static member FromKVPair 
        (pair:System.Collections.Generic.KeyValuePair<string,int>) =
        {NameIntPair={Name=pair.Key;Number=pair.Value}}
    static member FromString (s:string) = 
        let split=splitLineIfPossibleIntoTwoPieces '=' s
        if split.IsNone then None
        else
            let tryParseInt = tryParseGeneric<int> (snd split.Value)
            if tryParseInt.IsNone then None
            else Some {NameIntPair={Name=fst split.Value; Number=tryParseInt.Value}}
    static member FromJson (s:string) = 
        try 
            let newJsonIncomingObj
                =Newtonsoft.Json.JsonConvert.DeserializeObject<NameIntPairTypeJson>(s)
            let tryParseInt = tryParseGeneric<int> (newJsonIncomingObj.Number)
            let name=newJsonIncomingObj.Name
            if tryParseInt.IsNone then None
            else 
                let value=tryParseInt.Value
                let ret=OptionExampleFileLineType.FromNameAndNumber name value
                Some ret
        with |_ ->None
    member self.ToJson()=
        Newtonsoft.Json.JsonConvert.SerializeObject(self.NameIntPair,Newtonsoft.Json.Formatting.Indented)    
    override self.ToString() =
        self.NameIntPair.Name + "=" + self.NameIntPair.Number.ToString()
    member self.ToHtml() =
        "<div class='NameNumberPairTypeItem'><span class='NameNumberPairTypeItemName'>"
        + self.NameIntPair.Name + "</span>="
        + "<span class='NameNumberPairTypeItemNumber'>"
        + self.NameIntPair.Number.ToString() + "</span></div>"
    member self.ToKVPair() = 
        System.Collections.Generic.KeyValuePair<string,int>(self.NameIntPair.Name, self.NameIntPair.Number)

// A type shim to decouple from Json as much as possible
// We don't care what's in the Json as much as we care
// that we can process whatever we can and ignore the rest
// MUCH different than strongly-type db-type stuff cf Onion
type OptionExampleFileLineTypeJson={[<JsonPropertyAttribute>] NameIntPair:NameIntPairTypeJson} with
    member self.ToTextLine = self.NameIntPair.ToTextLine
type OptionExampleFileLinesJson ={[<JsonPropertyAttribute>] OptionExampleFileLines:OptionExampleFileLineTypeJson[]}
#nowarn "0067"
type OptionExampleFileLinesType = 
    private {[<JsonPropertyAttribute>] OptionExampleFileLines:OptionExampleFileLineType[]} with
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
        let probablyUsesEscapedPropertyNames(s) =
            let escapedQuoteRegex=new System.Text.RegularExpressions.Regex("\\\"")
            escapedQuoteRegex.IsMatch(s) && ((escapedQuoteRegex.Matches(s).Count%2) =0)
        let webFixedString = 
            if probablyUsesEscapedPropertyNames(s) 
                then s.Replace("\\\\\\\"", "")
                else s
        //let testJsonDat:OptionExampleFileLinesJson = {OptionExampleFileLines=
        //    [|
        //        {NameIntPairJson={Name="a";Number="2"}}
        //        ;{NameIntPair={Name="b";Number="5"}}
        //        ;{NameIntPair={Name="a";Number="5"}}
        //        ;{NameIntPair={Name="b";Number="9"}}
        //    |]}
        let JsonShim=Newtonsoft.Json.JsonConvert.DeserializeObject<OptionExampleFileLinesJson>(webFixedString)
        let JsonStrings = JsonShim.OptionExampleFileLines |> Array.map(fun x->x.ToTextLine())
        OptionExampleFileLinesType.FromStrings(JsonStrings)        
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
    member self.groupAndSum() =
        self.OptionExampleFileLines 
            |> Seq.map(fun x->x.ToKVPair()) 
            |> groupAndSumKV
            |> Seq.map(fun x->
                OptionExampleFileLineType.FromKVPairString 
                    (System.Collections.Generic.KeyValuePair<string,int>(fst x, snd x))
                )
    member self.TEST() = self.OptionExampleFileLines