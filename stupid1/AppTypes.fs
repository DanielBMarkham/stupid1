module AppTypes
open SystemTypes
open SystemUtils

type NameNumberPairType = { Name:string;Number:int} with
        override self.ToString() =
        self.Name + "=" + self.Number.ToString()
        static member FromString (s:string) = 
            let split=splitLineIfPossibleIntoTwoPieces '=' s
            if split.IsNone then None
            else
                let tryParseInt = tryParseGeneric<int> (snd split.Value)
                if tryParseInt.IsNone then None
                else Some {Name=fst split.Value; Number=tryParseInt.Value}
        member self.ToKVPair = 
            System.Collections.Generic.KeyValuePair<string,int>(self.Name, self.Number)
        static member FromKVPair 
            (pair:System.Collections.Generic.KeyValuePair<string,int>) =
            {Name=pair.Key;Number=pair.Value}
type OptionExampleFileLine = private NameNumberPair of NameNumberPairType with
    static member FromKVPairString (kv:System.Collections.Generic.KeyValuePair<string,int>) =
        NameNumberPairType.FromKVPair kv
    static member FromNameAndNumber (name:string) (number:int) =
        {Name=name; Number=number}
type OptionExampleFileLines = private OptionExampleFileLines of OptionExampleFileLine[] with
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
                        |> Seq.map(fun x->OptionExampleFileLine.FromNameAndNumber
                                            x.Key (snd (tryParsingValueIntoAnInteger x)))
        optionLines
    static member FromTypedCollection 
        (keyValueCollection:System.Collections.Generic.KeyValuePair<string,int> seq) =
            keyValueCollection |> Seq.map(fun x->NameNumberPairType.FromKVPair x)
    static member FromStrings (strings:seq<string>) =
        strings |> Seq.map(fun x-> NameNumberPairType.FromString x) |> Seq.choose id


